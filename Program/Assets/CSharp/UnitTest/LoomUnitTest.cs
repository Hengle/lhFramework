using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    using lhFramework.Infrastructure.Components;
    public class LoomUnitTest:MonoBehaviour
    {
        Loom m_loom;
        void Start()
        {
            m_loom = Loom.GetInstance();
        }
        void Update()
        {
            m_loom.Update();
            Loom.RunAsync(RunAsync);
        }
        void RunAsync()
        {
            for (int i = 0; i < 1000000; i++)
            {
                int a = Mathf.Abs(-1);
            }
            Loom.RunMain(() =>
            {

            });
        }
    }
}
