using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostloreAP
{
    public class DeathLinkKey : MonoBehaviour
    {
        InputAction deathlinkInput;
        public void Start()
        {
            deathlinkInput = new InputAction("toggleDeathlink", binding: "<Keyboard>/f1");
            deathlinkInput.performed -= InvokeDeathlinkToggle;
            deathlinkInput.performed += InvokeDeathlinkToggle;

            deathlinkInput.Enable();
        }

        public void InvokeDeathlinkToggle(InputAction.CallbackContext callback)
        {
            GLAPEvents.OnToggleDeathlinkPressed?.Invoke();
        }
    }
}
