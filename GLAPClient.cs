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

        public void Cleanup()
        {

        }

        public void Connect(string slotName_,string ip_="localhost")
        {

            _session = ArchipelagoSessionFactory.CreateSession(ip_, 38281);

            _session.TryConnectAndLogin("Ghostlore", slotName_, new Version(3, 3, 0), Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems);
            GLAPNotification.instance.DisplayMessage(slotName_ + " Connected to Archipelago!");
            for(int i = 0; i < _session.Locations.AllLocations.Count; i++)
            {
                DebugShow("Loc:"+_session.Locations.GetLocationNameFromId(_session.Locations.AllLocations[i]));
            }
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
