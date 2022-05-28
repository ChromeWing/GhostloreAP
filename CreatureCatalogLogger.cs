using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace GhostloreAP
{

    public class CreatureCatalog
    {
        public string[] Creatures { get; set; }
    }

    public class CreatureCatalogLogger : IGLAPSingleton
    {
        public static CreatureCatalogLogger instance { 
            get
            {
                if(_inst == null)
                {
                    _inst = new CreatureCatalogLogger();
                }
                return _inst;
            } 
        }

        public static CreatureCatalogLogger _inst;

        public List<Creature> lootableCreatures;
        public List<Creature> creatures;

        private string CatalogPath = Path.Combine(LoadingManager.PersistantDataPath, "creature-catalog.json");

        public CreatureCatalog catalog;


        private string[] validCreatureNames = {
            "Ahool",
            "Babi Ngepet",
            "Ice Jinn",
            "Thunder Jinn",
            "Fire Jinn",
            "E-Gui",
            "Gui-Kia",
            "Hantu Raya",
            "Hantu Tinggi",
            "Jenglot",
            "Jiang-Shi",
            "Komodo Wizard",
            "Salamancer",
            "Orang Minyak",
            "Penanggal",
            "Pocong",
            "Pontianak Tree",
            "Preta",
            "Rafflesia",
            "Rakshasa",
            "Mogui Summoner",
            "Toyol"
        };

        public CreatureCatalogLogger()
        {
        }

        public void Init()
        {
            //GetLootableCreatures();
            GetApprovedCreatures();
            if (File.Exists(CatalogPath))
            {
                catalog = JsonConvert.DeserializeObject<CreatureCatalog>(File.ReadAllText(CatalogPath));
            }
            else
            {
                catalog = CreateNewCatalog();
            }
        }

        private CreatureCatalog CreateNewCatalog()
        {
            CreatureCatalog creatureCatalog = new CreatureCatalog
            {
                Creatures = (from c in creatures select c.CreatureDisplayName).ToArray()
            };
            File.WriteAllText(CatalogPath, JsonConvert.SerializeObject(creatureCatalog, Formatting.Indented));
            return creatureCatalog;

        }
        public List<Creature> GetLootableCreatures()
        {
            if(lootableCreatures != null) { return lootableCreatures; }
            List<Creature> lootableCreatures_ = new List<Creature>();
            var monsters = CreatureManager.instance.CreaturePrefabs;

            foreach (Creature c in monsters.Keys)
            {
                if (c == null || c.LootTable == null || c.CreatureDisplayName == null || c.CreatureDisplayName.Length == 0)
                {
                    continue;
                }
                lootableCreatures_.Add(c);
            }
            lootableCreatures = lootableCreatures_;
            return lootableCreatures_;
        }

        public List<Creature> GetApprovedCreatures()
        {
            if (creatures != null) { return creatures; }
            List<Creature> creatures_ = new List<Creature>();
            var monsters = CreatureManager.instance.CreaturePrefabs;

            for (int c = 0; c < validCreatureNames.Length; c++)
            {
                var mName = validCreatureNames[c];
                foreach(Creature m in monsters.Keys)
                {
                    if (m.CreatureDisplayName == mName)
                    {
                        creatures_.Add(m);
                        break;
                    }
                }
            }
            creatures = creatures_;
            return creatures_;
        }

        public void Cleanup()
        {
            creatures = null;
        }
    }
}
