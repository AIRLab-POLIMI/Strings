
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Oasis.GameEvents
{
    public abstract class BaseGameEventSO : ScriptableObject
    {
        [SerializeField] [TextArea] protected string info; // for debug, to show info on the specific event
    }
}

