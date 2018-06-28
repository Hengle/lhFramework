using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class EffectAdditiveOffsetUtility : MonoBehaviour
{
    public Color color=Color.white;
    public Vector2 tiling=Vector2.one;
    public Vector2 offset;
    [HideInInspector]
    public MeshFilter filter;
    [HideInInspector]
    public MeshRenderer render;
    [HideInInspector]
    public Mesh sharedMesh;
    [HideInInspector]
    public List<Vector4> uv1;
    [HideInInspector]
    public Color[] colors;
    [HideInInspector]
    //public bool first;
    private Mesh m_mesh;

    private Color m_oldColor;
    private Vector2 m_oldTiling;
    private Vector2 m_oldOffset;
    // Use this for initialization
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
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
            //        UnityEngine.Object.Destroy(sharedMesh);
            //    }
            //    sharedMesh = filter.sharedMesh;
            //    filter.sharedMesh = Mesh.Instantiate(filter.sharedMesh);
            //    tangents = new Vector4[filter.sharedMesh.vertexCount];
            //    colors = new Color[filter.sharedMesh.vertexCount];
            //    return;
            //}
#endif

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            for (int i = 0; i < uv1.Count; i++)
            {
                uv1[i] =new Vector4(tiling.x, tiling.y,offset.x,offset.y);
            }
            //m_mesh.tangents = uv1;
            m_mesh.SetUVs(1, uv1);
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
            if (render.sharedMaterial.shader.name!="Effect/Additive_Offseting")
            {
                UnityEngine.Debug.LogError(name+ "====?EffectAdditiveOffsetUtility 必须使用这个shader：  Effect/Additive_Offseting ");
                return;
            }
            if (sharedMesh == null || !filter.sharedMesh.name.Contains(sharedMesh.name))
            {
                if (sharedMesh != null)
                {
                    UnityEngine.Object.DestroyImmediate(sharedMesh);
                }
                sharedMesh = filter.sharedMesh;
                filter.sharedMesh=Mesh.Instantiate(filter.sharedMesh);
                uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
                for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                {
                    uv1.Add( Vector4.zero);
                }
                colors = new Color[filter.sharedMesh.vertexCount];
                return;
            }
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            for (int i = 0; i < uv1.Count; i++)
            {
                uv1[i] = new Vector4(tiling.x, tiling.y, offset.x, offset.y);
            }
            filter.sharedMesh.SetUVs(1, uv1);
            filter.sharedMesh.colors = colors;
#endif
        }
    }
    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var thi = GetComponents<EffectAdditiveOffsetUtility>();
            if (thi != null && thi.Length>1)
            {
                DestroyImmediate(this);
                return;
            }
            filter = GetComponent<MeshFilter>();
            render = GetComponent<MeshRenderer>();
            if (render.sharedMaterial == null) return;
            if (sharedMesh == null && filter.sharedMesh == null) return;
            if (sharedMesh==null)
            {
                sharedMesh = filter.sharedMesh;
            }
            filter.sharedMesh = Mesh.Instantiate(sharedMesh);
            uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
            for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
            {
                uv1.Add(Vector4.zero);
            }
            colors = new Color[filter.sharedMesh.vertexCount];
            //if (!first)
            //{
            //    try
            //    {
            //        color = render.sharedMaterial.GetColor("_Color");
            //        var main_ST = render.sharedMaterial.GetVector("_MainTex_ST");
            //        tiling = new Vector2(main_ST.x, main_ST.y);
            //        offset = new Vector2(main_ST.z, main_ST.w);
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
            m_oldTiling = tiling;
            m_oldOffset = offset;
            if (filter.sharedMesh == null || render.sharedMaterial == null) return;
            if (m_mesh == null)
            {
                filter.sharedMesh = sharedMesh;
                m_mesh = filter.mesh;
            }
            if (uv1 == null || uv1.Count <= 0)
            {
                uv1 = new List<Vector4>(filter.mesh.vertexCount);
                for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                {
                    uv1.Add(Vector4.zero);
                }
            }
            if (colors == null || colors.Length <= 0)
            {
                colors = new Color[filter.mesh.vertexCount];
            }
        }
    }
    void OnDisable()
    {
        if (Application.isPlaying)
        {
            color=m_oldColor;
            m_oldTiling = tiling;
            m_oldOffset = offset;
        }
    }
}