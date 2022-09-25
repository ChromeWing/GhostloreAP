using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GhostloreAP
{
    public class GLAPItemGiver : Singleton<GLAPItemGiver>, IGLAPSingleton
    {
        private bool initialized = false;

        private const int minLootId = 10133000;
        private const int maxLootId = 10133035;

        private const int minRecipeId = 10133036;
        private const int maxRecipeId = 10133055;

        private const int chthoniteId = 10133056;
        private const int astraliteId = 10133057;

        private const int coinId = 10133058;


        private Dictionary<int, Creature> creatures;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);

            Init();
        }

        private void Init()
        {
            if (initialized) { return; }
            creatures = new Dictionary<int, Creature>();
            for(int i=0;i< CreatureCatalogLogger.instance.validCreatureNames.Length;i++)
            {
                var name_ = CreatureCatalogLogger.instance.validCreatureNames[i];
                creatures.Add(i+minLootId, CreatureCatalogLogger.instance.GetCreature(name_));
            }

            foreach(var k in creatures.Keys)
            {
                GLAPModLoader.DebugShowMessage(k + ":" + creatures[k]);
            }


            GLAPEvents.OnLoadedIntoNewArea += CheckHasQuestItems;
            GLAPModLoader.DebugShowMessage("Registered OnLoadedIntoNewArea event!!!");
            initialized = true;
        }

        public void Cleanup()
        {
            initialized = false;
            GLAPEvents.OnLoadedIntoNewArea -= CheckHasQuestItems;
            if (this == null) { return; }
            GameObject.Destroy(this.gameObject);
        }


        public void CheckHasQuestItems()
        {
            GLAPModLoader.DebugShowMessage("current location id:"+MapManager.GetActiveLocationID);
            GLAPModLoader.DebugShowMessage("CHECKHASQUESTITEMS!");
            if(GLAPClient.instance.HasItem(chthoniteId) && GLAPClient.instance.ItemGiven(chthoniteId) && !HasQuestItemInInventory("Chthonite"))
            {
                GLAPNotification.instance.DisplayMessage("Your Chthonite followed you here, please pick it up!",()=> { GiveItem(chthoniteId); });
            }
            if (GLAPClient.instance.HasItem(astraliteId) && GLAPClient.instance.ItemGiven(astraliteId) && !HasQuestItemInInventory("Astralite"))
            {
                GLAPNotification.instance.DisplayMessage("Your Astralite followed you here, please pick it up!", () => { GiveItem(astraliteId); });
            }
            GLAPModLoader.SaveLog();
        }

        private bool HasQuestItemInInventory(string name_)
        {
            return PlayerManager.instance.AnyPlayerHasQuestItem(ItemManager.instance.GetItemFromName(name_));
        }

        public void GiveItem(int item_)
        {
            switch (item_)
            {
                case var i when i>=minLootId && i<=maxLootId:
                    var creature_ = creatures[i];
                    DropItemsFrom(
                        creature_, 
                        QuestFactory.GetQuestWorkload(
                            creature_, 
                            GLAPSettings.workload, 
                            GLAPClient.instance.GetItemCountReceived(i) - 1
                        ),
                        GLAPLocationManager.instance.MaxLevel
                    );
                    break;
                case chthoniteId:
                    DropItem("Chthonite");
                    break;
                case astraliteId:
                    DropItem("Astralite");
                    break;
                case coinId:
                    DropCoins(1000);
                    break;
            }
        }


        public string GetItemReceievedMessage(int item_)
        {
            //GLAPNotification.instance.DisplayLog("item is: " + item_);
            switch (item_)
            {
                case var i when i >= minLootId && i <= maxLootId:
                    var creature_ = creatures[i];
                    return string.Format("Loot from {0} showers around you!", creature_.CreatureDisplayName());
                case var i when i >= minRecipeId && i <= maxRecipeId:
                    return "You received a new food recipe at the restaurant!";
                case chthoniteId:
                    return "An important item Chthonite appears in front of you!";
                case astraliteId:
                    return "An important item Astralite appears in front of you!";
                case coinId:
                    return "1000 coins shower around you!";
                    
            }
            return "Receieved item that we don't support yet :(";
        }

        public void DropItem(string name_)
        {
            var player = PlayerManager.instance.GetFirstPlayer();

            Item droppedItem_ = ItemManager.instance.GetItemFromName(name_);

            ItemManager.instance.SpawnItem(droppedItem_, 1, 1, 0, player.transform.position + Vector3.up * .1f, player.transform.position);
        }

        public async void DropCoins(int count_)
        {
            var player = PlayerManager.instance.GetFirstPlayer();
            await Task.Yield();

            for (int i = 0; i < 50; i++)
            {
                ItemManager.instance.SpawnItem(ItemManager.instance.GetItemFromName("Coins"), count_/50, 1, 0, player.transform.position + Vector3.up * .1f, player.transform.position);
                await Task.Delay(25);
            }

            
        }

        public async void DropItemsFrom(Creature creature,int count,float maxLevel)
        {
            var player = PlayerManager.instance.GetFirstPlayer();

            await Task.Yield();
            
            for (int i = 0; i < count; i++)
            {
                int itemLevel = (int)maxLevel;
                GLAPModLoader.DebugShowMessage("Dropping item level "+itemLevel);
                if (initialized)
                {
                    //NOTE: we are going to stick with player level to determine item drop.  If the player indicates they want async runs, I could use async levels instead (based on progress)
                    switch (GLAPSettings.itemLevelType)
                    {
                        case ItemLevelType.TiedToCharacterLevel:
                            creature.LootTable.TrySpawnLoot(player, player.Level, player.transform.position + Vector3.up * .1f, player.transform.position);
                            break;
                        case ItemLevelType.TiedToProgression:
                            creature.LootTable.TrySpawnLoot(player, itemLevel, player.transform.position + Vector3.up * .1f, player.transform.position);
                            break;
                    }
                }
                else
                {
                    return;
                }
                //DebugShowMessage("step6");
                await Task.Delay(100);
            }
            await Task.Delay(5000);
            
        }

    }
}
