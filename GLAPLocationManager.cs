using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GhostloreAP
{
    public class GLAPLocationManager : Singleton<GLAPLocationManager>, IGLAPSingleton
    {

        public List<GLAPLocation> locations { get; protected set; }


        private int locationsCleared = 0;
        private int totalLocationCount;

        private int totalMinLevel = 1;
        private int totalMaxLevel = 50;

        private bool registeredQuestEvents = false;

        private int MinLevel
        {
            get { return Math.Max((int)Mathf.Lerp(totalMinLevel, totalMaxLevel, ((float)locationsCleared) / totalLocationCount)-10, totalMinLevel); }
        }
        private int MaxLevel
        {
            get { return (int)(Mathf.Lerp(totalMinLevel, totalMaxLevel, ((float)locationsCleared) / totalLocationCount)); }
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);

            Init();
        }

        private void Init()
        {
            locations = new List<GLAPLocation>();

            CreateLocations();
            QuestFactory.instance.CreateAllAPQuests();
            RandomizeLocations();

            
        }

        public async void StartListeners()
        {
            await Task.Delay(1000);
            if (registeredQuestEvents) { return; }
            GLAPEvents.OnKillQuestCompleted += OnKillQuestCompleted;
            registeredQuestEvents = true;
        }

        public void ShutdownListeners()
        {
            GLAPEvents.OnKillQuestCompleted -= OnKillQuestCompleted;
            registeredQuestEvents = false;
        }

        private void CreateLocations()
        {
            foreach(var c in CreatureCatalogLogger.instance.creatures)
            {
                for(int i = 0; i < 5; i++)
                locations.Add(new GLAPLocation(c, QuestFactory.GetQuestWorkload(MonsterWorkload.QuickPlaythrough,i)));
            }
            locationsCleared = 0;
            totalLocationCount = locations.Count;
        }

        private void RandomizeLocations()
        {
            locations.Shuffle();
        }

        public void Cleanup()
        {
            locations.Clear();
            GLAPEvents.OnKillQuestCompleted -= OnKillQuestCompleted;
            registeredQuestEvents = false;
            if (this != null)
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        private void OnKillQuestCompleted(QuestInstance qi)
        {
            GLAPModLoader.DebugShowMessage("In OnKillQuestCompleted");
            if(locations.Count == 0) { return; }
            GLAPModLoader.DebugShowMessage("about to drop items in OnKillQuestCompleted");
            GLAPItemGiver.instance.DropItemsFrom(locations[0].creature, locations[0].count,MinLevel,MaxLevel);
            locations.RemoveAt(0);
            locationsCleared++;
        }

    }

    public struct GLAPLocation
    {
        public Creature creature;
        public int count;
        public GLAPLocation(Creature creature_,int count_)
        {
            creature = creature_;
            count = count_;
        }
    }
}
