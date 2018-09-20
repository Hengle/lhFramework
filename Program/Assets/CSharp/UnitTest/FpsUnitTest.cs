using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    public class FpsUnitTest : MonoBehaviour
    {

        private float timeLeft = 1;
        private int frameCount = 0;
        private int fps = 0;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            CalculateFPS(Time.deltaTime);
        }
        void OnGUI()
        {
            GUILayout.Label(fps.ToString());
        }

        private void CalculateFPS(float deltaTime)
        {
            timeLeft -= deltaTime;
            if (timeLeft < 0)
            {
                fps = frameCount;
                frameCount = 0;
                timeLeft = 1;
                return;
            }
            ++frameCount;
        }
    }
}
