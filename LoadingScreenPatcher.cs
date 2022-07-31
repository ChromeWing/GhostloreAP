using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GhostloreAP
{
    [HarmonyPatch(typeof(LoadingScreen),nameof(LoadingScreen.OnClick))]
    public class LoadingScreenPatcher
    {
        static void Postfix(bool ___loadingComplete, bool ___queuedContinue)
        {
            if(___loadingComplete && ___queuedContinue)
            {
                GLAPModLoader.DebugShowMessage("Firing the OnLoadedIntoNewArea event!");
                GLAPEvents.OnLoadedIntoNewArea?.Invoke();
            }
        }

    }
}
