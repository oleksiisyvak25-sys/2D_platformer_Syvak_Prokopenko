using System;
using System.Collections.Generic;
using System.Text;

namespace Assets
{
    internal interface IInteractable
    {
        bool CanInteract();

        void Interact();
    }
}
