using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GhostloreAP
{
    //How do we address the lack of supporting extended fields, functions and other data on source classes using Harmony?
    //We use this.
    //Includes many different organized POCOs with a binding attached to the original instance, so that we can add logic and data onto existing classes in the source code.
    public class ExtendedBindingManager : Singleton<ExtendedBindingManager>, IGLAPSingleton
    {
        private Dictionary<System.Type, List<ExtendedBinding>> bindings;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
            bindings = new Dictionary<System.Type, List<ExtendedBinding>>();
        }

        public ExtendedBinding Register(System.Object obj, ExtendedBinding binding)
        {
            binding.SetObj(obj);
            binding.Init();
            if(bindings.TryGetValue(obj.GetType(), out List<ExtendedBinding> list))
            {
                if (list.Contains(binding))
                {
                    list[list.IndexOf(binding)] = binding;
                    return binding;
                }
                list.Add(binding);
            }
            else
            {
                var newList = new List<ExtendedBinding>();
                newList.Add(binding);
                bindings.Add(obj.GetType(), newList);   
            }
            return binding;
        }

        public T RegisterAndSet<T>(System.Object obj,Action<T> setter) where T : ExtendedBinding,new()
        {
            T binding = new T();
            binding.Set<T>(setter);
            return Register(obj, binding) as T;
        }

        public T GetExtended<T>(System.Object obj) where T : ExtendedBinding
        {
            if(bindings.TryGetValue(obj.GetType(), out List<ExtendedBinding> list))
            {
                return (T)list.Find(x => x.GetRaw() == obj);
            }

            return default(T);
        }

        public void Cleanup()
        {
            if(this==null || bindings == null || bindings.Values == null) { return; }
            foreach(List<ExtendedBinding> list in bindings.Values)
            {
                foreach(ExtendedBinding binding in list)
                {
                    binding.Destroy();
                }
                list.Clear();
            }
            bindings.Clear();

            GameObject.Destroy(this.gameObject);
        }

    }

    public class XItemInstance : ExtendedBinding
    {
        public Item overrideItem;
        public int cost;
        public int AP_ShopSlot = -1;

        public override void Destroy()
        {
            overrideItem = null;
        }

        public override void Init()
        {

        }
    }

    public class XQuest : ExtendedBinding
    {
        public Creature target;

        public override void Destroy()
        {

        }

        public override void Init()
        {

        }
    }

    public class XQuestInstance : ExtendedBinding
    {
        public Creature target;

        public override void Destroy()
        {

        }

        public override void Init()
        {

        }

        public bool Matches(Creature creature)
        {
            return creature == target;
        }
    }

    public class XQuestRequirement : ExtendedBinding
    {
        public string locationName;
        public Creature target;
        public int killCount = 0;
        public int killRequirement = 10;



        private bool alreadyCleared = false;
        private bool startedListener = false;
        

        public override void Init()
        {
            
        }

        public void StartListener()
        {
            if (startedListener) { return; }
            GLAPEvents.OnCreatureKilled += OnCreatureKilled;
            //GLAPModLoader.DebugShowMessage("registered");
            if (GLAPClient.instance.Connected && GLAPClient.instance.LocationAlreadyChecked(locationName))
            {
                alreadyCleared = true;
            }
            startedListener = true;
        }

        public override void Destroy()
        {
            GLAPEvents.OnCreatureKilled -= OnCreatureKilled;
        }

        private void OnCreatureKilled(CharacterContainer character,int count_)
        {
            if (MetRequirement()) { return; }
            if(target.CreatureName == character.Creature.CreatureName)
            {
                killCount+=count_;
                GLAPNotification.instance.DisplayKillLog(character, killCount, killRequirement);
            }
        }

        public bool MetRequirement()
        {
            return alreadyCleared || killCount >= killRequirement;
        }

    }


    public abstract class ExtendedBinding
    {
        protected System.Object obj;

        public abstract void Init();

        public abstract void Destroy();

        public void Set<T>(Action<T> setter) where T : ExtendedBinding
        {
            setter(this as T);
        }

        public void SetObj(System.Object obj)
        {
            this.obj = obj;
        }

        public T GetBase<T>()
        {
            return (T)obj;
        }

        public System.Object GetRaw()
        {
            return obj; 
        }
    }
}
