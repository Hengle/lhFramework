using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class EffectMaskUVAdditiveUtility : MonoBehaviour {

    public Color color=Color.white;
    public Vector2 mainTiling=Vector2.one;
    public Vector2 mainOffset;
    public Vector2 maskTiling=Vector2.one;
    public Vector2 maskOffset;
    public Vector2 speedUV;
    [Range(1.0f, 10.0f)]
    public float power=1;

    [HideInInspector]
    public MeshFilter filter;
    [HideInInspector]
    public MeshRenderer render;
    [HideInInspector]
    public Mesh sharedMesh;
    [HideInInspector]
    public List<Vector4> uv3;
    [HideInInspector]
    public Color[] colors;
    [HideInInspector]
    public List<Vector4> uv1;
    [HideInInspector]
    public List<Vector4> uv2;
    //[HideInInspector]
    //public bool first;
    
    private Color m_oldColor;
    private Vector2 m_oldMainTiling;
    private Vector2 m_oldMainOffset;
    private Vector2 m_oldMaskTiling;
    private Vector2 m_oldMaskOffset;
    private Vector2 m_oldUv;
    private float m_oldPower;
    private Mesh m_mesh;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.isPlaying)
        {
            if (filter == null)
            {
                filter = GetComponent<MeshFilter>();
            }
            if (render == null)
            {
                render = GetComponent<MeshRenderer>();
            }
            if (filter.sharedMesh == null || render.sharedMaterial == null) return;

#if UNITY_EDITOR
            //if (!filter.sharedMesh.name.Contains(sharedMesh.name))
            //{
            //    if (sharedMesh != null)
            //    {
            //        UnityEngine.Object.DestroyImmediate(sharedMesh);
            //    }
            //    sharedMesh = filter.sharedMesh;
            //    filter.sharedMesh = Mesh.Instantiate(filter.sharedMesh);
            //    tangents = new Vector4[filter.sharedMesh.vertexCount];
            //    colors = new Color[filter.sharedMesh.vertexCount];
            //    uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
            //    uv2 = new List<Vector4>(filter.sharedMesh.vertexCount);
            //    for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
            //    {
            //        uv1.Add(Vector4.zero);
            //        uv2.Add(Vector4.zero);
            //    }
            //    return;
            //}
#endif
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            for (int i = 0; i < uv3.Count; i++)
            {
                uv3[i] = new Vector4(mainTiling.x, mainTiling.y, mainOffset.x, mainOffset.y);
            }
            for (int i = 0; i < uv1.Count; i++)
            {
                uv1[i] = new Vector4(maskTiling.x, maskTiling.y, maskOffset.x, maskOffset.y);
            }
            for (int i = 0; i < uv2.Count; i++)
            {
                uv2[i] = new Vector4(speedUV.x, speedUV.y, power, 0);
            }
            m_mesh.SetUVs(1, uv1);
            m_mesh.SetUVs(2, uv2);
            m_mesh.SetUVs(3, uv3);
            m_mesh.colors = colors;
        }
        else
        {
#if UNITY_EDITOR
            if (filter == null)
            {
                filter = GetComponent<MeshFilter>();
            }
            if (render == null)
            {
                render = GetComponent<MeshRenderer>();
            }
            if (filter.sharedMesh == null || render.sharedMaterial == null) return;
            if (render.sharedMaterial.shader.name != "Effect/Mask_UV_Additive_Offseting")
            {
                UnityEngine.Debug.LogError(name + "====?EffectMaskUVAdditiveUtility 必须使用这个shader：  Effect/Mask_UV_Additive_Offseting ");
                return;
            }
            if (sharedMesh == null || !filter.sharedMesh.name.Contains(sharedMesh.name))
            {
                if (sharedMesh != null)
                {
                    UnityEngine.Object.DestroyImmediate(sharedMesh);
                }
                sharedMesh = filter.sharedMesh;
                filter.sharedMesh = Mesh.Instantiate(filter.sharedMesh);
                uv3 = new List<Vector4>(filter.sharedMesh.vertexCount);
                colors = new Color[filter.sharedMesh.vertexCount];
                uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
                uv2 = new List<Vector4>(filter.sharedMesh.vertexCount);
                for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                {
                    uv1.Add(Vector4.zero);
                    uv2.Add(Vector4.zero);
                    uv3.Add(Vector4.zero);
                }
                return;
            }
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            for (int i = 0; i < uv3.Count; i++)
            {
                uv3[i] = new Vector4(mainTiling.x, mainTiling.y, mainOffset.x, mainOffset.y);
            }
            for (int i = 0; i < uv1.Count; i++)
            {
                uv1[i] = new Vector4(maskTiling.x, maskTiling.y, maskOffset.x, maskOffset.y);
            }
            for (int i = 0; i < uv2.Count; i++)
            {
                uv2[i] = new Vector4(speedUV.x, speedUV.y, power, 0);
            }
            filter.sharedMesh.SetUVs(1, uv1);
            filter.sharedMesh.SetUVs(2, uv2);
            filter.sharedMesh.SetUVs(3, uv3);
            filter.sharedMesh.colors = colors;
#endif
        }
    }
    void OnEnable()
    {

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var thi = GetComponents<EffectMaskUVAdditiveUtility>();
            if (thi != null && thi.Length > 1)
            {
                DestroyImmediate(this);
                return;
            }
            filter = GetComponent<MeshFilter>();
            render = GetComponent<MeshRenderer>();
            if (render.sharedMaterial == null) return;
            if (sharedMesh == null && filter.sharedMesh == null) return;
            if (sharedMesh == null)
            {
                sharedMesh = filter.sharedMesh;
            }
            filter.sharedMesh = Mesh.Instantiate(sharedMesh);
            uv3 = new List<Vector4>(filter.sharedMesh.vertexCount);
            colors = new Color[filter.sharedMesh.vertexCount];
            uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
            uv2 = new List<Vector4>(filter.sharedMesh.vertexCount);
            for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
            {
                uv1.Add(Vector4.zero);
                uv2.Add(Vector4.zero);
                uv3.Add(Vector4.zero);
            }
            //if (!first)
            //{
            //    try
            //    {
            //        color = render.sharedMaterial.GetColor("_color");
            //        var main_ST = render.sharedMaterial.GetVector("_MainTex_ST");
            //        mainTiling = new Vector2(main_ST.x, main_ST.y);
            //        mainOffset = new Vector2(main_ST.z, main_ST.w);
            //        var mask_ST = render.sharedMaterial.GetVector("_MaskTex_ST");
            //        maskTiling = new Vector2(mask_ST.x, mask_ST.y);
            //        maskOffset = new Vector2(mask_ST.z, mask_ST.w);
            //        var speedU = render.sharedMaterial.GetFloat("_SpeedU");
            //        var speedV = render.sharedMaterial.GetFloat("_SpeedV");
            //        speedUV = new Vector2(speedU, speedV);
            //        power = render.sharedMaterial.GetFloat("_Power");
            //    }
            //    catch
            //    {

            //    }
            //    first = true;
            //}
        }
#endif
        if (Application.isPlaying)
        {
            m_oldColor = color;
            m_oldMainTiling = mainTiling;
            m_oldMainOffset = mainOffset;
            m_oldMaskTiling = maskTiling;
            m_oldMaskOffset = maskOffset;
            m_oldUv = speedUV;
            m_oldPower = power;
            if (m_mesh == null)
            {
                filter.sharedMesh = sharedMesh;
                m_mesh = filter.mesh;
            }
            if (uv3 == null || uv3.Count <= 0)
            {
                uv3 = new List<Vector4>(filter.mesh.vertexCount);
                for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                {
                    uv3.Add(Vector4.zero);
                }
            }
            if (colors == null || colors.Length <= 0)
            {
                colors = new Color[filter.mesh.vertexCount];
            }
            if (uv1.Count<=0)
            {
                uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
                for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                {
                    uv1.Add(Vector4.zero);
                }
            }
            if (uv2.Count<=0)
            {
                uv2 = new List<Vector4>(filter.sharedMesh.vertexCount);
                for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                {
                    uv2.Add(Vector4.zero);
                }
            }
        }
    }
    void OnDisable()
    {
        if (Application.isPlaying)
        {
            color = m_oldColor;
            mainTiling=m_oldMainTiling ;
            mainOffset=m_oldMainOffset ;
            maskTiling=m_oldMaskTiling ;
            maskOffset=m_oldMaskOffset ;
            speedUV=m_oldUv ;
            power=m_oldPower ;
        }
    }
}
