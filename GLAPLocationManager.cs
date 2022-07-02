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
            Creature shopPlaceholder = null;
            foreach(var c in CreatureCatalogLogger.instance.creatures)
            {
                if (shopPlaceholder == null) { shopPlaceholder = c; }
                for(int i = 0; i < GLAPSettings.killQuestsPerMonster; i++)
                locations.Add(new GLAPLocation(c, QuestFactory.GetQuestWorkload(c,GLAPSettings.workload,i)));
            }

            for(int i = 0; i < 20; i++)
            {
                locations.Add(new GLAPLocation(shopPlaceholder, 10));
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

        private void OnKillQuestCompleted(XQuestRequirement xqr)
        {
            if(locations.Count == 0) { return; }

            GLAPClient.tryInstance?.CompleteKillQuestAsync(xqr.locationName);
        }

        public void CompleteShopCheck(int slot)
        {
            GLAPClient.tryInstance?.CompleteShopCheckAsync(slot);
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
