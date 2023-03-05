using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GhostloreAP
{
    public static class CharacterCreationPatcher
    {
        private static bool setupMenu = false;

        private static List<GameObject> props = new List<GameObject>();

        private static TMP_InputField input_slot, input_ip, input_port, input_password;

        private static bool accepted = false;

        public static bool Prefix(TMP_InputField ___input,NewGameMenu __instance)
        {
            if(setupMenu && accepted)
            {
                return true;
            }
            if (setupMenu)
            {
                AttemptCreateProfile(__instance);
            }
            else
            {
                SetupAPMenu(___input);
            }
            return false;
        }

        public static bool Prefix2(GameObject ___mainMenuButtons, List<GameObject> ___openedMenus)
        {
            if (___mainMenuButtons != null && !___mainMenuButtons.activeInHierarchy)
            {
                using (List<GameObject>.Enumerator enumerator = ___openedMenus.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.gameObject.activeInHierarchy)
                        {
                            return true;
                        }
                    }
                }
                CleanupAPMenu();
            }
            return true;
        }

        private static void AttemptCreateProfile(NewGameMenu __instance)
        {
            string slotName = input_slot.text;
            string ip = input_ip.text;
            int port = int.Parse(input_port.text);
            string password = input_password.text;
            AttemptConnect(slotName,ip,port,password,__instance);


        }

        private static void AttemptConnect(string slotName,string ip,int port,string password,NewGameMenu __instance)
        {
            GLAPClient.instance.Connect(slotName, ip, port, password, () =>
            {
                GLAPProfileManager.instance.InitProfile(slotName,ip,port,password);

                accepted = true;
                __instance.OnNewGame();
            },
            () =>
            {

            });
        }

        private static void CleanupAPMenu()
        {
            foreach(GameObject obj in props)
            {
                GameObject.Destroy(obj.gameObject);
            }
            props.Clear();
            setupMenu = false;
            accepted = false;
        }

        private static void SetupAPMenu(TMP_InputField ___input)
        {
            if (setupMenu) { return; }
            Canvas canvas_ = GetCanvas(___input.transform);

            List<Canvas> allCanvas_ = Component.FindObjectsOfType<Canvas>().ToList();

            LogDesc(allCanvas_);
            GLAPModLoader.SaveLog();
            Transform t = null;
            GameObject background_ = null;
            GameObject name_ = null;
            GameObject createButton_ = null;
            GameObject cancelButton_ = null;
            for (int i = 0; i < allCanvas_.Count; i++)
            {

                GameObject b = GetNestedChild(allCanvas_[i].gameObject, "Panels", "New game", "BG");
                GameObject n = GetNestedChild(allCanvas_[i].gameObject, "Panels", "New game", "Right side", "name");
                if (!background_)
                {
                    background_ = GetNestedChild(allCanvas_[i].gameObject, "Panels", "New game", "BG");
                }
                if (!name_)
                {
                    name_ = GetNestedChild(allCanvas_[i].gameObject, "Panels", "New game", "Right side", "name");
                }
                if (!createButton_)
                {
                    createButton_ = GetNestedChild(allCanvas_[i].gameObject, "Panels", "New game", "Right side", "Bottom buttons", "Create");
                }
                if (!cancelButton_)
                {
                    cancelButton_ = GetNestedChild(allCanvas_[i].gameObject, "Panels", "New game", "Right side", "Bottom buttons", "Back");
                }
                if (background_ && name_ && createButton_ && cancelButton_)
                {
                    break;
                }
            }
            LogComp(background_);

            SetupBackground(canvas_, background_);

            SetupButton(canvas_,createButton_);
            SetupButton(canvas_,cancelButton_);

            input_slot = SetupInputBox("Slot Name", canvas_, ___input, name_, 0);
            input_ip = SetupInputBox("Server IP", canvas_, ___input, name_, 1);
            input_port = SetupInputBox("Server Port", canvas_, ___input, name_, 2);
            input_password = SetupInputBox("Server Password", canvas_, ___input, name_, 3);

            input_slot.text = "";
            input_ip.text = "archipelago.gg";
            input_port.text = "38281";
            input_password.text = "";

            input_port.inputValidator = new PortNumberValidator();
            input_port.characterValidation = TMP_InputField.CharacterValidation.CustomValidator;
            input_password.contentType = TMP_InputField.ContentType.Password;

            //GLAPModLoader.SaveLog();
            setupMenu = true;
        }

        private static void Log(string log_)
        {
            GLAPModLoader.DebugShowMessage(log_);
        }

        private static void Log<T>(List<T> objs) where T : UnityEngine.Object
        {
            foreach(UnityEngine.Object obj in objs)
            {
                Log(obj.name);
            }
        }

        private static void LogDesc<T>(List<T> objs,int depth_=0,string path_="") where T : Component
        {
            foreach (Component obj in objs)
            {
                string indent_ = "";
                for(int i=0;i< depth_; i++)
                {
                    indent_ += ">";
                }
                string extPath_ = path_ +"/"+ obj.name;
                Log(extPath_);
                List<Transform> children = new List<Transform> ();
                for(int i = 0; i < obj.transform.childCount; i++)
                {
                    children.Add(obj.transform.GetChild(i));
                }
                LogDesc(children,depth_+1,extPath_);
            }
        }

        private static void LogComp(GameObject go)
        {
            Log(String.Format("{0}'s components ({1}):", go.name,go.GetComponents<Component>().Length));
            foreach(Component component in go.GetComponents<Component>())
            {
                Log(component.GetType().FullName);
            }
        }

        private static GameObject GetNestedChild(GameObject go,params string[] path)
        {
            GameObject result = go;
            var valid = true;
            for( int i = 0; i < path.Length; i++)
            {
                var p = path[i];
                for(int j = 0; j < result.transform.childCount; j++)
                {
                    var c = result.transform.GetChild(j);
                    if (c.gameObject.name == p)
                    {
                        result = c.gameObject;
                        break;
                    }
                    if (j == result.transform.childCount - 1)
                    {
                        valid = false;
                    }
                }
            }
            if (!valid) { return null; }
            return result;
        }

        private static void SetupBackground(Canvas canvas_,GameObject background_)
        {
            GameObject back_ = GameObject.Instantiate(background_,background_.transform.position+Vector3.back,background_.transform.rotation,canvas_.transform);
            back_.transform.SetAsLastSibling();
            Image img_ = back_.GetComponent<Image>();
            img_.color = new Color(img_.color.r * .5f, img_.color.g * .5f, img_.color.b * .5f);
            props.Add(back_);
        }

        private static void SetupButton(Canvas canvas_,GameObject button_)
        {
            GameObject btn_ = GameObject.Instantiate(button_,button_.transform.position,button_.transform.rotation,canvas_.transform);
            btn_.transform.SetAsLastSibling();
            props.Add(btn_);
        }

        private static TMP_InputField SetupInputBox(string name_,Canvas canvas_,TMP_InputField ___input, GameObject label,int i)
        {
            GameObject input_ = GameObject.Instantiate(___input.gameObject, ___input.transform.position + new Vector3(0, -i * GetUISpacingUnitsY(canvas_,140), 0), ___input.transform.rotation, canvas_.transform);
            input_.transform.SetAsLastSibling();
            GameObject label_ = GameObject.Instantiate(label, label.transform.position + new Vector3(-GetUISpacingUnitsX(canvas_,380), -i * GetUISpacingUnitsY(canvas_, 140) - GetUISpacingUnitsY(canvas_, 85), 0), label.transform.rotation, canvas_.transform);
            TMPro.TextMeshProUGUI tmp_ = label_.GetComponent<TMPro.TextMeshProUGUI>();
            tmp_.alignment = TextAlignmentOptions.Right;
            RectTransform tmpRT_=tmp_.GetComponent<RectTransform>();
            tmpRT_.pivot = new Vector2(1, .5f);
            tmpRT_.sizeDelta = new Vector2(GetUISpacingUnitsX(canvas_, 300), 1);
            tmp_.text = name_;
            props.Add(input_);
            props.Add(label_);
            return input_.GetComponent<TMP_InputField>();
        }

        private static float GetUISpacingUnitsY(Canvas canvas_,float value_)
        {
            return value_ * (canvas_.renderingDisplaySize.y / 1080f);
        }
        private static float GetUISpacingUnitsX(Canvas canvas_, float value_)
        {
            return value_ * (canvas_.renderingDisplaySize.x / 1920f);
        }

        private static Canvas GetCanvas(Transform t)
        {
            Canvas canvas = null;
            Transform p = t.parent;
            while(p!=null && canvas == null)
            {
                canvas = p.GetComponent<Canvas>();
                p = p.parent;
            }

            return canvas;
        }


        public static MethodInfo GetPrefix(Func<TMP_InputField,NewGameMenu,bool> action)
        {
            return action.Method;
        }

        public static MethodInfo GetPrefix2(Func<GameObject,List<GameObject>, bool> action)
        {
            return action.Method;
        }
    }


    public class PortNumberValidator : TMPro.TMP_InputValidator
    {
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (char.IsNumber(ch) && text.Length<5)
            {
                string textEdit_ = text + "";
                textEdit_ = textEdit_.Insert(pos,ch.ToString());
                int num_;
                if(int.TryParse(textEdit_, out num_))
                {
                    if(num_>= 0 && num_ <= 65535)
                    {
                        text = text.Insert(pos,ch.ToString());
                        pos++;
                        return ch;
                    }
                }
            }
            return '\0';
        }
    }
}
