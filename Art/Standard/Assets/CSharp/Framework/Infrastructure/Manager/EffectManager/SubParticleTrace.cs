using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
    public class SubParticleTrace:IClass
    {
        public int particleIndex;
        public Transform transform;
        public Vector3 offset;
        public EEffectBindType bindType;
        public int time;

        EClassType IClass.classType
        {
            get { return EClassType.Core_SubParticleTrace; }
            set { }
        }

        void IClass.OnReset()
        {
            transform = null;
            offset = Vector3.zero;
            time = 0;
        }
    }
}
