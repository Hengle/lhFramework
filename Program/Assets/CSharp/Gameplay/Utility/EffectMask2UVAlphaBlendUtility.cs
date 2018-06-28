using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class EffectMask2UVAlphaBlendUtility : MonoBehaviour {

    public Color color=Color.white;
    public Vector2 mainTiling=Vector2.one;
    public Vector2 mainOffset;
    public Vector2 maskTiling=Vector2.one;
    public Vector2 maskOffset;
    public Vector2 speedUV;
    public Vector2 mask2Tiling = Vector2.one;
    public Vector2 mask2Offset;
    public Vector2 speedUV2;
    [Range(1.0f, 10.0f)]
    public float power=1;

    [HideInInspector]
    public MeshFilter filter;
    [HideInInspector]
    public MeshRenderer render;
    [HideInInspector]
    public Mesh sharedMesh;
    [HideInInspector]
    public Color[] colors;
    [HideInInspector]
    public List<Vector4> uv1;
    [HideInInspector]
    public List<Vector4> uv2;
    [HideInInspector]
    public List<Vector4> uv3;
    //[HideInInspector]
    //public bool first;
    
    private Color m_oldColor;
    private Vector2 m_oldMainTiling;
    private Vector2 m_oldMainOffset;
    private Vector2 m_oldMaskTiling;
    private Vector2 m_oldMaskOffset;
    private Vector2 m_oldMask2Tiling;
    private Vector2 m_oldMask2Offset;
    private Vector2 m_oldSpeedUv;
    private Vector2 m_oldSpeedUv2;
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
                colors[i] = color* power;
            }
            for (int i = 0; i < uv1.Count; i++)
            {
                uv1[i] = new Vector4(speedUV.x, speedUV.y, speedUV2.x, speedUV2.y);
            }
            var mainOffsetVar = ToOne(this.mainOffset);
            var maskOffsetVar = ToOne(this.maskOffset);
            var mask2OffsetVar = ToOne(this.mask2Offset);
            //var mainTilingVar = new Vector4(mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1));
            for (int i = 0; i < uv2.Count; i++)
            {
                uv2[i] = new Vector4((mainTiling.x - (int)(mainTiling.x)) * 10000 + maskTiling.x, (mainTiling.y - (int)(mainTiling.y)) * 10000 + maskTiling.y, (mainOffsetVar.x - (int)(mainOffsetVar.x)) * 10000 + maskOffsetVar.x, (mainOffsetVar.y - (int)(mainOffsetVar.y)) * 10000 + maskOffsetVar.y);
            }
            for (int i = 0; i < uv3.Count; i++)
            {
                uv3[i] = new Vector4((int)(mainTiling.x) * 100 + mask2Tiling.x, (int)(mainTiling.y) * 100 + mask2Tiling.y, (int)(mainOffsetVar.x) * 100 + mask2OffsetVar.x, (int)(mainOffsetVar.y) * 100 + mask2OffsetVar.y);
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
            if (render.sharedMaterial.shader.name != "Effect/Mask2_UV_AlphaBlend_Offseting")
            {
                UnityEngine.Debug.LogError(name + "====?EffectMask2UVAlphaBlendUtility 必须使用这个shader：  Effect/Mask2_UV_AlphaBlend_Offseting ");
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
                colors = new Color[filter.sharedMesh.vertexCount];
                uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
                uv2 = new List<Vector4>(filter.sharedMesh.vertexCount);
                uv3 = new List<Vector4>(filter.sharedMesh.vertexCount);
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
                colors[i] = color * power;
            }
            for (int i = 0; i < uv1.Count; i++)
            {
                uv1[i] = new Vector4(speedUV.x, speedUV.y, speedUV2.x, speedUV2.y);
            }
            var mainOffsetVar = ToOne(this.mainOffset);
            var maskOffsetVar = ToOne(this.maskOffset);
            var mask2OffsetVar = ToOne(this.mask2Offset);
            //var mainTilingVar = new Vector4(mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1), mainTiling.x >= 0 ? mainTiling.x - (int)mainTiling.x : (mainTiling.x * -1) - (int)(mainTiling.x * -1));
            for (int i = 0; i < uv2.Count; i++)
            {
                uv2[i] = new Vector4((mainTiling.x - (int)(mainTiling.x)) * 10000 + maskTiling.x, (mainTiling.y - (int)(mainTiling.y)) * 10000 + maskTiling.y, (mainOffsetVar.x - (int)(mainOffsetVar.x)) * 10000 + maskOffsetVar.x, (mainOffsetVar.y - (int)(mainOffsetVar.y)) * 10000 + maskOffsetVar.y);
            }
            for (int i = 0; i < uv3.Count; i++)
            {
                uv3[i] = new Vector4((int)(mainTiling.x) * 100 + mask2Tiling.x, (int)(mainTiling.y) * 100 + mask2Tiling.y, (int)(mainOffsetVar.x) * 100 + mask2OffsetVar.x, (int)(mainOffsetVar.y) * 100 + mask2OffsetVar.y);
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
            var thi = GetComponents<EffectMask2UVAlphaBlendUtility>();
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
            colors = new Color[filter.sharedMesh.vertexCount];
            uv1 = new List<Vector4>(filter.sharedMesh.vertexCount);
            uv2 = new List<Vector4>(filter.sharedMesh.vertexCount);
            uv3 = new List<Vector4>(filter.sharedMesh.vertexCount);
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
            //        color = render.sharedMaterial.GetColor("_TintColor");
            //        var main_ST = render.sharedMaterial.GetVector("_MainTex_ST");
            //        mainTiling = new Vector2(main_ST.x, main_ST.y);
            //        mainOffset = new Vector2(main_ST.z, main_ST.w);
            //        var mask_ST = render.sharedMaterial.GetVector("_mask01_ST");
            //        maskTiling = new Vector2(mask_ST.x, mask_ST.y);
            //        maskOffset = new Vector2(mask_ST.z, mask_ST.w);
            //        var mask2_ST = render.sharedMaterial.GetVector("_mask02_ST");
            //        mask2Tiling = new Vector2(mask_ST.x, mask_ST.y);
            //        mask2Offset = new Vector2(mask_ST.z, mask_ST.w);
            //        var speedU = render.sharedMaterial.GetFloat("_x1");
            //        var speedV = render.sharedMaterial.GetFloat("_y1");
            //        speedUV = new Vector2(speedU, speedV);
            //        var speedU2 = render.sharedMaterial.GetFloat("_x2");
            //        var speedV2 = render.sharedMaterial.GetFloat("_y2");
            //        speedUV2 = new Vector2(speedU2, speedV2);
            //        power = render.sharedMaterial.GetFloat("_maint");
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
            m_oldMask2Tiling = mask2Tiling;
            m_oldMask2Offset = mask2Offset;
            m_oldSpeedUv = speedUV;
            m_oldSpeedUv2 = speedUV2;
            m_oldPower = power;
            if (m_mesh == null)
            {
                filter.sharedMesh = sharedMesh;
                m_mesh = filter.mesh;
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
            if (uv3.Count <= 0)
            {
                uv3 = new List<Vector4>(filter.sharedMesh.vertexCount);
                for (int i = 0; i < filter.sharedMesh.vertexCount; i++)
                {
                    uv3.Add(Vector4.zero);
                }
            }
        }
    }
    void OnDisable()
    {
        if (Application.isPlaying)
        {
            m_oldColor = color;
            m_oldMainTiling = mainTiling;
            m_oldMainOffset = mainOffset;
            m_oldMaskTiling = maskTiling;
            m_oldMaskOffset = maskOffset;
            m_oldMask2Tiling = mask2Tiling;
            m_oldMask2Offset = mask2Offset;
            m_oldSpeedUv = speedUV;
            m_oldSpeedUv2 = speedUV2;
            m_oldPower = power;
        }
    }
    Vector4 ToOne(Vector4 target)
    {
        return new Vector4(target.x > 0 ? target.x - (int)target.x : 1 - ((-1 * target.x) - (int)(-1 * target.x)), target.y > 0 ? target.y - (int)target.y : 1 - ((-1 * target.y) - (int)(-1 * target.y)), target.z > 0 ? target.z - (int)target.z : 1 - ((-1 * target.z) - (int)(-1 * target.z)), target.w > 0 ? target.w - (int)target.w : 1 - ((-1 * target.w) - (int)(-1 * target.w)));
    }
}
