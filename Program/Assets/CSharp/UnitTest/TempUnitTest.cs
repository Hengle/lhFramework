using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
	public class TempUnitTest : MonoBehaviour {
		public Vector3 rot;
		public Vector3[] target;
		public MeshFilter filter;
		private Mesh mesh;
		private Mesh sharedMesh;
		// Use this for initialization
		void Start () {
			sharedMesh = filter.sharedMesh;
			mesh = Mesh.Instantiate (sharedMesh);
			filter.sharedMesh = mesh;
			target = sharedMesh.vertices;
		}
		
		// Update is called once per frame
		void Update () {
			for (int i = 0; i < target.Length; i++) {
				target [i] =DoRotate(sharedMesh.vertices[i],rot);//DoTwistZ(DoTwistY( DoTwistX (sharedMesh.vertices [i], rot.x),rot.y),rot.z);
			}
			filter.sharedMesh.vertices= target;
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