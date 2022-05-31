using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(NPCCraftingRecipeTrader), "GetRecipeInstance")]
    public class NPCCraftingRecipeTraderPatcher
    {
        public class FoodCatalog
        {
            public string[] Foods { get; set; }
        }

        private static FoodCatalog foodCatalog = null;

        private static string CatalogPath = Path.Combine(LoadingManager.PersistantDataPath, "datamineFood.json");
        static void Postfix(CraftingRecipe[] ___craftingRecipePool)
        {
            if(foodCatalog != null) { return; }
            foodCatalog = new FoodCatalog
            {
                Foods = (from c in ___craftingRecipePool select c.CraftingRecipeName).ToArray()
            };
            File.WriteAllText(CatalogPath, JsonConvert.SerializeObject(foodCatalog, Formatting.Indented));
            
        }
    }
}
