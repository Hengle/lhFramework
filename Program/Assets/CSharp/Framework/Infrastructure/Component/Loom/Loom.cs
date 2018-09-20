using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

namespace lhFramework.Infrastructure.Components
{
    public class Loom
    {
        public static int maxThreads = 8;
        private static int numThreads;
        private List<Action> m_currentActions = new List<Action>();
        private List<Action> m_actions = new List<Action>();

        private static Loom m_instance;
        public static Loom GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new Loom();
        }
        Loom()
        {

        }
        public void Update()
        {
            lock (m_actions)
            {
                m_currentActions.Clear();
                for (int i = 0; i < m_actions.Count; i++)
                {
                    m_currentActions.Add(m_actions[i]);
                }
                m_actions.Clear();
            }
            foreach (var a in m_currentActions)
            {
                a();
            }

        }
        public void Dispose()
        {
            m_instance = null;
        }
        public static void RunMain(Action action)
        {
            lock (m_instance.m_actions)
            {
                m_instance.m_actions.Add(action);
            }
        }
        public static void RunAsync(Action a)
        {
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }
            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(m_instance.RunAction, a);
        }
        private void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }
        }
    }
}
//void ScaleMesh(Mesh mesh, float scale)
//{
//    //Get the vertices of a mesh
//    var vertices = mesh.vertices;
//    //Run the action on a new thread
//    Loom.RunAsync(() => {
//        //Loop through the vertices
//        for (var i = 0; i < vertices.Length; i++)
//        {
//            //Scale the vertex
//            vertices[i] = vertices[i] * scale;
//        }
//        //Run some code on the main thread
//        //to update the mesh
//        Loom.RunMain(() => {
//            //Set the vertices
//            mesh.vertices = vertices;
//            //Recalculate the bounds
//            mesh.RecalculateBounds();
//        });

//    });
//}