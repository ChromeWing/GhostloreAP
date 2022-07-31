using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    public class RecipeStorage : Singleton<RecipeStorage>, IGLAPSingleton
    {
        private List<CraftingRecipe> _recipes = new List<CraftingRecipe>();

        public void Cleanup()
        {
            _recipes.Clear();
        }

        public void ProvideAvailableRecipes(CraftingRecipe[] r,System.Action<CraftingRecipe> cb)
        {
            _recipes = r.ToList();
            System.Random rng_ = new System.Random(GLAPClient.instance.Seed);
            //GLAPModLoader.DebugShowMessage("_recipes.Count="+_recipes.Count);
            while(_recipes.Count > 20)
            {
                _recipes.RemoveAt((int)(rng_.NextDouble()*_recipes.Count));
            }
            //GLAPModLoader.DebugShowMessage("_recipes.Count=" + _recipes.Count);
            for (int i = 19; i >= 0; i--)
            {
                if (!(GLAPClient.tryInstance.Connected && GLAPClient.instance.RecipeOwned(i)))
                {
                    _recipes.RemoveAt(i);
                }
            }
            for(int i=0; i < _recipes.Count; i++)
            {
                cb(_recipes[i]);
            }
        }


    }

    [HarmonyPatch(typeof(NPCCraftingRecipeTrader), nameof(NPCCraftingRecipeTrader.CheckRecipes))]
    public class RecipePatcher
    {
        static void Postfix(ref List<CraftingRecipeInstance> ___recipes, ref CraftingRecipe[] ___craftingRecipePool)
        {
            ___recipes.Clear();
            List<CraftingRecipeInstance> insts_ = new List<CraftingRecipeInstance>();
            RecipeStorage.instance.ProvideAvailableRecipes(___craftingRecipePool, (r) =>
            {
                //GLAPModLoader.DebugShowMessage("recipe recieved");
                insts_.Add(new CraftingRecipeInstance(r));
            });

            for(int i = 0; i < insts_.Count; i++)
            {
                ___recipes.Add(insts_[i]);
            }
            //GLAPModLoader.DebugShowMessage("insts_.Count="+insts_.Count+", ___recipes.Count="+___recipes.Count);
        }
    }
}
