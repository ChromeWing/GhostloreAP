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
        private bool active = true;

        private readonly int minLootId = 10133000;
        private readonly int maxLootId = 10133022;


        private Dictionary<int, Creature> creatures;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);

            Init();
        }

        private void Init()
        {
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

            active = true;
        }

        public void Cleanup()
        {
            active = false;
            if (this == null) { return; }
            GameObject.Destroy(this.gameObject);
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
                        GLAPLocationManager.instance.MinLevel,
                        GLAPLocationManager.instance.MaxLevel
                    );
                    break;
            }
        }


        public string GetItemReceievedMessage(int item_)
        {
            GLAPNotification.instance.DisplayLog("item is: " + item_);
            switch (item_)
            {
                case var i when i >= minLootId && i <= maxLootId:
                    var creature_ = creatures[i];
                    return string.Format("Loot from {0} showers around you!", creature_.CreatureDisplayName);
                    
            }
            return "Receieved item that we don't support yet :(";
        }


        public async void DropItemsFrom(Creature creature,int count,float minLevel,float maxLevel)
        {
            var player = PlayerManager.instance.GetFirstPlayer();

            await Task.Yield();
            
            for (int i = 0; i < count; i++)
            {
                int itemLevel = (int)Mathf.Lerp(minLevel, maxLevel, ((float)i) / count);
                GLAPModLoader.DebugShowMessage("Dropping item level "+itemLevel);
                if (active)
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
