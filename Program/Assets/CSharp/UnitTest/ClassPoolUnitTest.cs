using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    using System;
    using Infrastructure.Managers;
    public class ClassPoolUnitTest : MonoBehaviour
    {
        public class TestA:IClass
        {
            public int a;

            void IClass.OnReset()
            {

            }
        }
        public class TestB:IClass
        {
            public int b;

            void IClass.OnReset()
            {

            }
        }
        public class TestC:IClass
        {
            public int c;

            void IClass.OnReset()
            {

            }
        }
        ClassManager m_classManager;
        private List<TestA> usedTestA = new List<TestA>();
        private List<TestB> usedTestB = new List<TestB>();
        private List<TestC> usedTestC = new List<TestC>();
        // Use this for initialization
        void Start()
        {
            m_classManager = ClassManager.GetInstance();
            Store();
            InvokeRepeating("RandomAdd", 0.2f, 0.2f);
            InvokeRepeating("RandomRemove", 1f, 0.8f);
        }

        // Update is called once per frame
        void Update()
        {

        }
        void OnDestroy()
        {
            m_classManager.Dispose();
        }
        void Store()
        {
            ClassManager.Store(typeof(TestA), 100);
            ClassManager.Store(typeof(TestB), 300);
            ClassManager.Store(typeof(TestC), 250);
        }
        void RandomAdd()
        {
            for (int i = 0; i < 5; i++)
            {
                usedTestA.Add(ClassManager.Get<TestA>());
            }
            for (int i = 0; i < 8; i++)
            {
                usedTestB.Add(ClassManager.Get<TestB>());
            }
            for (int i = 0; i < 5; i++)
            {
                usedTestC.Add(ClassManager.Get<TestC>());
            }
        }
        void RandomRemove()
        {
            for (int i = 0; i < usedTestA.Count; i++)
            {
                ClassManager.Free(usedTestA[i]);
                usedTestA.RemoveAt(i);
                i--;
            }
            for (int i = 0; i < usedTestB.Count; i++)
            {
                ClassManager.Free(usedTestB[i]);
                usedTestB.RemoveAt(i);
                i--;
            }
            for (int i = 0; i < usedTestC.Count; i++)
            {
                ClassManager.Free(usedTestC[i]);
                usedTestC.RemoveAt(i);
                i--;
            }
        }
    }
}