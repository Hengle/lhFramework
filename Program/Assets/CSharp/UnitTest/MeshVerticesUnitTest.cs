using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Threading;

namespace lhFramework.UnitTest
{
	public class MeshVerticesUnitTest:MonoBehaviour
    {
		public int count;
		public float speed;
		public MeshFilter cubeFilter;
		public MeshFilter targetFilter;
		public List<Vector3> cubes=new List<Vector3>();
		public List<Vector3> cubesEuler=new List<Vector3>();

		public Mesh mesh;
		public List<Vector3> vertices=new List<Vector3>();
		public List<int> triangles=new List<int>();
		private Mesh cubeMesh;
		private Vector3[] cubeMeshVertices;
		private List<Vector3> m_cubesDir = new List<Vector3> ();
		private BoxCollider m_cubeBox;
        void Start()
        {
			for (int i = 0; i < count; i++) {
				cubes.Add (new Vector3 (UnityEngine.Random.Range (-10, 10), 0, UnityEngine.Random.Range (-10, 10)));
				cubesEuler.Add (Vector3.zero);
				m_cubesDir.Add(new Vector3 (UnityEngine.Random.Range (-1, 1), 0, UnityEngine.Random.Range (-1, 1)));
			}
			cubeMesh = cubeFilter.sharedMesh;
			cubeMeshVertices = cubeMesh.vertices;
			mesh = new Mesh ();
			for (int i = 0; i < cubes.Count; i++) {
				vertices.AddRange (cubeMesh.vertices);
				for (int j = 0; j < cubeMesh.triangles.Length; j++) {
					triangles.Add (cubeMesh.triangles[j]+cubeMesh.vertexCount*i);
				}
			}
			mesh.SetVertices (vertices);
			mesh.SetTriangles (triangles,0);
			targetFilter.sharedMesh = mesh;
			Thread athread = new Thread(new ThreadStart(goThread));
			athread.IsBackground = true;
			athread.Start();
			InvokeRepeating("OnChangeDir", 3, 3);
			m_cubeBox = cubeFilter.GetComponent<BoxCollider> ();
		}
		void goThread()
		{
			while (true)
			{
				for (int i = 0; i < cubes.Count; i++) {
					for (int j = i*cubeMeshVertices.Length,x=0; x < cubeMeshVertices.Length; j++,x++) {
						vertices[j]=cubes[i]+DoRotate(cubeMeshVertices[x],cubesEuler[i]);
					}
				}
			}
		}
		void OnChangeDir(){
			for (int i = 0; i < count; i++) {
				m_cubesDir[i]=(new Vector3 (UnityEngine.Random.Range (-1, 1), 0, UnityEngine.Random.Range (-1, 1)));
			}
		}
        
        void Update()
		{
			mesh.bounds = new Bounds (m_cubeBox.center, m_cubeBox.size);
        }
		void OnPreRender(){
			mesh.SetVertices (vertices);
		}
		void OnPostRender(){
			for (int i = 0; i < cubes.Count; i++) {
				cubes[i]+=speed*m_cubesDir[i];//new Vector3 (UnityEngine.Random.Range (-10, 10), 0, UnityEngine.Random.Range (-10, 10));
				cubesEuler[i]+=m_cubesDir[i];//new Vector3 (UnityEngine.Random.Range (-30, 30), 0, UnityEngine.Random.Range (-30, 30));
			}
		}
        void OnGUI()
        {
		}
		Vector3 DoRotate(Vector3 position ,Vector3 rot){
			rot *= Mathf.Deg2Rad;
			float cosAngle = Mathf.Cos(rot.x);
			float sinAngle = Mathf.Sin(rot.x);
			float x = position.x;
			float y = position.y * cosAngle + sinAngle * position.z;      
			float z = -position.y * sinAngle + cosAngle * position.z;
			position= new Vector3(x,y,z);

			cosAngle = Mathf.Cos(rot.y);
			sinAngle = Mathf.Sin(rot.y);
			x = position.x * cosAngle - sinAngle * position.z;
			y = position.y;
			z = position.x * sinAngle + cosAngle * position.z;
			position= new Vector3(x,y,z);

			cosAngle = Mathf.Cos(rot.z);
			sinAngle = Mathf.Sin(rot.z);
			x = position.x * cosAngle + sinAngle * position.y;
			y = -position.x * sinAngle + cosAngle * position.y;
			z = position.z;
			return new Vector3(x,y,z);
		}
    }
}
