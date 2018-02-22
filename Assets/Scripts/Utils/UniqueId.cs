using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utilities
{
    [Serializable]
    public class UniqueId
    {
        [SerializeField]
        private string uniqueId;

        public UniqueId()
        {
            uniqueId = Guid.NewGuid().ToString();
        }

        public UniqueId(UniqueId original)
        {
            uniqueId = original.GetId();
        }

        public string GetId()
        {
            return uniqueId;
        }
    }
}