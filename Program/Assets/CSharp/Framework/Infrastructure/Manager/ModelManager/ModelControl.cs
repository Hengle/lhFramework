using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    public class ModelControl :MonoBehaviour, IModel
    {
        public Transform[] trams;
        public Component[] components;
        GameObject IModel.gameObject { get { return m_gameObject; } set { } }
        Transform IModel.transform { get { return m_transform; } set { } }
        EModelGroup IModel.group { get; set; }
        int IModel.index { get; set; }
        private GameObject m_gameObject;
        private Transform m_transform;
        void IModel.OnCreate()
        {
            m_transform = base.transform;
            m_gameObject = base.gameObject;
        }
        void IModel.OnFree()
        {

        }
        void IModel.OnUpdate()
        {

        }
        Transform IModel.Get(string name)
        {
            for (int i = 0; i < trams.Length; i++)
            {
                if (trams[i].name==name)
                {
                    return trams[i];
                }
            }
            return null;
        }
        T IModel.Get<T>()
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is T)
                {
                    return (T)components[i];
                }
            }
            return null;
        }
        void IModel.Gets<T>(ref List<T> list)
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is T)
                {
                    list.Add((T)components[i]);
                }
            }
        }
        void IModel.OnUse()
        {

        }
        void IModel.SetLayer(int layer)
        {

        }
    }
}
