using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Archipelago.MultiClient.Net;
using WebSocketSharp;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;

namespace GhostloreAP
{
    public class GLAPClient : Singleton<GLAPClient>, IGLAPSingleton
    {

        public static GLAPClient tryInstance { get
            {
                if (instance._session == null || !instance._session.Socket.Connected) { return null; }
                return instance;
            }
        }

        public bool Connected { get
            {
                return _session != null && _session.Socket.Connected;
            } }

        public int Seed { get
            {
                if (!Connected) { return 0; }
                return ConvertSeedToInt(_session.RoomState.Seed);
            } }

        public bool SessionMade { get
            {
                return _session != null;
            } }

        private int ConvertSeedToInt(string seed_)
        {
            int result = 0;
            foreach(char ch in seed_)
            {
                result = (result * 256 + (int)ch) % int.MaxValue;
            }
            return result;
        }

        private ArchipelagoSession _session;

        private bool _connectionPacketReceieved = false;



        private string _seed;

        private LoginResult _loginResult;

        private readonly string GAMENAME = "Ghostlore";
        private readonly string SHOPNAME = "Link Bracelet #{0}";

        public void Cleanup()
        {
            _connectionPacketReceieved = false;
            _loginResult = null;
            _session = null;
            _loginResult = null;
        }

        public async Task Connect(GLAPProfile profile)
        {
            await Connect(profile.slot_name,profile.ip,profile.port,profile.password);
        }

        public async Task Connect(string slotName_, string ip_ = "localhost", int port_ = 38281, string password_="", System.Action onSuccess_=null, System.Action onFail_=null)
        {
            GLAPModLoader.DebugShowMessage("stage1");
            //GLAPModLoader.SaveLog();
            _session = ArchipelagoSessionFactory.CreateSession(ip_, port_);
            
            
            if (password_ == "")
            {
                password_ = null;
            }

            try
            {
                GLAPModLoader.DebugShowMessage("stage2");
                Initialize();
                GLAPModLoader.DebugShowMessage("stage3");
                //GLAPModLoader.SaveLog();
                _loginResult = _session.TryConnectAndLogin("Ghostlore", slotName_, new Version(0, 3, 3), Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems,null,null,password_);

                GLAPModLoader.DebugShowMessage("stage4");
                //GLAPModLoader.SaveLog();
                await Task.Yield();
                
                if (_session.Socket.Connected)
                {
                    GLAPModLoader.DebugShowMessage("stage5");
                    //GLAPModLoader.SaveLog();
                    GLAPNotification.instance.DisplayMessage(slotName_ + " connected to Archipelago!");
                    GLAPNotification.instance.DisplayMessage("Welcome!");
                    var timeout_ = Time.fixedTime;
                    while (!_connectionPacketReceieved && Time.fixedTime-timeout_<5)
                    {
                        GLAPModLoader.DebugShowMessage("stage9");
                        await Task.Yield();
                    }
                    if(Time.fixedTime-timeout_ >= 5)
                    {
                        _session = null;
                        onFail_?.Invoke();
                        return;
                    }
                    GLAPModLoader.DebugShowMessage("stage6");
                    //GLAPModLoader.SaveLog();
                    onSuccess_?.Invoke();
                    return;

                }
                else
                {
                    GLAPModLoader.DebugShowMessage("stage7");
                    //GLAPModLoader.SaveLog();
                    GLAPNotification.instance.DisplayMessage(slotName_ + " failed to connect to Archipelago...");
                    GLAPNotification.instance.DisplayMessage("Please visit the Steam Workshop page for this mod for troubleshooting.");
                }
                
            }
            catch
            {
                GLAPModLoader.DebugShowMessage("stage8");
                GLAPModLoader.SaveLog();
                Disconnect();
            }

            onFail_?.Invoke();
            
        }

        public async Task WaitTillConnectionIsMade()
        {
            var timeout_ = Time.fixedTime;
            while (!_connectionPacketReceieved && Time.fixedTime - timeout_ < 5)
            {
                await Task.Yield();
            }
            if (Time.fixedTime - timeout_ >= 5)
            {
                return;
            }
        }

        public void ListenToItems()
        {
            bool queueEmpty_ = false;
            while (!queueEmpty_)
            {
                try
                {
                    var item_ = _session.Items.PeekItem();
                    _session.Items.DequeueItem();
                    ProcessItemReceieved(item_);

                }catch(InvalidOperationException ex)
                {
                    queueEmpty_ = true;
                }
            }
            _session.Items.ItemReceived += OnItemReceieved;
        }

        public void Disconnect()
        {
            CloseConnection();
            Cleanup();
        }

        private void Initialize()
        {
            _session.Socket.PacketReceived += OnPacketReceived;
        }

        private void CloseConnection()
        {
            GLAPModLoader.DebugShowMessage("CLOSE CONNECTION!");
            if (_session == null || _session.Socket==null) { return; }
            _session.Socket.PacketReceived -= OnPacketReceived;

            if (_session.Socket.Connected)
            {
                _session.Socket.Disconnect();
            }
            GLAPModLoader.DebugShowMessage("CLOSE CONNECTION SUCCESS!");
        }

        private void OnPacketReceived(ArchipelagoPacketBase packet_)
        {


            switch (packet_)
            {
                case ConnectedPacket connectedPacket:
                    OnPacketConnected(connectedPacket);
                    _connectionPacketReceieved = true;
                    break;
                case PrintPacket printPacket:
                    OnPacketPrint(printPacket);
                    break;
                case PrintJsonPacket printJsonPacket:
                    OnPacketJsonPrint(printJsonPacket);
                    break;
                case RoomUpdatePacket roomUpdatePacket:
                    OnPacketRoomUpdate(roomUpdatePacket);
                    break;
            }


        }

        private void OnItemReceieved(ReceivedItemsHelper helper)
        {
            var item = helper.PeekItem();
            helper.DequeueItem();
            ProcessItemReceieved(item);
        }

        private void ProcessItemReceieved(NetworkItem item)
        {
            int itemId_ = item.Item;
            long locId_ = item.Location;
            if (GLAPProfileManager.instance.ItemGranted(itemId_, locId_))
            {
                GLAPLocationManager.instance.RefreshLocationCountCleared();
                GLAPNotification.instance.DisplayMessage(GetReceivedItemMessage(itemId_), () =>
                {
                    GLAPItemGiver.instance.GiveItem(itemId_);

                });
            }
        }

        private bool AlreadyRecievedItem(int item_,long location_)
        {
            return false;
        }

        private void OnPacketConnected(ConnectedPacket p)
        {
            GLAPSettings.Set(p.SlotData);
        }

        private void OnPacketPrint(PrintPacket p)
        {
            GLAPNotification.instance.DisplayLog(p.Text);
        }

        private void OnPacketJsonPrint(PrintJsonPacket p)
        {
            var text = new StringBuilder();
            foreach (var d in p.Data)
            {
                switch (d.Type)
                {
                    case JsonMessagePartType.PlayerId:
                        text.Append(GetPlayerName(int.Parse(d.Text)));
                        break;
                    case JsonMessagePartType.ItemId:
                        text.Append(GetItemName(long.Parse(d.Text)));
                        break;
                    case JsonMessagePartType.LocationId:
                        text.Append(GetLocationName(long.Parse(d.Text)));
                        break;
                    default:
                        text.Append(d.Text);
                        break;
                }
            }

            GLAPNotification.instance.DisplayLog(text.ToString());
        }

        private void OnPacketRoomUpdate(RoomUpdatePacket p)
        {

        }

        public void CompleteShopCheckAsync(int index)
        {
            _session.Locations.CompleteLocationChecksAsync((success) =>
            {

            }, GetShopLocation(index));
        }
        public void CompleteChthoniteCheck()
        {
            _session.Locations.CompleteLocationChecksAsync((success) =>
            {

            }, GetQuestChestLocation("Chthonite"));
        }

        public void CompleteAstraliteCheck()
        {
            _session.Locations.CompleteLocationChecksAsync((success) =>
            {

            }, GetQuestChestLocation("Astralite"));
        }


        public void CompleteKillQuestAsync(string locationName_)
        {
            _session.Locations.CompleteLocationChecksAsync((success) =>
            {

            }, GetLocationFromName(locationName_));

        }

        public bool ShopAlreadyChecked(int index)
        {
            return LocationAlreadyChecked(GetShopLocation(index));
        }

        public bool RecipeOwned(int index)
        {
            return _session.Items.AllItemsReceived.Where(i => GetItemName(i.Item) == ("Recipe "+(index+1))).Count() != 0;
        }

        public long GetShopLocation(int index)
        {
            return _session.Locations.GetLocationIdFromName(GAMENAME, string.Format(SHOPNAME, index + 1));
        }

        public long GetQuestChestLocation(string name_)
        {
            GLAPModLoader.DebugShowMessage("completing check for:\"" + string.Format("{0} Chest", name_)+"\"");
            return _session.Locations.GetLocationIdFromName(GAMENAME, string.Format("{0} Chest", name_));
        }

        public long GetLocationFromName(string name)
        {
            return _session.Locations.GetLocationIdFromName(GAMENAME, name);
        }

        public string GetLocationName(long id)
        {
            return _session.Locations.GetLocationNameFromId(id);
        }

        public string GetReceivedItemMessage(int id_) => GLAPItemGiver.instance.GetItemReceievedMessage(id_);
        

        public List<long> GetAllShopLocations()
        {
            List<long> shop_ = new List<long>();
            for (int i = 0; i < 20; i++)
            {
                shop_.Add(GetShopLocation(i));
            }
            return shop_;
        }

        public void GetShopEntryNamesAsync(Action<int,string> cb)
        {
            _session.Locations.ScoutLocationsAsync((info) =>
            {
                for (int i = 0; i < info.Locations.Length; i++) {
                    var ownerName = GetPlayerName(info.Locations[i].Player);
                    var itemName = GetItemName(info.Locations[i].Item);
                    var entryName = string.Format("{0}'s {1}",ownerName,itemName);
                    cb(i,entryName);
                }
            }, false, GetAllShopLocations().ToArray());
        }

        public int GetItemCountReceived(int id_)
        {
            return _session.Items.AllItemsReceived.Where(i=>i.Item==id_).Count();
        }

        public bool HasItem(int id_)
        {
            return GetItemCountReceived(id_) != 0;
        }

        public bool ItemGiven(int id_)
        {
            return _session.Items.AllItemsReceived.Where(i => i.Item == id_ && !GLAPProfileManager.instance.ItemGranted(i.Item, i.Location, true)).Count() > 0;
        }

        public int GetLootItemCountReceived()
        {
            return _session.Items.AllItemsReceived.Where(i => GetItemName(i.Item).Contains("Loot")).Count();
        }

        public string GetPlayerName(int index)
        {
            return _session.Players.GetPlayerName(index);
        }

        public string GetItemName(long id)
        {
            return _session.Items.GetItemName(id);
        }


        public bool LocationAlreadyChecked(long id)
        {
            return _session.Locations.AllLocationsChecked.Contains(id);
        }

        public bool LocationAlreadyChecked(string name_)
        {
            return _session.Locations.AllLocationsChecked.Contains(GetLocationFromName(name_));
        }

        private void DebugLog(string val_)
        {
            GLAPNotification.instance.DisplayLog(val_);
        }
        private void DebugShow(string val_)
        {
            GLAPNotification.instance.DisplayMessage(val_);
        }
    }
}
