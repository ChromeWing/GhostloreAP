using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GhostloreAP
{
    public class TestTextDrawer : MonoBehaviour
    {
        private string message="";


        void Awake()
        {
        }


        public void DisplayMessage(string msg_)
        {
            message = msg_+"\n"+message;
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 400, 800), message);
        }
    }
}
