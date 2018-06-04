using System;
using System.Collections.Generic;
using UnityEngine;
namespace lhFramework.Infrastructure.Managers
{
    public interface IModel
    {
        int index { get; set; }
        EModelGroup group { get; set; }
        GameObject gameObject { get; set; }
        Transform transform { get; set; }
        void OnCreate();
        void OnUpdate();
        void OnUse();
        void OnFree();
        Transform Get(string name);
        T Get<T>() where T:Component;
        void Gets<T>(ref List<T> list) where T : Component;
        void SetLayer(int layer);
    }
}
