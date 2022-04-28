using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

/* TODO:
 * postfix NPCTrader.CheckInventory() to setup the archipelago itemshop.
 * postfix InventoryPanel.ReceiveDraggedItem() (the one that returns a bool) in order to detect a transaction occurred for a particular itemInstance in the archipelago shop.
 * ^^ you will want to grab the ItemData field inside itemInstance (Item type)
 * generate ExtendedItem extendedbinding for the Items in the archipelago shop in order to see who it's for.
 * 
 * grab LevelUpPanel with FindObjectByType, and store a reference to the animator in an extendedbinding.
 * ^^have the extendedbinding know when the animator is currently animating, so that it knows when to go in and display the notification for an item/check.
 * 
 */

namespace GhostloreAP
{
    public class GLAPModLoader : IModLoader
    {
        private static TestTextDrawer debugMsg;

        private static Harmony harmony;

        private bool active;

        private List<IGLAPSingleton> singletons;


        public void OnCreated()
        {
            DebugShowMessage("Huh?");
            singletons = new List<IGLAPSingleton>();
            InitSingletons();

            DebugShowMessage("Huh??");

            harmony = new Harmony("com.chromewing.ghostlore-archipelago");
            harmony.PatchAll();

            DebugShowMessage("Huh????");
        }

        private void InitSingletons()
        {
            CreatureCatalogLogger.instance.Init();
            ExtendedBindingManager.EnsureExists();
            GLAPLocationManager.EnsureExists();
            GLAPItemGiver.EnsureExists();


            singletons.Add(ExtendedBindingManager.instance);
            singletons.Add(GLAPLocationManager.instance);
            singletons.Add(GLAPItemGiver.instance);
            singletons.Add(QuestFactory.instance);
            singletons.Add(CreatureCatalogLogger.instance);
        }

        private void DisposeSingletons()
        {
            foreach(var singleton in singletons)
            {
                if(singleton != null)
                singleton.Cleanup();
            }
            singletons.Clear();
        }

        public void OnGameLoaded(LoadMode mode)
        {
            active = true;
            GLAPLocationManager.instance.StartListeners();
            WelcomePlayer();
            
        }

        public void OnGameUnloaded()
        {
            GLAPLocationManager.instance.ShutdownListeners();
            Shutdown();
        }

        public void OnReleased()
        {
            harmony.UnpatchAll();

            DisposeSingletons();
            Shutdown();
        }

        private void Shutdown()
        {
            active = false;

        }

        private async void WelcomePlayer()
        {
           
            await DisplayMessageAsyncRoutine("Welcome to Archipelago!");



        }

        public static void DebugShowMessage(string msg_)
        {
            if(debugMsg == null)
            {
                var go = new GameObject("Archipelago");
                debugMsg = go.AddComponent<TestTextDrawer>();
            }
            debugMsg.DisplayMessage(msg_); 
        }

        private async Task DisplayMessageAsyncRoutine(string msg_)
        {
            await Task.Delay(1000);
            while (TimeManager.instance.IsPaused())
            {
                await Task.Yield();
            }
            await Task.Delay(500);
            //DebugShowMessage("Welcome!");



        }

        
    }
}
