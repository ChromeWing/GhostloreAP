using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Archipelago.MultiClient.Net;
using WebSocketSharp;

namespace GhostloreAP
{
    public class GLAPClient : Singleton<GLAPClient>, IGLAPSingleton
    {

        private ArchipelagoSession _session;

        private readonly string GAMENAME = "Ghostlore";
        private readonly string SHOPNAME = "Link Bracelet #{0}";

        public void Cleanup()
        {

        }

        public void Connect(string slotName_,string ip_="localhost")
        {

            _session = ArchipelagoSessionFactory.CreateSession(ip_, 38281);

            _session.TryConnectAndLogin("Ghostlore", slotName_, new Version(0, 3, 3), Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems);
            GLAPNotification.instance.DisplayMessage(slotName_ + " Connected to Archipelago!");

            ItemFactory.instance.CacheShopItemNames();
        }

        public void CompleteShopCheckAsync(int index)
        {
            _session.Locations.CompleteLocationChecksAsync((success) =>
            {
                if (success)
                {
                    GLAPNotification.instance.DisplayLog("WE COMPLETED CHECK ON BUYING!");
                }
                else
                {
                    GLAPNotification.instance.DisplayLog("complete shop check failed...");
                }
            }, GetShopLocation(index));
        }

        public bool ShopAlreadyChecked(int index)
        {
            return LocationAlreadyChecked(GetShopLocation(index));
        }

        public long GetShopLocation(int index)
        {
            GLAPNotification.instance.DisplayLog(string.Format(SHOPNAME, index + 1));
            return _session.Locations.GetLocationIdFromName(GAMENAME, string.Format(SHOPNAME, index + 1));
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
