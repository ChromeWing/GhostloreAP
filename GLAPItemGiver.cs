﻿using System;
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

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);

            Init();
        }

        private void Init()
        {
            active = true;
        }

        public void Cleanup()
        {
            active = false;
            if (this == null) { return; }
            GameObject.Destroy(this.gameObject);
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
                    creature.LootTable.TrySpawnLoot(player, itemLevel, player.transform.position + Vector3.up * .1f, player.transform.position);
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