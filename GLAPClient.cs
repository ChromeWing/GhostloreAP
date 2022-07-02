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
                if(instance._session == null || !instance._session.Socket.Connected) { return null; }
                return instance;
            }
        }

        public bool Connected { get
            {
                return _session != null && _session.Socket.Connected;
            } }

        private ArchipelagoSession _session;

        private bool _connectionPacketReceieved = false;

        
        
        private string _seed;

        private LoginResult _loginResult;

        private readonly string GAMENAME = "Ghostlore";
        private readonly string SHOPNAME = "Link Bracelet #{0}";

        public void Cleanup()
        {
            _connectionPacketReceieved = false;
            _session = null;
            _loginResult = null;
        }

        public async Task Connect(string slotName_,string ip_="localhost")
        {

            _session = ArchipelagoSessionFactory.CreateSession(ip_, 38281);

            try
            {
                Initialize();
                _loginResult = _session.TryConnectAndLogin("Ghostlore", slotName_, new Version(0, 3, 3), Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems);
                

                if (_session.Socket.Connected)
                {
                    GLAPNotification.instance.DisplayMessage(slotName_ + " connected to Archipelago!");
                    GLAPNotification.instance.DisplayMessage("Welcome!");
                    while (!_connectionPacketReceieved)
                    {
                        await Task.Yield();
                    }

                }
                else
                {
                    GLAPNotification.instance.DisplayMessage(slotName_ + " failed to connect to Archipelago...");
                    GLAPNotification.instance.DisplayMessage("Please visit the Steam Workshop page for this mod for troubleshooting.");
                }

            }
            catch
            {
                Disconnect();
            }

        }

        public void Disconnect()
        {
            CloseConnection();
        }

        private void Initialize()
        {
            _session.Socket.PacketReceived += OnPacketReceived;
        }

        private void CloseConnection()
        {

            GLAPModLoader.DebugShowMessage("CLOSE CONNECTION!");
            _session.Socket.PacketReceived -= OnPacketReceived;
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
            foreach(var d in p.Data)
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

        public long GetShopLocation(int index)
        {
            return _session.Locations.GetLocationIdFromName(GAMENAME, string.Format(SHOPNAME, index + 1));
        }

        public long GetLocationFromName(string name)
        {
            return _session.Locations.GetLocationIdFromName(GAMENAME, name);
        }

        public string GetLocationName(long id)
        {
            return _session.Locations.GetLocationNameFromId(id);
        }

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
