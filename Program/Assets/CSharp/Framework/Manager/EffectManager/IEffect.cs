
using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    public interface IEffect
    {
        int index { get; set; }
        EEffectGroup group { get; set; }
        UnityEngine.GameObject gameObject { get; set; }
        UnityEngine.Transform transform { get; set; }
        int InstanceID();
        void Create();
        void OnUpdate();
        void OnPause();
        void OnResume();
        void Bind(int index, Transform trans, Vector3 offset,EEffectBindType bindType, int time,Vector3 dir);
        void UnBind(int index);
        void ResetLayer();
        void SetLayer(int layer);
    }
}
