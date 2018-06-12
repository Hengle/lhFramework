//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

/*
    Test different texture packers
    Test lots of multiple material configs
    Try using on Coast scene
*/

/*
  
Notes on Normal Maps in Unity3d

Unity stores normal maps in a non standard format for some platforms. Think of the standard format as being english, unity's as being
french. The raw image files in the project folder are in english, the AssetImporter converts them to french. Texture2D.GetPixels returns 
french. This is a problem when we build an atlas from Texture2D objects and save the result in the project folder.
Unity wants us to flag this file as a normal map but if we do it is effectively translated twice.

Solutions:

    1) convert the normal map to english just before saving to project. Then set the normal flag and let the Importer do translation.
    This was rejected because Unity doesn't translate for all platforms. I would need to check with every version of Unity which platforms
    use which format.

    2) Uncheck "normal map" on importer before bake and re-check after bake. This is the solution I am using.

*/
namespace DigitalOpus.MB.Core
{

    [System.Serializable]
    public class ShaderTextureProperty
    {
        public string name;
        public bool isNormalMap;

        public ShaderTextureProperty(string n,
                                     bool norm)
        {
            name = n;
            isNormalMap = norm;
        }

        public static string[] GetNames(List<ShaderTextureProperty> props)
        {
            string[] ss = new string[props.Count];
            for (int i = 0; i < ss.Length; i++)
            {
                ss[i] = props[i].name;
            }
            return ss;
        }
    }

    [System.Serializable]
    public class MB3_TextureCombiner
    {

        public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

        public static ShaderTextureProperty[] shaderTexPropertyNames = new ShaderTextureProperty[] {
            new ShaderTextureProperty("_MainTex",false),
            new ShaderTextureProperty("_BumpMap",true),
            new ShaderTextureProperty("_Normal",true),
            new ShaderTextureProperty("_BumpSpecMap",false),
            new ShaderTextureProperty("_DecalTex",false),
            new ShaderTextureProperty("_Detail",false),
            new ShaderTextureProperty("_GlossMap",false),
            new ShaderTextureProperty("_Illum",false),
            new ShaderTextureProperty("_LightTextureB0",false),
            new ShaderTextureProperty("_ParallaxMap",false),
            new ShaderTextureProperty("_ShadowOffset",false),
            new ShaderTextureProperty("_TranslucencyMap",false),
            new ShaderTextureProperty("_SpecMap",false),
            new ShaderTextureProperty("_SpecGlossMap",false),
            new ShaderTextureProperty("_TranspMap",false),
            new ShaderTextureProperty("_MetallicGlossMap",false),
            new ShaderTextureProperty("_OcclusionMap",false),
            new ShaderTextureProperty("_EmissionMap",false),
            new ShaderTextureProperty("_DetailMask",false), 
//			new ShaderTextureProperty("_DetailAlbedoMap",false), 
//			new ShaderTextureProperty("_DetailNormalMap",true),
		};

        [SerializeField]
        protected MB2_TextureBakeResults _textureBakeResults;
        public MB2_TextureBakeResults textureBakeResults
        {
            get { return _textureBakeResults; }
            set { _textureBakeResults = value; }
        }

        [SerializeField]
        protected int _atlasPadding = 1;
        public int atlasPadding
        {
            get { return _atlasPadding; }
            set { _atlasPadding = value; }
        }

        [SerializeField]
        protected int _maxAtlasSize = 1;
        public int maxAtlasSize
        {
            get { return _maxAtlasSize; }
            set { _maxAtlasSize = value; }
        }

        [SerializeField]
        protected bool _resizePowerOfTwoTextures = false;
        public bool resizePowerOfTwoTextures
        {
            get { return _resizePowerOfTwoTextures; }
            set { _resizePowerOfTwoTextures = value; }
        }

        [SerializeField]
        protected bool _fixOutOfBoundsUVs = false;
        public bool fixOutOfBoundsUVs
        {
            get { return _fixOutOfBoundsUVs; }
            set { _fixOutOfBoundsUVs = value; }
        }

        [SerializeField]
        protected int _maxTilingBakeSize = 1024;
        public int maxTilingBakeSize
        {
            get { return _maxTilingBakeSize; }
            set { _maxTilingBakeSize = value; }
        }

        [SerializeField]
        protected bool _saveAtlasesAsAssets = false;
        public bool saveAtlasesAsAssets
        {
            get { return _saveAtlasesAsAssets; }
            set { _saveAtlasesAsAssets = value; }
        }

        [SerializeField]
        protected MB2_PackingAlgorithmEnum _packingAlgorithm = MB2_PackingAlgorithmEnum.UnitysPackTextures;
        public MB2_PackingAlgorithmEnum packingAlgorithm
        {
            get { return _packingAlgorithm; }
            set { _packingAlgorithm = value; }
        }

        [SerializeField]
        protected bool _meshBakerTexturePackerForcePowerOfTwo = true;
        public bool meshBakerTexturePackerForcePowerOfTwo
        {
            get { return _meshBakerTexturePackerForcePowerOfTwo; }
            set { _meshBakerTexturePackerForcePowerOfTwo = value; }
        }

        [SerializeField]
        protected List<ShaderTextureProperty> _customShaderPropNames = new List<ShaderTextureProperty>();
        public List<ShaderTextureProperty> customShaderPropNames
        {
            get { return _customShaderPropNames; }
            set { _customShaderPropNames = value; }
        }

        [SerializeField]
        protected bool _normalizeTexelDensity = false;

        [SerializeField]
        protected bool _considerNonTextureProperties = false;
        public bool considerNonTextureProperties
        {
            get { return _considerNonTextureProperties; }
            set { _considerNonTextureProperties = value; }
        }

        protected TextureBlender resultMaterialTextureBlender;
        protected TextureBlender[] textureBlenders = new TextureBlender[0];

        //copies of textures created for the the atlas baking that should be destroyed in finalize
        protected List<Texture2D> _temporaryTextures = new List<Texture2D>();

        //so we can undo read flag on procedural materials in finalize
        protected List<ProceduralMaterialInfo> _proceduralMaterials = new List<ProceduralMaterialInfo>();

        /*
        Like a material but also stores its tiling info since the same texture
        with different tiling may need to be baked to a separate spot in the atlas
        note that it is sometimes possible for textures with different tiling to share an atlas rectangle
        To accomplish this need to store:
             uvTiling per TexSet (can be set to 0,0,1,1 by pushing tiling down into material tiling)
             matTiling per MeshBakerMaterialTexture (this is the total tiling baked into the atlas)
             matSubrectInFullSamplingRect per material (a MeshBakerMaterialTexture can be used by multiple materials. This is the subrect in the atlas)
        Normally UVTilings is applied first then material tiling after. This is difficult for us to use when baking meshes. It is better to apply material
        tiling first then UV Tiling. There is a transform for modifying the material tiling to handle this.
        once the material tiling is applied first then the uvTiling can be pushed down into the material tiling.
        
            Also note that this can wrap a procedural texture. The procedural texture is converted to a Texture2D in Step2 NOT BEFORE. This is important so that can
            build packing layout quickly. 
            
                Should always check if texture is null using 'isNull' function since Texture2D could be null but ProceduralTexture not
                Should not call GetTexture2D before procedural textures are created
        
            there will be one of these per material texture property (maintex, bump etc...)
        */
        public class MeshBakerMaterialTexture
        {
            //private ProceduralTexture _procT;
            private Texture2D _t;
            internal static bool readyToBuildAtlases = false;
            public Texture2D t
            {
                set { _t = value; }
            }

            //This should never be called until we are ready to build atlases
            public Texture2D GetTexture2D()
            {
                if (!readyToBuildAtlases)
                {
                    Debug.LogError("This function should not be called before Step3. For steps 1 and 2 should always call methods like isNull, width, height");
                    throw new Exception("GetTexture2D called before ready to build atlases");
                }
                return _t;
            }

            public bool isNull
            {
                get { return _t == null/* && _procT == null*/; }
            }

            public int width
            {
                get
                {
                    if (_t != null) return _t.width;
                    //else if (_procT != null) return _procT.width;
                    throw new Exception("Texture was null. can't get width");
                }
            }

            public int height
            {
                get
                {
                    if (_t != null) return _t.height;
                    //else if (_procT != null) return _procT.height;
                    throw new Exception("Texture was null. can't get height");
                }
            }

            public float texelDensity; //how many pixels per polygon area

            //if these are the same for all properties then these can be merged
            public DRect encapsulatingSamplingRect; //sampling rect including both material tiling and uv Tiling
            public DRect matTilingRect;

            public MeshBakerMaterialTexture() { }
            public MeshBakerMaterialTexture(Texture tx)
            {
                if (tx is Texture2D)
                {
                    _t = (Texture2D)tx;
                }
                //else if (tx is ProceduralTexture)
                //{
                //    _procT = (ProceduralTexture)tx;
                //}
                else if (tx == null)
                {
                    //do nothing
                }
                else
                {
                    Debug.LogError("An error occured. Texture must be Texture2D " + tx);
                }
            }
            public MeshBakerMaterialTexture(Texture tx, Vector2 o, Vector2 s, float texelDens)
            {
                if (tx is Texture2D)
                {
                    _t = (Texture2D)tx;
                }
                //else if (tx is ProceduralTexture)
                //{
                //    _procT = (ProceduralTexture)tx;
                //}
                else if (tx == null)
                {
                    //do nothing
                }
                else
                {
                    Debug.LogError("An error occured. Texture must be Texture2D " + tx);
                }
                matTilingRect = new DRect(o, s);
                texelDensity = texelDens;
            }

            public string GetTexName()
            {
                if (_t != null) return _t.name;
                //else if (_procT != null) return _procT.name;
                return "";
            }

            public bool AreTexturesEqual(MeshBakerMaterialTexture b)
            {
                if (_t == b._t /*&& _procT == b._procT*/) return true;
                return false;
            }

            /*
            public bool IsProceduralTexture()
            {
                return (_procT != null);
            }
            
            public ProceduralTexture GetProceduralTexture()
            {
                return _procT;
            }

            public Texture2D ConvertProceduralToTexture2D(List<Texture2D> temporaryTextures)
            {
                int w = _procT.width;
                int h = _procT.height;

                bool mips = true;
                bool isLinear = false;
                GC.Collect(3, GCCollectionMode.Forced);
                Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, mips, isLinear);
                Color32[] pixels = _procT.GetPixels32(0, 0, w, h);
                tex.SetPixels32(0, 0, w, h, pixels);
                tex.Apply();
                tex.name = _procT.name;
                temporaryTextures.Add(tex);
                return tex;
            }
            */
        }


        public class MatAndTransformToMerged
        {
            public Material mat;
            /*MB_TexSets can be merged into a combined MB_TexSet if the texutures overlap enough. When this happens
            the materials may not use the whole rect in the atlas. This property defines the rect relative to the combined
            source rect that was used */
            //public DRect atlasSubrectMaterialOnly = new DRect(0f, 0f, 1f, 1f);

            public DRect obUVRectIfTilingSame = new DRect(0f, 0f, 1f, 1f);
            public DRect samplingRectMatAndUVTiling = new DRect(); //cached value this is full sampling rect used by this material. It is not updated as material sampling rects get merged. It will always be source sampling rect.
            public DRect materialTiling = new DRect();
            public string objName;

            public MatAndTransformToMerged(Material m)
            {
                mat = m;
            }

            public override bool Equals(object obj)
            {
                if (obj is MatAndTransformToMerged)
                {
                    MatAndTransformToMerged o = (MatAndTransformToMerged)obj;


                    if (o.mat == mat && o.obUVRectIfTilingSame == obUVRectIfTilingSame)
                    {
                        return true;
                    }
                }
                return false;
            }

            public override int GetHashCode()
            {
                return mat.GetHashCode() ^ obUVRectIfTilingSame.GetHashCode() ^ samplingRectMatAndUVTiling.GetHashCode();
            }

        }

        public class SamplingRectEnclosesComparer : IComparer<MatAndTransformToMerged>
        {
            public int Compare(MatAndTransformToMerged x, MatAndTransformToMerged y)
            {
                if (x.samplingRectMatAndUVTiling.Equals(y.samplingRectMatAndUVTiling))
                {
                    return 0;
                }
                else if (x.samplingRectMatAndUVTiling.Encloses(y.samplingRectMatAndUVTiling))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        public class MatsAndGOs
        {
            public List<MatAndTransformToMerged> mats;
            public List<GameObject> gos;
        }

        //a set of textures one for each "maintex","bump" that one or more materials use.
        public class MB_TexSet
        {
            public MeshBakerMaterialTexture[] ts; //one per "maintex", "bump"

            public MatsAndGOs matsAndGOs;

            //public TextureBlender textureBlender; //only used if _considerNonTextureProperties is true
            public bool allTexturesUseSameMatTiling = false;

            public Vector2 obUVoffset = new Vector2(0f, 0f);
            public Vector2 obUVscale = new Vector2(1f, 1f);
            public int idealWidth; //all textures will be resized to this size
            public int idealHeight;

            public DRect obUVrect
            {
                get { return new DRect(obUVoffset, obUVscale); }
            }

            public MB_TexSet(MeshBakerMaterialTexture[] tss, Vector2 uvOffset, Vector2 uvScale)
            {
                ts = tss;
                obUVoffset = uvOffset;
                obUVscale = uvScale;
                allTexturesUseSameMatTiling = false;
                matsAndGOs = new MatsAndGOs();
                matsAndGOs.mats = new List<MatAndTransformToMerged>();
                matsAndGOs.gos = new List<GameObject>();
            }

            // The two texture sets are equal if they are using the same 
            // textures/color properties for each map and have the same
            // tiling for each of those color properties
            public bool IsEqual(object obj, bool fixOutOfBoundsUVs, bool considerNonTextureProperties, TextureBlender resultMaterialTextureBlender)
            {
                if (!(obj is MB_TexSet))
                {
                    return false;
                }
                MB_TexSet other = (MB_TexSet)obj;
                if (other.ts.Length != ts.Length)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < ts.Length; i++)
                    {
                        if (ts[i].matTilingRect != other.ts[i].matTilingRect)
                            return false;
                        if (!ts[i].AreTexturesEqual(other.ts[i]))
                            return false;
                        if (considerNonTextureProperties)
                        {
                            if (resultMaterialTextureBlender != null)
                            {
                                if (!resultMaterialTextureBlender.NonTexturePropertiesAreEqual(matsAndGOs.mats[0].mat, other.matsAndGOs.mats[0].mat))
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    //IMPORTANT don't use Vector2 != Vector2 because it is only acurate to about 5 decimal places
                    //this can lead to tiled rectangles that can't accept rectangles.
                    if (fixOutOfBoundsUVs && (obUVoffset.x != other.obUVoffset.x ||
                                              obUVoffset.y != other.obUVoffset.y))
                        return false;
                    if (fixOutOfBoundsUVs && (obUVscale.x != other.obUVscale.x ||
                                             obUVscale.y != other.obUVscale.y))
                        return false;
                    return true;
                }
            }

            //assumes all materials use the same obUVrects.
            public void CalcInitialFullSamplingRects(bool fixOutOfBoundsUVs)
            {
                DRect validFullSamplingRect = new Core.DRect(0, 0, 1, 1);
                if (fixOutOfBoundsUVs)
                {
                    validFullSamplingRect = obUVrect;
                }

                for (int propIdx = 0; propIdx < ts.Length; propIdx++)
                {
                    if (!ts[propIdx].isNull)
                    {
                        DRect matTiling = ts[propIdx].matTilingRect;
                        DRect ruv;
                        if (fixOutOfBoundsUVs)
                        {
                            ruv = obUVrect;
                        }
                        else
                        {
                            ruv = new DRect(0.0, 0.0, 1.0, 1.0);
                        }
                        ts[propIdx].encapsulatingSamplingRect = MB3_UVTransformUtility.CombineTransforms(ref ruv, ref matTiling);
                        validFullSamplingRect = ts[propIdx].encapsulatingSamplingRect;

                    }
                }
                //if some of the textures were null make them match the sampling of one of the other textures
                for (int propIdx = 0; propIdx < ts.Length; propIdx++)
                {
                    if (ts[propIdx].isNull)
                    {
                        ts[propIdx].encapsulatingSamplingRect = validFullSamplingRect;
                    }
                }
            }

            public void CalcMatAndUVSamplingRects()
            {
                if (allTexturesUseSameMatTiling)
                {
                    DRect matTiling = new DRect(0f, 0f, 1f, 1f);
                    for (int propIdx = 0; propIdx < ts.Length; propIdx++)
                    {
                        if (!ts[propIdx].isNull)
                        {
                            matTiling = ts[propIdx].matTilingRect;
                        }
                    }
                    for (int matIdx = 0; matIdx < matsAndGOs.mats.Count; matIdx++)
                    {
                        matsAndGOs.mats[matIdx].materialTiling = matTiling;
                        matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling = MB3_UVTransformUtility.CombineTransforms(ref matsAndGOs.mats[matIdx].obUVRectIfTilingSame, ref matTiling);  //MB3_TextureCombiner.GetSourceSamplingRect(ts[matIdx], obUVoffset, obUVscale);
                    }
                }
                else
                {
                    // material tiling is different for each texture property
                    // in this case we set the material tiling to 1 here
                    for (int matIdx = 0; matIdx < matsAndGOs.mats.Count; matIdx++)
                    {
                        DRect matTiling = new DRect(0f, 0f, 1f, 1f);
                        matsAndGOs.mats[matIdx].materialTiling = matTiling;
                        matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling = MB3_UVTransformUtility.CombineTransforms(ref matsAndGOs.mats[matIdx].obUVRectIfTilingSame, ref matTiling);  //MB3_TextureCombiner.GetSourceSamplingRect(ts[matIdx], obUVoffset, obUVscale);
                    }
                }
            }

            public bool AllTexturesAreSameForMerge(MB_TexSet other, /*bool considerTintColor*/ bool considerNonTextureProperties, TextureBlender resultMaterialTextureBlender)
            {
                if (other.ts.Length != ts.Length)
                {
                    return false;
                }
                else
                {
                    if (!other.allTexturesUseSameMatTiling || !allTexturesUseSameMatTiling)
                    {
                        return false;
                    }
                    // must use same set of textures
                    int idxOfFirstNoneNull = -1;
                    for (int i = 0; i < ts.Length; i++)
                    {
                        if (!ts[i].AreTexturesEqual(other.ts[i]))
                            return false;
                        if (idxOfFirstNoneNull == -1 && !ts[i].isNull)
                        {
                            idxOfFirstNoneNull = i;
                        }
                        if (considerNonTextureProperties)
                        {
                            if (resultMaterialTextureBlender != null)
                            {
                                if (!resultMaterialTextureBlender.NonTexturePropertiesAreEqual(matsAndGOs.mats[0].mat, other.matsAndGOs.mats[0].mat))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    if (idxOfFirstNoneNull != -1)
                    {
                        //check that all textures are the same. Have already checked all tiling is same
                        for (int i = 0; i < ts.Length; i++)
                        {
                            if (!ts[i].AreTexturesEqual(other.ts[i]))
                            {
                                return false;
                            }
                        }

                        //=========================================================
                        // OLD check less strict
                        //When comparting two sets of textures (main, bump, spec ...) A and B that have different scales & offsets.They can share if:
                        //    - the scales of each texPropertyName (main, bump ...) are the same ratio: ASmain / BSmain = ASbump / BSbump = ASspec / BSspec
                        //    - the offset of A to B in uv space is the same for each texPropertyName:
                        //        offset = final - initial = OA / SB - OB must be the same
                        /*
                        MeshBakerMaterialTexture ma = ts[idxOfFirstNoneNull];
                        MeshBakerMaterialTexture mb = other.ts[idxOfFirstNoneNull];
                        //construct a rect that will ratio and offset
                        DRect r1 = new DRect(   (ma.matTilingRect.x / mb.matTilingRect.width - mb.matTilingRect.x),
                                                (ma.matTilingRect.y / mb.matTilingRect.height - mb.matTilingRect.y),
                                                (mb.matTilingRect.width / ma.matTilingRect.width),
                                                (mb.matTilingRect.height / ma.matTilingRect.height));
                        for (int i = 0; i < ts.Length; i++)
                        {
                            if (ts[i].t != null)
                            {
                                ma = ts[i];
                                mb = other.ts[i];
                                DRect r2 = new DRect(   (ma.matTilingRect.x / mb.matTilingRect.width - mb.matTilingRect.x),
                                                        (ma.matTilingRect.y / mb.matTilingRect.height - mb.matTilingRect.y),
                                                        (mb.matTilingRect.width / ma.matTilingRect.width),
                                                        (mb.matTilingRect.height / ma.matTilingRect.height));
                                if (Math.Abs(r2.x - r1.x) > 10e-10f) return false;
                                if (Math.Abs(r2.y - r1.y) > 10e-10f) return false;
                                if (Math.Abs(r2.width - r1.width) > 10e-10f) return false;
                                if (Math.Abs(r2.height - r1.height) > 10e-10f) return false;
                            }
                        }
                        */
                    }
                    return true;
                }
            }

            internal string GetDescription()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[GAME_OBJS=");
                for (int i = 0; i < matsAndGOs.gos.Count; i++)
                {
                    sb.AppendFormat("{0},", matsAndGOs.gos[i].name);
                }
                sb.AppendFormat("MATS=");
                for (int i = 0; i < matsAndGOs.mats.Count; i++)
                {
                    sb.AppendFormat("{0},", matsAndGOs.mats[i].mat.name);
                }
                sb.Append("]");
                return sb.ToString();
            }

            internal string GetMatSubrectDescriptions()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < matsAndGOs.mats.Count; i++)
                {
                    sb.AppendFormat("\n    {0}={1},", matsAndGOs.mats[i].mat.name, matsAndGOs.mats[i].samplingRectMatAndUVTiling);
                }
                return sb.ToString();
            }
        }

        protected class ProceduralMaterialInfo
        {
            public ProceduralMaterial proceduralMat;
            public bool originalIsReadableVal;
        }

        //This runs a coroutine without pausing it is used to build the textures from the editor
        public static bool _RunCorutineWithoutPauseIsRunning = false;
        public static void RunCorutineWithoutPause(IEnumerator cor, int recursionDepth)
        {
            if (recursionDepth == 0)
            {
                _RunCorutineWithoutPauseIsRunning = true;
            }
            if (recursionDepth > 20)
            {
                Debug.LogError("Recursion Depth Exceeded.");
                return;
            }
            while (cor.MoveNext())
            {
                object retObj = cor.Current;
                if (retObj is YieldInstruction)
                {
                    //do nothing
                }
                else if (retObj == null)
                {
                    //do nothing
                }
                else if (retObj is IEnumerator)
                {
                    RunCorutineWithoutPause((IEnumerator)cor.Current, recursionDepth + 1);
                }
            }
            if (recursionDepth == 0)
            {
                _RunCorutineWithoutPauseIsRunning = false;
            }
        }

        /**<summary>Combines meshes and generates texture atlases. NOTE running coroutines at runtime does not work in Unity 4</summary>
	    *  <param name="progressInfo">A delegate function that will be called to report progress.</param>
	    *  <param name="textureEditorMethods">If called from the editor should be an instance of MB2_EditorMethods. If called at runtime should be null.</param>
	    *  <remarks>Combines meshes and generates texture atlases</remarks> */
        public bool CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods = null, List<AtlasPackingResult> packingResults = null, bool onlyPackRects = false)
        {
            CombineTexturesIntoAtlasesCoroutineResult result = new CombineTexturesIntoAtlasesCoroutineResult();
            RunCorutineWithoutPause(_CombineTexturesIntoAtlases(progressInfo, result, resultAtlasesAndRects, resultMaterial, objsToMesh, allowedMaterialsFilter, textureEditorMethods, packingResults, onlyPackRects), 0);
            return result.success;
        }

        /**
         Same as CombineTexturesIntoAtlases except this version runs as a coroutine to spread the load of baking textures at runtime across several frames
         */

        public class CombineTexturesIntoAtlasesCoroutineResult
        {
            public bool success = true;
            public bool isFinished = false;
        }


        //float _maxTimePerFrameForCoroutine;
        public IEnumerator CombineTexturesIntoAtlasesCoroutine(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods = null, CombineTexturesIntoAtlasesCoroutineResult coroutineResult = null, float maxTimePerFrame = .01f, List<AtlasPackingResult> packingResults = null, bool onlyPackRects = false)
        {
            if (!_RunCorutineWithoutPauseIsRunning && (MBVersion.GetMajorVersion() < 5 || (MBVersion.GetMajorVersion() == 5 && MBVersion.GetMinorVersion() < 3)))
            {
                Debug.LogError("Running the texture combiner as a coroutine only works in Unity 5.3 and higher");
                yield return null;
            }
            coroutineResult.success = true;
            coroutineResult.isFinished = false;
            if (maxTimePerFrame <= 0f)
            {
                Debug.LogError("maxTimePerFrame must be a value greater than zero");
                coroutineResult.isFinished = true;
                yield break;
            }
            //_maxTimePerFrameForCoroutine = maxTimePerFrame;
            yield return _CombineTexturesIntoAtlases(progressInfo, coroutineResult, resultAtlasesAndRects, resultMaterial, objsToMesh, allowedMaterialsFilter, textureEditorMethods, packingResults, onlyPackRects);
            coroutineResult.isFinished = true;
            yield break;
        }

#if UNITY_WSA && !UNITY_EDITOR
        //not defined for WSA runtime
#else 
        static bool InterfaceFilter(Type typeObj, System.Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }
#endif

        void _LoadTextureBlenders()
        {
#if UNITY_WSA && !UNITY_EDITOR
        //not defined for WSA runtime
#else   
            string qualifiedInterfaceName = "DigitalOpus.MB.Core.TextureBlender";
            var interfaceFilter = new TypeFilter(InterfaceFilter);
            List<Type> types = new List<Type>();
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Collections.IEnumerable typesIterator = null;
                try
                {
                    typesIterator = ass.GetTypes();
                }
                catch (Exception e)
                {
                    //Debug.Log("The assembly that I could not read types for was: " + ass.GetName());
                    //suppress error
                    e.Equals(null);
                }
                if (typesIterator != null)
                {
                    foreach (Type ty in ass.GetTypes())
                    {
                        var myInterfaces = ty.FindInterfaces(interfaceFilter, qualifiedInterfaceName);
                        if (myInterfaces.Length > 0)
                        {
                            types.Add(ty);
                        }
                    }
                }
            }
            TextureBlender fallbackTB = null;
            List<TextureBlender> textureBlendersList = new List<TextureBlender>();
            foreach (Type tt in types)
            {
                if (!tt.IsAbstract && !tt.IsInterface)
                {
                    TextureBlender instance = (TextureBlender)Activator.CreateInstance(tt);
                    if (instance is TextureBlenderFallback)
                    {
                        fallbackTB = instance;
                    }
                    else
                    {
                        textureBlendersList.Add(instance);
                    }
                }
            }
            if (fallbackTB != null) textureBlendersList.Add(fallbackTB); // must come last in list
            textureBlenders = textureBlendersList.ToArray();
            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                Debug.Log(string.Format("Loaded {0} TextureBlenders.", textureBlenders.Length));
            }
#endif
        }

        bool _CollectPropertyNames(Material resultMaterial, List<ShaderTextureProperty> texPropertyNames)
        {
            //try custom properties remove duplicates
            for (int i = 0; i < texPropertyNames.Count; i++)
            {
                ShaderTextureProperty s = _customShaderPropNames.Find(x => x.name.Equals(texPropertyNames[i].name));
                if (s != null)
                {
                    _customShaderPropNames.Remove(s);
                }
            }

            Material m = resultMaterial;
            if (m == null)
            {
                Debug.LogError("Please assign a result material. The combined mesh will use this material.");
                return false;
            }

            //Collect the property names for the textures
            string shaderPropStr = "";
            for (int i = 0; i < shaderTexPropertyNames.Length; i++)
            {
                if (m.HasProperty(shaderTexPropertyNames[i].name))
                {
                    shaderPropStr += ", " + shaderTexPropertyNames[i].name;
                    if (!texPropertyNames.Contains(shaderTexPropertyNames[i])) texPropertyNames.Add(shaderTexPropertyNames[i]);
                    if (m.GetTextureOffset(shaderTexPropertyNames[i].name) != new Vector2(0f, 0f))
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material has non-zero offset. This is may be incorrect.");
                    }
                    if (m.GetTextureScale(shaderTexPropertyNames[i].name) != new Vector2(1f, 1f))
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material should have tiling of 1,1");
                    }
                }
            }

            for (int i = 0; i < _customShaderPropNames.Count; i++)
            {
                if (m.HasProperty(_customShaderPropNames[i].name))
                {
                    shaderPropStr += ", " + _customShaderPropNames[i].name;
                    texPropertyNames.Add(_customShaderPropNames[i]);
                    if (m.GetTextureOffset(_customShaderPropNames[i].name) != new Vector2(0f, 0f))
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material has non-zero offset. This is probably incorrect.");
                    }
                    if (m.GetTextureScale(_customShaderPropNames[i].name) != new Vector2(1f, 1f))
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material should probably have tiling of 1,1.");
                    }
                }
                else
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Result material shader does not use property " + _customShaderPropNames[i].name + " in the list of custom shader property names");
                }
            }

            return true;
        }

        IEnumerator _CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, CombineTexturesIntoAtlasesCoroutineResult result, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods, List<AtlasPackingResult> atlasPackingResult, bool onlyPackRects)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                _temporaryTextures.Clear();
                MeshBakerMaterialTexture.readyToBuildAtlases = false;

                if (textureEditorMethods != null)
                {
                    textureEditorMethods.Clear();
                    textureEditorMethods.OnPreTextureBake();
                }

                if (objsToMesh == null || objsToMesh.Count == 0)
                {
                    Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
                    result.success = false;
                    yield break;
                }
                if (_atlasPadding < 0)
                {
                    Debug.LogError("Atlas padding must be zero or greater.");
                    result.success = false;
                    yield break;
                }
                if (_maxTilingBakeSize < 2 || _maxTilingBakeSize > 4096)
                {
                    Debug.LogError("Invalid value for max tiling bake size.");
                    result.success = false;
                    yield break;
                }
                for (int i = 0; i < objsToMesh.Count; i++)
                {
                    Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
                    for (int j = 0; j < ms.Length; j++)
                    {
                        Material m = ms[j];
                        if (m == null)
                        {
                            Debug.LogError("Game object " + objsToMesh[i] + " has a null material");
                            result.success = false;
                            yield break;
                        }

                    }
                }


                if (progressInfo != null)
                    progressInfo("Collecting textures for " + objsToMesh.Count + " meshes.", .01f);

                List<ShaderTextureProperty> texPropertyNames = new List<ShaderTextureProperty>();
                if (!_CollectPropertyNames(resultMaterial, texPropertyNames))
                {
                    result.success = false;
                    yield break;
                }
                if (_fixOutOfBoundsUVs && (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal ||
                                            _packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical))
                {
                    if (LOG_LEVEL >= MB2_LogLevel.info)
                    {
                        Debug.LogWarning("'Consider Mesh UVs' is enabled but packing algorithm is MeshBakerTexturePacker_Horizontal or MeshBakerTexturePacker_Vertical. It is recommended to use these packers without using 'Consider Mesh UVs'");
                    }
                }
                if (_considerNonTextureProperties)
                {
                    _LoadTextureBlenders();
                    resultMaterialTextureBlender = FindMatchingTextureBlender(resultMaterial.shader.name);
                    if (resultMaterialTextureBlender != null)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Using _considerNonTextureProperties found a TextureBlender for result material. Using: " + resultMaterialTextureBlender);
                    }
                    else
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.error) Debug.LogWarning("Using _considerNonTextureProperties could not find a TextureBlender that matches the shader on the result material. Using the Fallback Texture Blender.");
                        resultMaterialTextureBlender = new TextureBlenderFallback();
                    }
                }

                if (onlyPackRects)
                {
                    yield return __RunTexturePacker(result, texPropertyNames, objsToMesh, allowedMaterialsFilter, textureEditorMethods, atlasPackingResult);
                }
                else
                {
                    yield return __CombineTexturesIntoAtlases(progressInfo, result, resultAtlasesAndRects, resultMaterial, texPropertyNames, objsToMesh, allowedMaterialsFilter, textureEditorMethods);
                }
                /*
			} catch (MissingReferenceException mrex){
				Debug.LogError("Creating atlases failed a MissingReferenceException was thrown. This is normally only happens when trying to create very large atlases and Unity is running out of Memory. Try changing the 'Texture Packer' to a different option, it may work with an alternate packer. This error is sometimes intermittant. Try baking again.");
				Debug.LogError(mrex);
			} catch (Exception ex){
				Debug.LogError(ex);*/
            }
            finally
            {
                _destroyTemporaryTextures();
                _restoreProceduralMaterials();
                if (textureEditorMethods != null)
                {
                    textureEditorMethods.RestoreReadFlagsAndFormats(progressInfo);
                    textureEditorMethods.OnPostTextureBake();
                }
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("===== Done creating atlases for " + resultMaterial + " Total time to create atlases " + sw.Elapsed.ToString());
            }
            //result.success = success;
        }


        //texPropertyNames is the list of texture properties in the resultMaterial
        //allowedMaterialsFilter is a list of materials. Objects without any of these materials will be ignored.
        //						 this is used by the multiple materials filter
        //textureEditorMethods encapsulates editor only functionality such as saving assets and tracking texture assets whos format was changed. Is null if using at runtime. 
        IEnumerator __CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, CombineTexturesIntoAtlasesCoroutineResult result, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<ShaderTextureProperty> texPropertyNames, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods)
        {
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("__CombineTexturesIntoAtlases texture properties in shader:" + texPropertyNames.Count + " objsToMesh:" + objsToMesh.Count + " _fixOutOfBoundsUVs:" + _fixOutOfBoundsUVs);

            if (progressInfo != null) progressInfo("Collecting textures ", .01f);
            /*
			each atlas (maintex, bump, spec etc...) will have distinctMaterialTextures.Count images in it.
			each distinctMaterialTextures record is a set of textures, one for each atlas. And a list of materials
			that use that distinct set of textures. 
			*/
            List<MB_TexSet> distinctMaterialTextures = new List<MB_TexSet>(); //one per distinct set of textures
            List<GameObject> usedObjsToMesh = new List<GameObject>();
            yield return __Step1_CollectDistinctMatTexturesAndUsedObjects(progressInfo, result, objsToMesh, allowedMaterialsFilter, texPropertyNames, textureEditorMethods, distinctMaterialTextures, usedObjsToMesh);
            if (!result.success)
            {
                yield break;
            }

            if (MB3_MeshCombiner.EVAL_VERSION)
            {
                bool usesAllowedShaders = true;
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    for (int j = 0; j < distinctMaterialTextures[i].matsAndGOs.mats.Count; j++)
                    {
                        if (!distinctMaterialTextures[i].matsAndGOs.mats[j].mat.shader.name.EndsWith("Diffuse") &&
                            !distinctMaterialTextures[i].matsAndGOs.mats[j].mat.shader.name.EndsWith("Bumped Diffuse"))
                        {
                            Debug.LogError("The free version of Mesh Baker only works with Diffuse and Bumped Diffuse Shaders. The full version can be used with any shader. Material " + distinctMaterialTextures[i].matsAndGOs.mats[j].mat.name + " uses shader " + distinctMaterialTextures[i].matsAndGOs.mats[j].mat.shader.name);
                            usesAllowedShaders = false;
                        }
                    }
                }
                if (!usesAllowedShaders)
                {
                    result.success = false;
                    yield break;
                }
            }

            //Textures in each material (_mainTex, Bump, Spec ect...) must be same size
            //Calculate the best sized to use. Takes into account tiling
            //if only one texture in atlas re-uses original sizes	
            bool[] allTexturesAreNullAndSameColor = new bool[texPropertyNames.Count];
            yield return __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(progressInfo, result, distinctMaterialTextures, texPropertyNames, allTexturesAreNullAndSameColor, textureEditorMethods);
            if (!result.success)
            {
                yield break;
            }

            int _padding = __step2_CalculateIdealSizesForTexturesInAtlasAndPadding;

            //    buildAndSaveAtlases
            yield return __Step3_BuildAndSaveAtlasesAndStoreResults(result, progressInfo, distinctMaterialTextures, texPropertyNames, allTexturesAreNullAndSameColor, _padding, textureEditorMethods, resultAtlasesAndRects, resultMaterial);
        }

        IEnumerator __RunTexturePacker(CombineTexturesIntoAtlasesCoroutineResult result, List<ShaderTextureProperty> texPropertyNames, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, MB2_EditorMethodsInterface textureEditorMethods, List<AtlasPackingResult> packingResult)
        {
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("__RunTexturePacker texture properties in shader:" + texPropertyNames.Count + " objsToMesh:" + objsToMesh.Count + " _fixOutOfBoundsUVs:" + _fixOutOfBoundsUVs);

            List<MB_TexSet> distinctMaterialTextures = new List<MB_TexSet>(); //one per distinct set of textures
            List<GameObject> usedObjsToMesh = new List<GameObject>();
            yield return __Step1_CollectDistinctMatTexturesAndUsedObjects(null, result, objsToMesh, allowedMaterialsFilter, texPropertyNames, textureEditorMethods, distinctMaterialTextures, usedObjsToMesh);
            if (!result.success)
            {
                yield break;
            }

            bool[] allTexturesAreNullAndSameColor = new bool[texPropertyNames.Count];
            yield return __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(null, result, distinctMaterialTextures, texPropertyNames, allTexturesAreNullAndSameColor, textureEditorMethods);
            if (!result.success)
            {
                yield break;
            }

            int _padding = __step2_CalculateIdealSizesForTexturesInAtlasAndPadding;

            //    run the texture packer only
            AtlasPackingResult[] aprs = __Step3_RunTexturePacker(distinctMaterialTextures, _padding);
            for (int i = 0; i < aprs.Length; i++)
            {
                packingResult.Add(aprs[i]);
            }
        }

        //Fills distinctMaterialTextures (a list of TexSets) and usedObjsToMesh. Each TexSet is a rectangle in the set of atlases.
        //If allowedMaterialsFilter is empty then all materials on allObjsToMesh will be collected and usedObjsToMesh will be same as allObjsToMesh
        //else only materials in allowedMaterialsFilter will be included and usedObjsToMesh will be objs that use those materials.
        //bool __step1_CollectDistinctMatTexturesAndUsedObjects;
        IEnumerator __Step1_CollectDistinctMatTexturesAndUsedObjects(ProgressUpdateDelegate progressInfo, CombineTexturesIntoAtlasesCoroutineResult result,
                                                                List<GameObject> allObjsToMesh,
                                                             List<Material> allowedMaterialsFilter,
                                                             List<ShaderTextureProperty> texPropertyNames,
                                                             MB2_EditorMethodsInterface textureEditorMethods,
                                                             List<MB_TexSet> distinctMaterialTextures, //Will be populated
                                                             List<GameObject> usedObjsToMesh) //Will be populated, is a subset of allObjsToMesh
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            // Collect distinct list of textures to combine from the materials on objsToCombine
            bool outOfBoundsUVs = false;
            Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>(); //cache results
            for (int i = 0; i < allObjsToMesh.Count; i++)
            {
                GameObject obj = allObjsToMesh[i];
                if (progressInfo != null) progressInfo("Collecting textures for " + obj, ((float)i) / allObjsToMesh.Count / 2f);
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Collecting textures for object " + obj);

                if (obj == null)
                {
                    Debug.LogError("The list of objects to mesh contained nulls.");
                    result.success = false;
                    yield break;
                }

                Mesh sharedMesh = MB_Utility.GetMesh(obj);
                if (sharedMesh == null)
                {
                    Debug.LogError("Object " + obj.name + " in the list of objects to mesh has no mesh.");
                    result.success = false;
                    yield break;
                }

                Material[] sharedMaterials = MB_Utility.GetGOMaterials(obj);
                if (sharedMaterials.Length == 0)
                {
                    Debug.LogError("Object " + obj.name + " in the list of objects has no materials.");
                    result.success = false;
                    yield break;
                }

                //analyze mesh or grab cached result of previous analysis, stores one result for each submesh
                MB_Utility.MeshAnalysisResult[] mar;
                if (!meshAnalysisResultsCache.TryGetValue(sharedMesh.GetInstanceID(), out mar))
                {
                    mar = new MB_Utility.MeshAnalysisResult[sharedMesh.subMeshCount];
                    for (int j = 0; j < sharedMesh.subMeshCount; j++)
                    {
                        MB_Utility.hasOutOfBoundsUVs(sharedMesh, ref mar[j], j);
                        if (_normalizeTexelDensity)
                        {
                            mar[j].submeshArea = GetSubmeshArea(sharedMesh, j);
                        }
                        if (_fixOutOfBoundsUVs && !mar[j].hasUVs)
                        {
                            //assume UVs will be generated if this feature is being used and generated UVs will be 0,0,1,1
                            mar[j].uvRect = new Rect(0, 0, 1, 1);
                            Debug.LogWarning("Mesh for object " + obj + " has no UV channel but 'consider UVs' is enabled. Assuming UVs will be generated filling 0,0,1,1 rectangle.");
                        }
                    }
                    meshAnalysisResultsCache.Add(sharedMesh.GetInstanceID(), mar);
                }
                if (_fixOutOfBoundsUVs && LOG_LEVEL >= MB2_LogLevel.trace)
                {
                    Debug.Log("Mesh Analysis for object " + obj + " numSubmesh=" + mar.Length + " HasOBUV=" + mar[0].hasOutOfBoundsUVs + " UVrectSubmesh0=" + mar[0].uvRect);
                }

                for (int matIdx = 0; matIdx < sharedMaterials.Length; matIdx++)
                { //for each submesh
                    if (progressInfo != null) progressInfo(String.Format("Collecting textures for {0} submesh {1}", obj, matIdx), ((float)i) / allObjsToMesh.Count / 2f);
                    Material mat = sharedMaterials[matIdx];

                    //check if this material is in the list of source materaials
                    if (allowedMaterialsFilter != null && !allowedMaterialsFilter.Contains(mat))
                    {
                        continue;
                    }

                    //Rect uvBounds = mar[matIdx].sourceUVRect;
                    outOfBoundsUVs = outOfBoundsUVs || mar[matIdx].hasOutOfBoundsUVs;

                    if (mat.name.Contains("(Instance)"))
                    {
                        Debug.LogError("The sharedMaterial on object " + obj.name + " has been 'Instanced'. This was probably caused by a script accessing the meshRender.material property in the editor. " +
                                       " The material to UV Rectangle mapping will be incorrect. To fix this recreate the object from its prefab or re-assign its material from the correct asset.");
                        result.success = false;
                        yield break;
                    }

                    if (_fixOutOfBoundsUVs)
                    {
                        if (!MB_Utility.AreAllSharedMaterialsDistinct(sharedMaterials))
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + obj.name + " uses the same material on multiple submeshes. This may generate strange resultAtlasesAndRects especially when used with fix out of bounds uvs. Try duplicating the material.");
                        }
                    }

                    //need to set up procedural material before converting its texs to texture2D
                    if (mat is ProceduralMaterial)
                    {
                        _addProceduralMaterial((ProceduralMaterial)mat);
                    }

                    //collect textures scale and offset for each texture in objects material
                    MeshBakerMaterialTexture[] mts = new MeshBakerMaterialTexture[texPropertyNames.Count];
                    for (int j = 0; j < texPropertyNames.Count; j++)
                    {
                        Texture tx = null;
                        Vector2 scale = Vector2.one;
                        Vector2 offset = Vector2.zero;
                        float texelDensity = 0f;
                        if (mat.HasProperty(texPropertyNames[j].name))
                        {
                            Texture txx = mat.GetTexture(texPropertyNames[j].name);
                            if (txx != null)
                            {
                                if (txx is Texture2D)
                                {
                                    tx = txx;
                                    TextureFormat f = ((Texture2D)tx).format;
                                    bool isNormalMap = false;
                                    if (!Application.isPlaying && textureEditorMethods != null) isNormalMap = textureEditorMethods.IsNormalMap((Texture2D)tx);
                                    if ((f == TextureFormat.ARGB32 ||
                                        f == TextureFormat.RGBA32 ||
                                        f == TextureFormat.BGRA32 ||
                                        f == TextureFormat.RGB24 ||
                                        f == TextureFormat.Alpha8) && !isNormalMap) //DXT5 does not work
                                    {
                                        //good
                                    }
                                    else
                                    {
                                        //TRIED to copy texture using tex2.SetPixels(tex1.GetPixels()) but bug in 3.5 means DTX1 and 5 compressed textures come out skewe
                                        if (Application.isPlaying && _packingAlgorithm != MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast)
                                        {
                                            Debug.LogError("Object " + obj.name + " in the list of objects to mesh uses Texture " + tx.name + " uses format " + f + " that is not in: ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT. These textures cannot be resized at runtime. Try changing texture format. If format says 'compressed' try changing it to 'truecolor'");
                                            result.success = false;
                                            yield break;
                                        }
                                        else
                                        {
                                            tx = (Texture2D)mat.GetTexture(texPropertyNames[j].name);
                                        }
                                    }
                                }

                                else if (txx is ProceduralTexture)
                                {
                                    //if (!MBVersion.IsTextureFormatRaw(((ProceduralTexture)txx).format))
                                    //{
                                    //    Debug.LogError("Object " + obj.name + " in the list of objects to mesh uses a ProceduarlTexture that is not in a RAW format. Convert textures to RAW.");
                                    //    result.success = false;
                                    //    yield break;
                                    //}
                                    tx = txx;
                                }

                                else
                                {
                                    Debug.LogError("Object " + obj.name + " in the list of objects to mesh uses a Texture that is not a Texture2D. Cannot build atlases.");
                                    result.success = false;
                                    yield break;
                                }

                            }

                            if (tx != null && _normalizeTexelDensity)
                            {
                                //todo this doesn't take into account tiling and out of bounds UV sampling
                                if (mar[j].submeshArea == 0)
                                {
                                    texelDensity = 0f;
                                }
                                else
                                {
                                    texelDensity = (tx.width * tx.height) / (mar[j].submeshArea);
                                }
                            }
                            scale = mat.GetTextureScale(texPropertyNames[j].name);
                            offset = mat.GetTextureOffset(texPropertyNames[j].name);
                        }
                        mts[j] = new MeshBakerMaterialTexture(tx, offset, scale, texelDensity);
                    }

                    Vector2 obUVscale = new Vector2(mar[matIdx].uvRect.width, mar[matIdx].uvRect.height);
                    Vector2 obUVoffset = new Vector2(mar[matIdx].uvRect.x, mar[matIdx].uvRect.y);

                    //Add to distinct set of textures if not already there
                    MB_TexSet setOfTexs = new MB_TexSet(mts, obUVoffset, obUVscale);  //one of these per submesh
                    MatAndTransformToMerged matt = new MatAndTransformToMerged(mat);
                    setOfTexs.matsAndGOs.mats.Add(matt);
                    MB_TexSet setOfTexs2 = distinctMaterialTextures.Find(x => x.IsEqual(setOfTexs, _fixOutOfBoundsUVs, _considerNonTextureProperties, resultMaterialTextureBlender));
                    if (setOfTexs2 != null)
                    {
                        setOfTexs = setOfTexs2;
                    }
                    else
                    {
                        distinctMaterialTextures.Add(setOfTexs);
                    }
                    if (!setOfTexs.matsAndGOs.mats.Contains(matt))
                    {
                        setOfTexs.matsAndGOs.mats.Add(matt);
                    }
                    if (!setOfTexs.matsAndGOs.gos.Contains(obj))
                    {
                        setOfTexs.matsAndGOs.gos.Add(obj);
                        if (!usedObjsToMesh.Contains(obj)) usedObjsToMesh.Add(obj);
                    }
                }
            }

            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log(String.Format("Step1_CollectDistinctTextures collected {0} sets of textures fixOutOfBoundsUV={1} considerNonTextureProperties={2}", distinctMaterialTextures.Count, _fixOutOfBoundsUVs, _considerNonTextureProperties));

            if (distinctMaterialTextures.Count == 0)
            {
                Debug.LogError("None of the source object materials matched any of the allowed materials for this submesh.");
                result.success = false;
                yield break;
            }

            MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects(distinctMaterialTextures, fixOutOfBoundsUVs);

            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Total time Step1_CollectDistinctTextures " + (sw.ElapsedMilliseconds).ToString("f5"));
            yield break;
        }

        int __step2_CalculateIdealSizesForTexturesInAtlasAndPadding;
        IEnumerator __Step2_CalculateIdealSizesForTexturesInAtlasAndPadding(ProgressUpdateDelegate progressInfo,
                                                                    CombineTexturesIntoAtlasesCoroutineResult result,
                                                                    List<MB_TexSet> distinctMaterialTextures,
                                                                    List<ShaderTextureProperty> texPropertyNames,
                                                                    bool[] allTexturesAreNullAndSameColor,
                                                                    MB2_EditorMethodsInterface textureEditorMethods)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            MeshBakerMaterialTexture.readyToBuildAtlases = true;

            // check if all textures are null and use same color for each atlas
            // will not generate an atlas if so
            for (int i = 0; i < texPropertyNames.Count; i++)
            {
                bool allTexturesAreNull = true;
                bool allNonTexturePropertiesAreSame = true;
                for (int j = 0; j < distinctMaterialTextures.Count; j++)
                {
                    if (!distinctMaterialTextures[j].ts[i].isNull)
                    {
                        allTexturesAreNull = false;
                        break;
                    }
                    else if (_considerNonTextureProperties)
                    {
                        for (int k = j + 1; k < distinctMaterialTextures.Count; k++)
                        {
                            Color colJ = resultMaterialTextureBlender.GetColorIfNoTexture(distinctMaterialTextures[j].matsAndGOs.mats[0].mat, texPropertyNames[i]);
                            Color colK = resultMaterialTextureBlender.GetColorIfNoTexture(distinctMaterialTextures[k].matsAndGOs.mats[0].mat, texPropertyNames[i]);
                            if (colJ != colK)
                            {
                                allNonTexturePropertiesAreSame = false;
                                break;
                            }
                        }
                    }

                }
                allTexturesAreNullAndSameColor[i] = allTexturesAreNull && allNonTexturePropertiesAreSame;
                if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log(String.Format("AllTexturesAreNullAndSameColor prop: {0} val:{1}", texPropertyNames[i].name, allTexturesAreNullAndSameColor[i]));
            }

            int _padding = _atlasPadding;
            if (distinctMaterialTextures.Count == 1 && _fixOutOfBoundsUVs == false)
            {
                if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("All objects use the same textures in this set of atlases. Original textures will be reused instead of creating atlases.");
                _padding = 0;
            }
            else
            {
                if (allTexturesAreNullAndSameColor.Length != texPropertyNames.Count)
                {
                    Debug.LogError("allTexturesAreNullAndSameColor array must be the same length of texPropertyNames.");
                }
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Calculating ideal sizes for texSet TexSet " + i + " of " + distinctMaterialTextures.Count);
                    MB_TexSet txs = distinctMaterialTextures[i];
                    txs.idealWidth = 1;
                    txs.idealHeight = 1;
                    int tWidth = 1;
                    int tHeight = 1;
                    if (txs.ts.Length != texPropertyNames.Count)
                    {
                        Debug.LogError("length of arrays in each element of distinctMaterialTextures must be texPropertyNames.Count");
                    }
                    //get the best size all textures in a TexSet must be the same size.
                    for (int propIdx = 0; propIdx < texPropertyNames.Count; propIdx++)
                    {
                        MeshBakerMaterialTexture matTex = txs.ts[propIdx];
                        if (!matTex.matTilingRect.size.Equals(Vector2.one) && distinctMaterialTextures.Count > 1)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Texture " + matTex.GetTexName() + "is tiled by " + matTex.matTilingRect.size + " tiling will be baked into a texture with maxSize:" + _maxTilingBakeSize);
                        }
                        if (!txs.obUVscale.Equals(Vector2.one) && distinctMaterialTextures.Count > 1 && _fixOutOfBoundsUVs)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Texture " + matTex.GetTexName() + "has out of bounds UVs that effectively tile by " + txs.obUVscale + " tiling will be baked into a texture with maxSize:" + _maxTilingBakeSize);
                        }
                        if (!allTexturesAreNullAndSameColor[propIdx] && matTex.isNull)
                        {
                            //create a small 16 x 16 texture to use in the atlas
                            if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("No source texture creating a 16x16 texture.");
                            matTex.t = _createTemporaryTexture(16, 16, TextureFormat.ARGB32, true);
                            if (_considerNonTextureProperties && resultMaterialTextureBlender != null)
                            {
                                Color col = resultMaterialTextureBlender.GetColorIfNoTexture(txs.matsAndGOs.mats[0].mat, texPropertyNames[propIdx]);
                                if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("Setting texture to solid color " + col);
                                MB_Utility.setSolidColor(matTex.GetTexture2D(), col);
                            }
                            else
                            {
                                Color col = GetColorIfNoTexture(texPropertyNames[propIdx]);
                                MB_Utility.setSolidColor(matTex.GetTexture2D(), col);
                            }
                            if (fixOutOfBoundsUVs)
                            {
                                matTex.encapsulatingSamplingRect = txs.obUVrect;
                            }
                            else
                            {
                                matTex.encapsulatingSamplingRect = new DRect(0, 0, 1, 1);
                            }
                        }

                        if (!matTex.isNull)
                        {
                            Vector2 dim = GetAdjustedForScaleAndOffset2Dimensions(matTex, txs.obUVoffset, txs.obUVscale);
                            if ((int)(dim.x * dim.y) > tWidth * tHeight)
                            {
                                if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("    matTex " + matTex.GetTexName() + " " + dim + " has a bigger size than " + tWidth + " " + tHeight);
                                tWidth = (int)dim.x;
                                tHeight = (int)dim.y;
                            }
                        }
                    }
                    if (_resizePowerOfTwoTextures)
                    {
                        if (tWidth <= _padding * 5)
                        {
                            Debug.LogWarning(String.Format("Some of the textures have widths close to the size of the padding. It is not recommended to use _resizePowerOfTwoTextures with widths this small.", txs.ToString()));
                        }
                        if (tHeight <= _padding * 5)
                        {
                            Debug.LogWarning(String.Format("Some of the textures have heights close to the size of the padding. It is not recommended to use _resizePowerOfTwoTextures with heights this small.", txs.ToString()));
                        }
                        if (IsPowerOfTwo(tWidth))
                        {
                            tWidth -= _padding * 2;
                        }
                        if (IsPowerOfTwo(tHeight))
                        {
                            tHeight -= _padding * 2;
                        }
                        if (tWidth < 1) tWidth = 1;
                        if (tHeight < 1) tHeight = 1;
                    }
                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("    Ideal size is " + tWidth + " " + tHeight);
                    txs.idealWidth = tWidth;
                    txs.idealHeight = tHeight;
                }
            }
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Total time Step2 Calculate Ideal Sizes part1: " + sw.Elapsed.ToString());
            //convert textures to readable formats here.
            if (distinctMaterialTextures.Count > 1)
            {
                //make procedural materials readable
                for (int i = 0; i < _proceduralMaterials.Count; i++)
                {
                    if (!_proceduralMaterials[i].proceduralMat.isReadable)
                    {
                        _proceduralMaterials[i].originalIsReadableVal = _proceduralMaterials[i].proceduralMat.isReadable;
                        _proceduralMaterials[i].proceduralMat.isReadable = true;
                        //textureEditorMethods.AddProceduralMaterialFormat(_proceduralMaterials[i].proceduralMat);
                        _proceduralMaterials[i].proceduralMat.RebuildTexturesImmediately();
                    }
                }
                //convert procedural textures to RAW format
                /*
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    for (int j = 0; j < texPropertyNames.Count; j++)
                    {
                        if (distinctMaterialTextures[i].ts[j].IsProceduralTexture())
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Converting procedural texture to Textur2D:" + distinctMaterialTextures[i].ts[j].GetTexName() + " property:" + texPropertyNames[i]);
                            Texture2D txx = distinctMaterialTextures[i].ts[j].ConvertProceduralToTexture2D(_temporaryTextures);
                            distinctMaterialTextures[i].ts[j].t = txx;
                        }
                    }
                }
                */
                if (_packingAlgorithm != MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast)
                {

                    for (int i = 0; i < distinctMaterialTextures.Count; i++)
                    {
                        for (int j = 0; j < texPropertyNames.Count; j++)
                        {
                            Texture tx = distinctMaterialTextures[i].ts[j].GetTexture2D();
                            if (tx != null)
                            {
                                if (textureEditorMethods != null)
                                {
                                    if (progressInfo != null) progressInfo(String.Format("Convert texture {0} to readable format ", tx), .5f);
                                    textureEditorMethods.AddTextureFormat((Texture2D)tx, texPropertyNames[j].isNormalMap);
                                }
                            }

                        }
                    }
                }
            }


            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Total time Step2 Calculate Ideal Sizes part2: " + sw.Elapsed.ToString());
            __step2_CalculateIdealSizesForTexturesInAtlasAndPadding = _padding;
            yield break;
        }

        AtlasPackingResult[] __Step3_RunTexturePacker(List<MB_TexSet> distinctMaterialTextures, int _padding)
        {
            AtlasPackingResult[] apr = __RuntTexturePackerOnly(distinctMaterialTextures, _padding);
            //copy materials PackingResults
            for (int i = 0; i < apr.Length; i++)
            {
                List<MatsAndGOs> matsList = new List<MatsAndGOs>();
                apr[i].data = matsList;
                for (int j = 0; j < apr[i].srcImgIdxs.Length; j++)
                {
                    MB3_TextureCombiner.MB_TexSet ts = distinctMaterialTextures[apr[i].srcImgIdxs[j]];
                    matsList.Add(ts.matsAndGOs);
                }
            }
            return apr;
        }

        IEnumerator __Step3_BuildAndSaveAtlasesAndStoreResults(CombineTexturesIntoAtlasesCoroutineResult result, ProgressUpdateDelegate progressInfo, List<MB_TexSet> distinctMaterialTextures, List<ShaderTextureProperty> texPropertyNames, bool[] allTexturesAreNullAndSameColor, int _padding, MB2_EditorMethodsInterface textureEditorMethods, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            // note that we may not create some of the atlases because all textures are null
            int numAtlases = texPropertyNames.Count;

            //generate report want to do this before
            StringBuilder report = new StringBuilder();
            if (numAtlases > 0)
            {
                report = new StringBuilder();
                report.AppendLine("Report");
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    MB_TexSet txs = distinctMaterialTextures[i];
                    report.AppendLine("----------");
                    report.Append("This set of textures will be resized to:" + txs.idealWidth + "x" + txs.idealHeight + "\n");
                    for (int j = 0; j < txs.ts.Length; j++)
                    {
                        if (!txs.ts[j].isNull)
                        {
                            report.Append("   [" + texPropertyNames[j].name + " " + txs.ts[j].GetTexName() + " " + txs.ts[j].width + "x" + txs.ts[j].height + "]");
                            if (txs.ts[j].matTilingRect.size != Vector2.one || txs.ts[j].matTilingRect.min != Vector2.zero) report.AppendFormat(" material scale {0} offset{1} ", txs.ts[j].matTilingRect.size.ToString("G4"), txs.ts[j].matTilingRect.min.ToString("G4"));
                            if (txs.obUVscale != Vector2.one || txs.obUVoffset != Vector2.zero) report.AppendFormat(" obUV scale {0} offset{1} ", txs.obUVscale.ToString("G4"), txs.obUVoffset.ToString("G4"));
                            report.AppendLine("");
                        }
                        else
                        {
                            report.Append("   [" + texPropertyNames[j].name + " null ");
                            if (allTexturesAreNullAndSameColor[j])
                            {
                                report.Append("no atlas will be created all textures null]\n");
                            }
                            else
                            {
                                report.AppendFormat("a 16x16 texture will be created]\n");
                            }
                        }
                    }
                    report.AppendLine("");
                    report.Append("Materials using:");
                    for (int j = 0; j < txs.matsAndGOs.mats.Count; j++)
                    {
                        report.Append(txs.matsAndGOs.mats[j].mat.name + ", ");
                    }
                    report.AppendLine("");
                }
            }


            //run the garbage collector to free up as much memory as possible before bake to reduce MissingReferenceException problems
            GC.Collect();
            Texture2D[] atlases = new Texture2D[numAtlases];
            Rect[] rectsInAtlas;
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("time Step 3 Create And Save Atlases part 1 " + sw.Elapsed.ToString());
            if (_packingAlgorithm == MB2_PackingAlgorithmEnum.UnitysPackTextures)
            {
                rectsInAtlas = __CreateAtlasesUnityTexturePacker(progressInfo, numAtlases, distinctMaterialTextures, texPropertyNames, allTexturesAreNullAndSameColor, resultMaterial, atlases, textureEditorMethods, _padding);
            }
            else if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker || _packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal || _packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical)
            {
                yield return __CreateAtlasesMBTexturePacker(progressInfo, numAtlases, distinctMaterialTextures, texPropertyNames, allTexturesAreNullAndSameColor, resultMaterial, atlases, textureEditorMethods, _padding);
                rectsInAtlas = __createAtlasesMBTexturePacker;
            }
            else
            {
                rectsInAtlas = __CreateAtlasesMBTexturePackerFast(progressInfo, numAtlases, distinctMaterialTextures, texPropertyNames, allTexturesAreNullAndSameColor, resultMaterial, atlases, textureEditorMethods, _padding);
            }
            float t3 = sw.ElapsedMilliseconds;

            AdjustNonTextureProperties(resultMaterial, texPropertyNames, distinctMaterialTextures, _considerNonTextureProperties, textureEditorMethods);

            if (progressInfo != null) progressInfo("Building Report", .7f);

            //report on atlases created
            StringBuilder atlasMessage = new StringBuilder();
            atlasMessage.AppendLine("---- Atlases ------");
            for (int i = 0; i < numAtlases; i++)
            {
                if (atlases[i] != null)
                {
                    atlasMessage.AppendLine("Created Atlas For: " + texPropertyNames[i].name + " h=" + atlases[i].height + " w=" + atlases[i].width);
                }
                else if (allTexturesAreNullAndSameColor[i])
                {
                    atlasMessage.AppendLine("Did not create atlas for " + texPropertyNames[i].name + " because all source textures were null.");
                }
            }
            report.Append(atlasMessage.ToString());

            List<MB_MaterialAndUVRect> mat2rect_map = new List<MB_MaterialAndUVRect>();
            for (int i = 0; i < distinctMaterialTextures.Count; i++)
            {
                List<MatAndTransformToMerged> mats = distinctMaterialTextures[i].matsAndGOs.mats;
                Rect fullSamplingRect = new Rect(0, 0, 1, 1);
                if (distinctMaterialTextures[i].ts.Length > 0)
                {
                    if (distinctMaterialTextures[i].allTexturesUseSameMatTiling)
                    {
                        fullSamplingRect = distinctMaterialTextures[i].ts[0].encapsulatingSamplingRect.GetRect();
                    }
                    else
                    {
                        fullSamplingRect = distinctMaterialTextures[i].obUVrect.GetRect();
                    }
                }
                for (int j = 0; j < mats.Count; j++)
                {
                    MB_MaterialAndUVRect key = new MB_MaterialAndUVRect(mats[j].mat, rectsInAtlas[i], mats[j].samplingRectMatAndUVTiling.GetRect(), mats[j].materialTiling.GetRect(), fullSamplingRect, mats[j].objName);
                    if (!mat2rect_map.Contains(key))
                    {
                        mat2rect_map.Add(key);
                    }
                }
            }

            resultAtlasesAndRects.atlases = atlases;                             // one per texture on result shader
            resultAtlasesAndRects.texPropertyNames = ShaderTextureProperty.GetNames(texPropertyNames); // one per texture on source shader
            resultAtlasesAndRects.mat2rect_map = mat2rect_map;

            if (progressInfo != null) progressInfo("Restoring Texture Formats & Read Flags", .8f);
            _destroyTemporaryTextures();
            if (textureEditorMethods != null) textureEditorMethods.RestoreReadFlagsAndFormats(progressInfo);
            if (report != null && LOG_LEVEL >= MB2_LogLevel.info) Debug.Log(report.ToString());
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Time Step 3 Create And Save Atlases part 3 " + (sw.ElapsedMilliseconds - t3).ToString("f5"));
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Total time Step 3 Create And Save Atlases " + sw.Elapsed.ToString());
            yield break;
        }

        AtlasPackingResult[] __RuntTexturePackerOnly(List<MB_TexSet> distinctMaterialTextures, int _padding)
        {
            AtlasPackingResult[] packerRects;
            if (distinctMaterialTextures.Count == 1 && _fixOutOfBoundsUVs == false)
            {

                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Only one image per atlas. Will re-use original texture");
                packerRects = new AtlasPackingResult[1];
                packerRects[0] = new AtlasPackingResult();
                packerRects[0].rects = new Rect[1];
                packerRects[0].srcImgIdxs = new int[] { 0 };
                packerRects[0].rects[0] = new Rect(0f, 0f, 1f, 1f);

                MeshBakerMaterialTexture dmt = null;
                if (distinctMaterialTextures[0].ts.Length > 0)
                {
                    dmt = distinctMaterialTextures[0].ts[0];

                }
                packerRects[0].atlasX = dmt.isNull ? 16 : dmt.width;
                packerRects[0].atlasY = dmt.isNull ? 16 : dmt.height;
                packerRects[0].usedW = dmt.isNull ? 16 : dmt.width;
                packerRects[0].usedH = dmt.isNull ? 16 : dmt.height;
            }
            else
            {
                List<Vector2> imageSizes = new List<Vector2>();
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    imageSizes.Add(new Vector2(distinctMaterialTextures[i].idealWidth, distinctMaterialTextures[i].idealHeight));
                }
                MB2_TexturePacker tp = CreateTexturePacker();
                tp.atlasMustBePowerOfTwo = _meshBakerTexturePackerForcePowerOfTwo;
                packerRects = tp.GetRects(imageSizes, _maxAtlasSize, _padding, true);
                //Debug.Assert(packerRects.Length != 0);
            }
            return packerRects;
        }

        MB2_TexturePacker CreateTexturePacker()
        {
            if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker)
            {
                return new MB2_TexturePackerRegular();
            }
            else if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast)
            {
                return new MB2_TexturePackerRegular();
            }
            else if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal)
            {
                MB2_TexturePackerHorizontalVert tp = new MB2_TexturePackerHorizontalVert();
                tp.packingOrientation = MB2_TexturePackerHorizontalVert.TexturePackingOrientation.horizontal;
                return tp;
            }
            else if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical)
            {
                MB2_TexturePackerHorizontalVert tp = new MB2_TexturePackerHorizontalVert();
                tp.packingOrientation = MB2_TexturePackerHorizontalVert.TexturePackingOrientation.vertical;
                return tp;
            }
            else
            {
                Debug.LogError("packing algorithm must be one of the MeshBaker options to create a Texture Packer");
            }
            return null;
        }

        Rect[] __createAtlasesMBTexturePacker;
        IEnumerator __CreateAtlasesMBTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases, List<MB_TexSet> distinctMaterialTextures, List<ShaderTextureProperty> texPropertyNames, bool[] allTexturesAreNullAndSameColor, Material resultMaterial, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, int _padding)
        {
            Rect[] uvRects;
            if (distinctMaterialTextures.Count == 1 && _fixOutOfBoundsUVs == false)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Only one image per atlas. Will re-use original texture");
                uvRects = new Rect[1];
                uvRects[0] = new Rect(0f, 0f, 1f, 1f);
                for (int i = 0; i < numAtlases; i++)
                {
                    MeshBakerMaterialTexture dmt = distinctMaterialTextures[0].ts[i];
                    atlases[i] = dmt.GetTexture2D();
                    resultMaterial.SetTexture(texPropertyNames[i].name, atlases[i]);
                    resultMaterial.SetTextureScale(texPropertyNames[i].name, dmt.matTilingRect.size);
                    resultMaterial.SetTextureOffset(texPropertyNames[i].name, dmt.matTilingRect.min);
                }
            }
            else
            {
                List<Vector2> imageSizes = new List<Vector2>();
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    imageSizes.Add(new Vector2(distinctMaterialTextures[i].idealWidth, distinctMaterialTextures[i].idealHeight));
                }
                MB2_TexturePacker tp = CreateTexturePacker();
                tp.atlasMustBePowerOfTwo = _meshBakerTexturePackerForcePowerOfTwo;
                int atlasSizeX = 1;
                int atlasSizeY = 1;

                int atlasMaxDimension = _maxAtlasSize;

                AtlasPackingResult[] packerRects = tp.GetRects(imageSizes, atlasMaxDimension, _padding);


                atlasSizeX = packerRects[0].atlasX;
                atlasSizeY = packerRects[0].atlasY;
                //Debug.Assert(packerRects.Length != 0);
                uvRects = packerRects[0].rects;
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Generated atlas will be " + atlasSizeX + "x" + atlasSizeY + " (Max atlas size for platform: " + atlasMaxDimension + ")");
                for (int propIdx = 0; propIdx < numAtlases; propIdx++)
                {
                    Texture2D atlas = null;
                    if (allTexturesAreNullAndSameColor[propIdx])
                    {
                        atlas = null;
                        if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("=== Not creating atlas for " + texPropertyNames[propIdx].name + " because textures are null and default value parameters are the same.");
                    }
                    else
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("=== Creating atlas for " + texPropertyNames[propIdx].name);
                        GC.Collect();

                        //use a jagged array because it is much more efficient in memory
                        Color[][] atlasPixels = new Color[atlasSizeY][];
                        for (int j = 0; j < atlasPixels.Length; j++)
                        {
                            atlasPixels[j] = new Color[atlasSizeX];
                        }
                        bool isNormalMap = false;
                        if (texPropertyNames[propIdx].isNormalMap) isNormalMap = true;

                        for (int texSetIdx = 0; texSetIdx < distinctMaterialTextures.Count; texSetIdx++)
                        {
                            string s = "Creating Atlas '" + texPropertyNames[propIdx].name + "' texture " + distinctMaterialTextures[texSetIdx];
                            if (progressInfo != null) progressInfo(s, .01f);
                            MB_TexSet texSet = distinctMaterialTextures[texSetIdx];
                            if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log(string.Format("Adding texture {0} to atlas {1}", texSet.ts[propIdx].GetTexture2D() == null ? "null" : texSet.ts[propIdx].GetTexName(), texPropertyNames[propIdx]));
                            Rect r = uvRects[texSetIdx];
                            Texture2D t = texSet.ts[propIdx].GetTexture2D();
                            int x = Mathf.RoundToInt(r.x * atlasSizeX);
                            int y = Mathf.RoundToInt(r.y * atlasSizeY);
                            int ww = Mathf.RoundToInt(r.width * atlasSizeX);
                            int hh = Mathf.RoundToInt(r.height * atlasSizeY);
                            if (ww == 0 || hh == 0) Debug.LogError("Image in atlas has no height or width");
                            if (progressInfo != null) progressInfo(s + " set ReadWrite flag", .01f);
                            if (textureEditorMethods != null) textureEditorMethods.SetReadWriteFlag(t, true, true);
                            if (progressInfo != null) progressInfo(s + "Copying to atlas: '" + texSet.ts[propIdx].GetTexName() + "'", .02f);
                            DRect samplingRect = texSet.ts[propIdx].encapsulatingSamplingRect;
                            yield return CopyScaledAndTiledToAtlas(texSet.ts[propIdx], texSet, texPropertyNames[propIdx], samplingRect, x, y, ww, hh, _fixOutOfBoundsUVs, _maxTilingBakeSize, atlasPixels, atlasSizeX, isNormalMap, progressInfo);
                            //							Debug.Log("after copyScaledAndTiledAtlas");
                        }
                        yield return numAtlases;
                        if (progressInfo != null) progressInfo("Applying changes to atlas: '" + texPropertyNames[propIdx].name + "'", .03f);
                        atlas = new Texture2D(atlasSizeX, atlasSizeY, TextureFormat.ARGB32, true);
                        for (int j = 0; j < atlasPixels.Length; j++)
                        {
                            atlas.SetPixels(0, j, atlasSizeX, 1, atlasPixels[j]);
                        }
                        atlas.Apply();
                        if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Saving atlas " + texPropertyNames[propIdx].name + " w=" + atlas.width + " h=" + atlas.height);
                    }
                    atlases[propIdx] = atlas;
                    if (progressInfo != null) progressInfo("Saving atlas: '" + texPropertyNames[propIdx].name + "'", .04f);
                    if (_saveAtlasesAsAssets && textureEditorMethods != null)
                    {
                        textureEditorMethods.SaveAtlasToAssetDatabase(atlases[propIdx], texPropertyNames[propIdx], propIdx, resultMaterial);
                    }
                    else
                    {
                        resultMaterial.SetTexture(texPropertyNames[propIdx].name, atlases[propIdx]);
                    }
                    resultMaterial.SetTextureOffset(texPropertyNames[propIdx].name, Vector2.zero);
                    resultMaterial.SetTextureScale(texPropertyNames[propIdx].name, Vector2.one);
                    _destroyTemporaryTextures(); // need to save atlases before doing this				
                }
            }
            __createAtlasesMBTexturePacker = uvRects;
            //			Debug.Log("finished!");
            yield break;
        }

        Rect[] __CreateAtlasesMBTexturePackerFast(ProgressUpdateDelegate progressInfo, int numAtlases, List<MB_TexSet> distinctMaterialTextures, List<ShaderTextureProperty> texPropertyNames, bool[] allTexturesAreNullAndSameColor, Material resultMaterial, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, int _padding)
        {
            Rect[] uvRects;
            if (distinctMaterialTextures.Count == 1 && _fixOutOfBoundsUVs == false)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Only one image per atlas. Will re-use original texture");
                uvRects = new Rect[1];
                uvRects[0] = new Rect(0f, 0f, 1f, 1f);
                for (int i = 0; i < numAtlases; i++)
                {
                    MeshBakerMaterialTexture dmt = distinctMaterialTextures[0].ts[i];
                    atlases[i] = dmt.GetTexture2D();
                    resultMaterial.SetTexture(texPropertyNames[i].name, atlases[i]);
                    resultMaterial.SetTextureScale(texPropertyNames[i].name, dmt.matTilingRect.size);
                    resultMaterial.SetTextureOffset(texPropertyNames[i].name, dmt.matTilingRect.min);
                }
            }
            else
            {
                List<Vector2> imageSizes = new List<Vector2>();
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    imageSizes.Add(new Vector2(distinctMaterialTextures[i].idealWidth, distinctMaterialTextures[i].idealHeight));
                }
                MB2_TexturePacker tp = CreateTexturePacker();
                tp.atlasMustBePowerOfTwo = _meshBakerTexturePackerForcePowerOfTwo;
                int atlasSizeX = 1;
                int atlasSizeY = 1;

                int atlasMaxDimension = _maxAtlasSize;

                AtlasPackingResult[] packerRects = tp.GetRects(imageSizes, atlasMaxDimension, _padding);
                atlasSizeX = packerRects[0].atlasX;
                atlasSizeY = packerRects[0].atlasY;
                //Debug.Assert(packerRects.Length == 1);
                uvRects = packerRects[0].rects;
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Generated atlas will be " + atlasSizeX + "x" + atlasSizeY + " (Max atlas size for platform: " + atlasMaxDimension + ")");

                //create a game object
                GameObject renderAtlasesGO = null;
                try
                {
                    renderAtlasesGO = new GameObject("MBrenderAtlasesGO");
                    MB3_AtlasPackerRenderTexture atlasRenderTexture = renderAtlasesGO.AddComponent<MB3_AtlasPackerRenderTexture>();
                    renderAtlasesGO.AddComponent<Camera>();
                    if (_considerNonTextureProperties)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn)
                        {
                            Debug.LogWarning("Blend Non-Texture Properties has limited functionality when used with Mesh Baker Texture Packer Fast.");
                        }
                    }
                    for (int i = 0; i < numAtlases; i++)
                    {
                        Texture2D atlas = null;
                        if (allTexturesAreNullAndSameColor[i])
                        {
                            atlas = null;
                            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Not creating atlas for " + texPropertyNames[i].name + " because textures are null and default value parameters are the same.");
                        }
                        else
                        {
                            GC.Collect();
                            if (progressInfo != null) progressInfo("Creating Atlas '" + texPropertyNames[i].name + "'", .01f);
                            // ===========
                            // configure it
                            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("About to render " + texPropertyNames[i].name + " isNormal=" + texPropertyNames[i].isNormalMap);
                            atlasRenderTexture.LOG_LEVEL = LOG_LEVEL;
                            atlasRenderTexture.width = atlasSizeX;
                            atlasRenderTexture.height = atlasSizeY;
                            atlasRenderTexture.padding = _padding;
                            atlasRenderTexture.rects = uvRects;
                            atlasRenderTexture.textureSets = distinctMaterialTextures;
                            atlasRenderTexture.indexOfTexSetToRender = i;
                            atlasRenderTexture.texPropertyName = texPropertyNames[i];
                            atlasRenderTexture.isNormalMap = texPropertyNames[i].isNormalMap;
                            atlasRenderTexture.fixOutOfBoundsUVs = _fixOutOfBoundsUVs;
                            atlasRenderTexture.considerNonTextureProperties = _considerNonTextureProperties;
                            atlasRenderTexture.resultMaterialTextureBlender = resultMaterialTextureBlender;
                            // call render on it
                            atlas = atlasRenderTexture.OnRenderAtlas(this);

                            // destroy it
                            // =============
                            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Saving atlas " + texPropertyNames[i].name + " w=" + atlas.width + " h=" + atlas.height + " id=" + atlas.GetInstanceID());
                        }
                        atlases[i] = atlas;
                        if (progressInfo != null) progressInfo("Saving atlas: '" + texPropertyNames[i].name + "'", .04f);
                        if (_saveAtlasesAsAssets && textureEditorMethods != null)
                        {
                            textureEditorMethods.SaveAtlasToAssetDatabase(atlases[i], texPropertyNames[i], i, resultMaterial);
                        }
                        else
                        {
                            resultMaterial.SetTexture(texPropertyNames[i].name, atlases[i]);
                        }
                        resultMaterial.SetTextureOffset(texPropertyNames[i].name, Vector2.zero);
                        resultMaterial.SetTextureScale(texPropertyNames[i].name, Vector2.one);
                        _destroyTemporaryTextures(); // need to save atlases before doing this				
                    }
                }
                catch (Exception ex)
                {
                    //Debug.LogError(ex);
                    Debug.LogException(ex);
                }
                finally
                {
                    if (renderAtlasesGO != null)
                    {
                        MB_Utility.Destroy(renderAtlasesGO);
                    }
                }
            }
            return uvRects;
        }


        Rect[] __CreateAtlasesUnityTexturePacker(ProgressUpdateDelegate progressInfo, int numAtlases, List<MB_TexSet> distinctMaterialTextures, List<ShaderTextureProperty> texPropertyNames, bool[] allTexturesAreNullAndSameColor, Material resultMaterial, Texture2D[] atlases, MB2_EditorMethodsInterface textureEditorMethods, int _padding)
        {
            Rect[] uvRects;
            if (distinctMaterialTextures.Count == 1 && _fixOutOfBoundsUVs == false)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Only one image per atlas. Will re-use original texture");
                uvRects = new Rect[1];
                uvRects[0] = new Rect(0f, 0f, 1f, 1f);
                for (int i = 0; i < numAtlases; i++)
                {
                    MeshBakerMaterialTexture dmt = distinctMaterialTextures[0].ts[i];
                    atlases[i] = dmt.GetTexture2D();
                    resultMaterial.SetTexture(texPropertyNames[i].name, atlases[i]);
                    resultMaterial.SetTextureScale(texPropertyNames[i].name, dmt.matTilingRect.size);
                    resultMaterial.SetTextureOffset(texPropertyNames[i].name, dmt.matTilingRect.min);
                }
            }
            else
            {
                long estArea = 0;
                int atlasSizeX = 1;
                int atlasSizeY = 1;
                uvRects = null;
                for (int i = 0; i < numAtlases; i++)
                { //i is an atlas "MainTex", "BumpMap" etc...
                  //-----------------------
                    Texture2D atlas = null;
                    if (allTexturesAreNullAndSameColor[i])
                    {
                        atlas = null;
                    }
                    else
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.LogWarning("Beginning loop " + i + " num temporary textures " + _temporaryTextures.Count);
                        for (int j = 0; j < distinctMaterialTextures.Count; j++)
                        { //j is a distinct set of textures one for each of "MainTex", "BumpMap" etc...
                            MB_TexSet txs = distinctMaterialTextures[j];

                            int tWidth = txs.idealWidth;
                            int tHeight = txs.idealHeight;

                            Texture2D tx = txs.ts[i].GetTexture2D();
                            if (tx == null)
                            {
                                tx = txs.ts[i].t = _createTemporaryTexture(tWidth, tHeight, TextureFormat.ARGB32, true);
                                if (_considerNonTextureProperties && resultMaterialTextureBlender != null)
                                {
                                    Color col = resultMaterialTextureBlender.GetColorIfNoTexture(txs.matsAndGOs.mats[0].mat, texPropertyNames[i]);
                                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("Setting texture to solid color " + col);
                                    MB_Utility.setSolidColor(tx, col);
                                }
                                else
                                {
                                    Color col = GetColorIfNoTexture(texPropertyNames[i]);
                                    MB_Utility.setSolidColor(tx, col);
                                }
                            }

                            if (progressInfo != null)
                            {
                                progressInfo("Adjusting for scale and offset " + tx, .01f);
                            }
                            if (textureEditorMethods != null)
                            {
                                textureEditorMethods.SetReadWriteFlag(tx, true, true);
                            }
                            tx = GetAdjustedForScaleAndOffset2(txs.ts[i], txs.obUVoffset, txs.obUVscale);

                            //create a resized copy if necessary
                            if (tx.width != tWidth || tx.height != tHeight)
                            {
                                if (progressInfo != null) progressInfo("Resizing texture '" + tx + "'", .01f);
                                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.LogWarning("Copying and resizing texture " + texPropertyNames[i].name + " from " + tx.width + "x" + tx.height + " to " + tWidth + "x" + tHeight);
                                tx = _resizeTexture((Texture2D)tx, tWidth, tHeight);
                            }

                            txs.ts[i].t = tx;
                        }

                        Texture2D[] texToPack = new Texture2D[distinctMaterialTextures.Count];
                        for (int j = 0; j < distinctMaterialTextures.Count; j++)
                        {
                            Texture2D tx = distinctMaterialTextures[j].ts[i].GetTexture2D();
                            estArea += tx.width * tx.height;
                            if (_considerNonTextureProperties)
                            {
                                tx = TintTextureWithTextureCombiner(tx, distinctMaterialTextures[j], texPropertyNames[i]);
                            }
                            texToPack[j] = tx;
                        }

                        if (textureEditorMethods != null) textureEditorMethods.CheckBuildSettings(estArea);

                        if (Math.Sqrt(estArea) > 3500f)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("The maximum possible atlas size is 4096. Textures may be shrunk");
                        }
                        atlas = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                        if (progressInfo != null)
                            progressInfo("Packing texture atlas " + texPropertyNames[i].name, .25f);
                        if (i == 0)
                        {
                            if (progressInfo != null)
                                progressInfo("Estimated min size of atlases: " + Math.Sqrt(estArea).ToString("F0"), .1f);
                            if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("Estimated atlas minimum size:" + Math.Sqrt(estArea).ToString("F0"));

                            _addWatermark(texToPack);

                            if (distinctMaterialTextures.Count == 1 && _fixOutOfBoundsUVs == false)
                            { //don't want to force power of 2 so tiling will still work
                                uvRects = new Rect[1] { new Rect(0f, 0f, 1f, 1f) };
                                atlas = _copyTexturesIntoAtlas(texToPack, _padding, uvRects, texToPack[0].width, texToPack[0].height);
                            }
                            else
                            {
                                int maxAtlasSize = 4096;
                                uvRects = atlas.PackTextures(texToPack, _padding, maxAtlasSize, false);
                            }

                            if (LOG_LEVEL >= MB2_LogLevel.info) Debug.Log("After pack textures atlas size " + atlas.width + " " + atlas.height);
                            atlasSizeX = atlas.width;
                            atlasSizeY = atlas.height;
                            atlas.Apply();
                        }
                        else
                        {
                            if (progressInfo != null)
                                progressInfo("Copying Textures Into: " + texPropertyNames[i].name, .1f);
                            atlas = _copyTexturesIntoAtlas(texToPack, _padding, uvRects, atlasSizeX, atlasSizeY);
                        }
                    }
                    atlases[i] = atlas;
                    //----------------------

                    if (_saveAtlasesAsAssets && textureEditorMethods != null)
                    {
                        textureEditorMethods.SaveAtlasToAssetDatabase(atlases[i], texPropertyNames[i], i, resultMaterial);
                    }
                    resultMaterial.SetTextureOffset(texPropertyNames[i].name, Vector2.zero);
                    resultMaterial.SetTextureScale(texPropertyNames[i].name, Vector2.one);

                    _destroyTemporaryTextures(); // need to save atlases before doing this
                    GC.Collect();
                }
            }
            return uvRects;
        }

        void _addWatermark(Texture2D[] texToPack)
        {
        }

        Texture2D _addWatermark(Texture2D texToPack)
        {
            return texToPack;
        }

        Texture2D _copyTexturesIntoAtlas(Texture2D[] texToPack, int padding, Rect[] rs, int w, int h)
        {
            Texture2D ta = new Texture2D(w, h, TextureFormat.ARGB32, true);
            MB_Utility.setSolidColor(ta, Color.clear);
            for (int i = 0; i < rs.Length; i++)
            {
                Rect r = rs[i];
                Texture2D t = texToPack[i];
                int x = Mathf.RoundToInt(r.x * w);
                int y = Mathf.RoundToInt(r.y * h);
                int ww = Mathf.RoundToInt(r.width * w);
                int hh = Mathf.RoundToInt(r.height * h);
                if (t.width != ww && t.height != hh)
                {
                    t = MB_Utility.resampleTexture(t, ww, hh);
                    _temporaryTextures.Add(t);
                }
                ta.SetPixels(x, y, ww, hh, t.GetPixels());
            }
            ta.Apply();
            return ta;
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        private static bool LOG_LEVEL_TRACE_MERGE_MAT_SUBRECTS = false;
        void MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects(List<MB_TexSet> distinctMaterialTextures, bool fixOutOfBoundsUVs)
        {
            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                Debug.Log("MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects");
            }
            int numMerged = 0;

            // IMPORTANT: Note that the verts stored in the mesh are NOT Normalized UV Coords. They are normalized * [UVTrans]. To get normalized UV
            // coords we must multiply them by [invUVTrans]. Need to do this to the verts in the mesh before we do any transforms with them.
            // Also check that all textures use same tiling. This is a prerequisite for merging.
            for (int i = 0; i < distinctMaterialTextures.Count; i++)
            {
                MB_TexSet tx = distinctMaterialTextures[i];
                int idxOfFirstNotNull = -1;
                bool allAreSame = true;
                DRect firstRect = new DRect();
                for (int propIdx = 0; propIdx < tx.ts.Length; propIdx++)
                {
                    if (idxOfFirstNotNull != -1)
                    {
                        if (!tx.ts[propIdx].isNull && firstRect != tx.ts[propIdx].matTilingRect)
                        {
                            allAreSame = false;
                        }
                    }
                    else if (!tx.ts[propIdx].isNull)
                    {
                        idxOfFirstNotNull = propIdx;
                        firstRect = tx.ts[propIdx].matTilingRect;
                    }
                }
                if (LOG_LEVEL_TRACE_MERGE_MAT_SUBRECTS == true)
                {
                    Debug.LogFormat("TextureSet {0} allTexturesUseSameMatTiling = {1}", i, allAreSame);
                }
                if (allAreSame)
                {
                    tx.allTexturesUseSameMatTiling = true;
                }
                else
                {
                    if (LOG_LEVEL <= MB2_LogLevel.info || LOG_LEVEL_TRACE_MERGE_MAT_SUBRECTS)
                    {
                        Debug.Log(string.Format("Textures in material(s) do not all use the same material tiling. This set of textures will not be considered for merge: {0} ", tx.GetDescription()));
                    }
                    tx.allTexturesUseSameMatTiling = false;
                }
            }

            // for each distinctMaterialTexture calculate the material tiling subrects atlasSubrectMaterialOnly
            // Full sample rect conains both material and UV tiling.
            // Where would normalized UV coords map to in fullSamplingRect if there was only material tiling (no mesh UV tiling).
            // Since none of the rectangles in the atlas have been merged yet and the fullSamplingRect includes (UV and mat) tiling, 
            // This is the inverse of the uv rect.
            // we need to keep track of this for adding an unknown mesh with this material. The new mesh might have different UV tiling
            // atlasSubrectMaterialOnly tells us how much of the tiling is due to the material vs. UV. 
            for (int i = 0; i < distinctMaterialTextures.Count; i++)
            {
                MB_TexSet tx = distinctMaterialTextures[i];

                DRect ruv;
                if (fixOutOfBoundsUVs)
                {
                    ruv = new DRect(tx.obUVoffset, tx.obUVscale);
                }
                else
                {
                    ruv = new DRect(0.0, 0.0, 1.0, 1.0);
                }
                for (int matIdx = 0; matIdx < tx.matsAndGOs.mats.Count; matIdx++)
                {
                    //tx.mats[matIdx].atlasSubrectMaterialOnly = atlasSubrectMaterialOnly;
                    tx.matsAndGOs.mats[matIdx].obUVRectIfTilingSame = ruv;
                    tx.matsAndGOs.mats[matIdx].objName = distinctMaterialTextures[i].matsAndGOs.gos[0].name;
                }

                tx.CalcInitialFullSamplingRects(fixOutOfBoundsUVs);

                tx.CalcMatAndUVSamplingRects();

            }

            // need to calculate the srcSampleRect for the complete tiling in the atlas
            // for each material need to know what the subrect would be in the atlas if material UVRect was 0,0,1,1 and Merged uvRect was full tiling
            List<int> MarkedForDeletion = new List<int>();
            for (int i = 0; i < distinctMaterialTextures.Count; i++)
            {
                MB_TexSet tx2 = distinctMaterialTextures[i];
                for (int j = i + 1; j < distinctMaterialTextures.Count; j++)
                {
                    MB_TexSet tx1 = distinctMaterialTextures[j];
                    if (tx1.AllTexturesAreSameForMerge(tx2, _considerNonTextureProperties, resultMaterialTextureBlender))
                    {
                        double accumulatedAreaCombined = 0f;
                        double accumulatedAreaNotCombined = 0f;
                        DRect encapsulatingRectMerged = new DRect();
                        int idxOfFirstNotNull = -1;
                        for (int propIdx = 0; propIdx < tx2.ts.Length; propIdx++)
                        {
                            if (!tx2.ts[propIdx].isNull)
                            {
                                if (idxOfFirstNotNull == -1) idxOfFirstNotNull = propIdx;
                            }
                        }

                        if (idxOfFirstNotNull != -1)
                        {
                            // only in here if all properties use the same tiling so don't need to worry about which propIdx we are dealing with
                            DRect encapsulatingRect1 = tx1.matsAndGOs.mats[0].samplingRectMatAndUVTiling;
                            for (int matIdx = 1; matIdx < tx1.matsAndGOs.mats.Count; matIdx++)
                            {
                                encapsulatingRect1 = MB3_UVTransformUtility.GetEncapsulatingRect(ref encapsulatingRect1, ref tx1.matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling);
                            }
                            DRect encapsulatingRect2 = tx2.matsAndGOs.mats[0].samplingRectMatAndUVTiling;
                            for (int matIdx = 1; matIdx < tx2.matsAndGOs.mats.Count; matIdx++)
                            {
                                encapsulatingRect2 = MB3_UVTransformUtility.GetEncapsulatingRect(ref encapsulatingRect2, ref tx2.matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling);
                            }

                            encapsulatingRectMerged = MB3_UVTransformUtility.GetEncapsulatingRect(ref encapsulatingRect1, ref encapsulatingRect2);
                            accumulatedAreaCombined += encapsulatingRectMerged.width * encapsulatingRectMerged.height;
                            accumulatedAreaNotCombined += encapsulatingRect1.width * encapsulatingRect1.height + encapsulatingRect2.width * encapsulatingRect2.height;
                        }
                        else
                        {
                            encapsulatingRectMerged = new DRect(0f, 0f, 1f, 1f);
                        }

                        //the distinct material textures may overlap.
                        //if the area of these rectangles combined is less than the sum of these areas of these rectangles then merge these distinctMaterialTextures
                        if (accumulatedAreaCombined < accumulatedAreaNotCombined)
                        {
                            // merge tx2 into tx1
                            numMerged++;
                            StringBuilder sb = null;
                            if (LOG_LEVEL >= MB2_LogLevel.info)
                            {
                                sb = new StringBuilder();
                                sb.AppendFormat("About To Merge:\n   TextureSet1 {0}\n   TextureSet2 {1}\n", tx1.GetDescription(), tx2.GetDescription());
                                if (LOG_LEVEL >= MB2_LogLevel.trace)
                                {
                                    for (int matIdx = 0; matIdx < tx1.matsAndGOs.mats.Count; matIdx++)
                                    {
                                        sb.AppendFormat("tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n",
                                            tx1.matsAndGOs.mats[matIdx].mat, tx1.matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling, tx1.ts[0].encapsulatingSamplingRect);
                                    }
                                    for (int matIdx = 0; matIdx < tx2.matsAndGOs.mats.Count; matIdx++)
                                    {
                                        sb.AppendFormat("tx2 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n",
                                            tx2.matsAndGOs.mats[matIdx].mat, tx2.matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling, tx2.ts[0].encapsulatingSamplingRect);
                                    }
                                }
                            }

                            //copy game objects over
                            for (int k = 0; k < tx2.matsAndGOs.gos.Count; k++)
                            {
                                if (!tx1.matsAndGOs.gos.Contains(tx2.matsAndGOs.gos[k]))
                                {
                                    tx1.matsAndGOs.gos.Add(tx2.matsAndGOs.gos[k]);
                                }
                            }
                            //need to calculate what the Merged UV rect would be
                            //need to know what subRect of that rect each source material would use.
                            //modify the trans in tx1 materials so it will adjust to combined rect
                            //don't want to replace it because it might have been merged earlier.
                            //[atlasSubrectMaterialOnly] = [materialTiling][MeshUVRect][invEncapsulatingRectMerged][invMeshUVRect]
                            /*
                            DRect invEncapsulatingRectMerged = MB3_UVTransformUtility.InverseTransform(ref encapsulatingRectMerged);
                            for (int matIdx = 0; matIdx < tx1.mats.Count; matIdx++)
                            {
                                DRect invfullSampleSubrect = MB3_UVTransformUtility.CombineTransforms(ref tx1.mats[matIdx].samplingRectMatAndUVTiling, ref invEncapsulatingRectMerged);
                                DRect invMeshUVRect = tx1.mats[matIdx].obUVRectIfTilingSame;
                                invMeshUVRect = MB3_UVTransformUtility.InverseTransform(ref invMeshUVRect);
                                tx1.mats[matIdx].atlasSubrectMaterialOnly = MB3_UVTransformUtility.CombineTransforms(ref invfullSampleSubrect, ref invMeshUVRect);
                            }
                            */

                            //copy materials over from tx2 to tx1
                            for (int matIdx = 0; matIdx < tx2.matsAndGOs.mats.Count; matIdx++)
                            {
                                //DRect invfullSampleSubrect = MB3_UVTransformUtility.CombineTransforms(ref tx2.mats[matIdx].samplingRectMatAndUVTiling, ref invEncapsulatingRectMerged);
                                //DRect invMeshUVRect = tx2.mats[matIdx].obUVRectIfTilingSame;
                                //invMeshUVRect = MB3_UVTransformUtility.InverseTransform(ref invMeshUVRect);
                                //tx2.mats[matIdx].atlasSubrectMaterialOnly = MB3_UVTransformUtility.CombineTransforms(ref invfullSampleSubrect, ref invMeshUVRect);
                                tx1.matsAndGOs.mats.Add(tx2.matsAndGOs.mats[matIdx]);
                            }

                            //sort the mats so that small ones enclosed by big ones come first. This is necessary
                            tx1.matsAndGOs.mats.Sort(new SamplingRectEnclosesComparer());

                            //the box for material tiling combined stored on a per material  basis
                            for (int propIdx = 0; propIdx < tx1.ts.Length; propIdx++)
                            {
                                tx1.ts[propIdx].encapsulatingSamplingRect = encapsulatingRectMerged;
                            }

                            if (!MarkedForDeletion.Contains(i)) MarkedForDeletion.Add(i);

                            if (LOG_LEVEL >= MB2_LogLevel.debug)
                            {
                                if (LOG_LEVEL >= MB2_LogLevel.trace)
                                {
                                    sb.AppendFormat("=== After Merge TextureSet {0}\n", tx1.GetDescription());
                                    for (int matIdx = 0; matIdx < tx1.matsAndGOs.mats.Count; matIdx++)
                                    {
                                        sb.AppendFormat("tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n",
                                            tx1.matsAndGOs.mats[matIdx].mat, tx1.matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling, tx1.ts[0].encapsulatingSamplingRect);
                                    }
                                    //Integrity check that sampling rects fit into enapsulating rects
                                    if (MB3_MeshBakerRoot.DO_INTEGRITY_CHECKS)
                                    {
                                        if (MB3_MeshBakerRoot.DO_INTEGRITY_CHECKS) { DoIntegrityCheckMergedAtlasRects(distinctMaterialTextures); }
                                    }
                                }
                                Debug.Log(sb.ToString());
                            }
                            break;
                        }
                        else
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.debug)
                            {
                                Debug.Log(string.Format("Considered merging {0} and {1} but there was not enough overlap. It is more efficient to bake these to separate rectangles.", tx1.GetDescription(), tx2.GetDescription()));
                            }
                        }
                    }
                }
            }
            //remove distinctMaterialTextures that were merged
            for (int j = MarkedForDeletion.Count - 1; j >= 0; j--)
            {
                distinctMaterialTextures.RemoveAt(MarkedForDeletion[j]);
            }
            MarkedForDeletion.Clear();
            if (LOG_LEVEL >= MB2_LogLevel.info)
            {
                Debug.Log(String.Format("MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects complete merged {0}", numMerged));
            }
            if (MB3_MeshBakerRoot.DO_INTEGRITY_CHECKS) { DoIntegrityCheckMergedAtlasRects(distinctMaterialTextures); }
        }

        void DoIntegrityCheckMergedAtlasRects(List<MB_TexSet> distinctMaterialTextures)
        {
            if (MB3_MeshBakerRoot.DO_INTEGRITY_CHECKS)
            {
                for (int i = 0; i < distinctMaterialTextures.Count; i++)
                {
                    MB_TexSet tx1 = distinctMaterialTextures[i];

                    if (!tx1.allTexturesUseSameMatTiling)
                    {
                        continue;
                    }
                    //Integrity check that sampling rects fit into enapsulating rects
                    //DRect encapsulatingRectCannonical = tx1.ts[0].encapsulatingSamplingRect;
                    //MB3_UVTransformUtility.Canonicalize(ref encapsulatingRectCannonical, 0, 0);
                    for (int matIdx = 0; matIdx < tx1.matsAndGOs.mats.Count; matIdx++)
                    {
                        MatAndTransformToMerged mat = tx1.matsAndGOs.mats[matIdx];
                        DRect uvR = mat.obUVRectIfTilingSame;
                        DRect matR = mat.materialTiling;
                        if (!MB2_TextureBakeResults.IsMeshAndMaterialRectEnclosedByAtlasRect(uvR.GetRect(), matR.GetRect(), tx1.ts[0].encapsulatingSamplingRect.GetRect(), LOG_LEVEL))
                        {
                            Debug.LogErrorFormat("mesh " + tx1.matsAndGOs.mats[matIdx].objName + "\n" +
                                                " uv=" + uvR + "\n" +
                                                " mat=" + matR.GetRect().ToString("f5") + "\n" +
                                                " samplingRect=" + tx1.matsAndGOs.mats[matIdx].samplingRectMatAndUVTiling.GetRect().ToString("f4") + "\n" +
                                                " encapsulatingRect " + tx1.ts[0].encapsulatingSamplingRect.GetRect().ToString("f4") + "\n");
                            //" encapsulatingRectCannonical=" + rr.ToString("f4") + "\n\n");
                            Debug.LogErrorFormat(String.Format("Integrity check failed. " + tx1.matsAndGOs.mats[matIdx].objName + " Encapsulating sampling rect failed to contain potentialRect\n"));

                        }
                    }
                }
            }
        }

        Vector2 GetAdjustedForScaleAndOffset2Dimensions(MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale)
        {
            if (source.matTilingRect.x == 0f && source.matTilingRect.y == 0f && source.matTilingRect.width == 1f && source.matTilingRect.height == 1f)
            {
                if (_fixOutOfBoundsUVs)
                {
                    if (obUVoffset.x == 0f && obUVoffset.y == 0f && obUVscale.x == 1f && obUVscale.y == 1f)
                    {
                        return new Vector2(source.width, source.height); //no adjustment necessary
                    }
                }
                else
                {
                    return new Vector2(source.width, source.height); //no adjustment necessary
                }
            }

            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("GetAdjustedForScaleAndOffset2Dimensions: " + source.GetTexName() + " " + obUVoffset + " " + obUVscale);
            float newWidth = (float)source.encapsulatingSamplingRect.width * source.width;
            float newHeight = (float)source.encapsulatingSamplingRect.height * source.height;

            if (newWidth > _maxTilingBakeSize) newWidth = _maxTilingBakeSize;
            if (newHeight > _maxTilingBakeSize) newHeight = _maxTilingBakeSize;
            if (newWidth < 1f) newWidth = 1f;
            if (newHeight < 1f) newHeight = 1f;
            return new Vector2(newWidth, newHeight);
        }

        // used by Unity texture packer to handle tiled textures.
        // may create a new texture that has the correct tiling to handle fix out of bounds UVs
        public Texture2D GetAdjustedForScaleAndOffset2(MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale)
        {
            Texture2D sourceTex = source.GetTexture2D();
            if (source.matTilingRect.x == 0f && source.matTilingRect.y == 0f && source.matTilingRect.width == 1f && source.matTilingRect.height == 1f)
            {
                if (_fixOutOfBoundsUVs)
                {
                    if (obUVoffset.x == 0f && obUVoffset.y == 0f && obUVscale.x == 1f && obUVscale.y == 1f)
                    {
                        return sourceTex; //no adjustment necessary
                    }
                }
                else
                {
                    return sourceTex; //no adjustment necessary
                }
            }
            Vector2 dim = GetAdjustedForScaleAndOffset2Dimensions(source, obUVoffset, obUVscale);

            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.LogWarning("GetAdjustedForScaleAndOffset2: " + sourceTex + " " + obUVoffset + " " + obUVscale);
            float newWidth = dim.x;
            float newHeight = dim.y;
            float scx = (float)source.matTilingRect.width;
            float scy = (float)source.matTilingRect.height;
            float ox = (float)source.matTilingRect.x;
            float oy = (float)source.matTilingRect.y;
            if (_fixOutOfBoundsUVs)
            {
                scx *= obUVscale.x;
                scy *= obUVscale.y;
                ox = (float)(source.matTilingRect.x * obUVscale.x + obUVoffset.x);
                oy = (float)(source.matTilingRect.y * obUVscale.y + obUVoffset.y);
            }
            Texture2D newTex = _createTemporaryTexture((int)newWidth, (int)newHeight, TextureFormat.ARGB32, true);
            for (int i = 0; i < newTex.width; i++)
            {
                for (int j = 0; j < newTex.height; j++)
                {
                    float u = i / newWidth * scx + ox;
                    float v = j / newHeight * scy + oy;
                    newTex.SetPixel(i, j, sourceTex.GetPixelBilinear(u, v));
                }
            }
            newTex.Apply();
            return newTex;
        }

        internal static DRect GetSourceSamplingRect(MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale)
        {
            DRect rMatTiling = source.matTilingRect;
            DRect rUVTiling = new DRect(obUVoffset, obUVscale);
            DRect r = MB3_UVTransformUtility.CombineTransforms(ref rMatTiling, ref rUVTiling);
            return r;
        }

        Texture2D TintTextureWithTextureCombiner(Texture2D t, MB_TexSet sourceMaterial, ShaderTextureProperty shaderPropertyName)
        {
            if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log(string.Format("Blending texture {0} mat {1} with non-texture properties using TextureBlender {2}", t.name, sourceMaterial.matsAndGOs.mats[0].mat, resultMaterialTextureBlender));

            resultMaterialTextureBlender.OnBeforeTintTexture(sourceMaterial.matsAndGOs.mats[0].mat, shaderPropertyName.name);
            //combine the tintColor with the texture
            t = _createTextureCopy(t);
            for (int i = 0; i < t.height; i++)
            {
                Color[] cs = t.GetPixels(0, i, t.width, 1);
                for (int j = 0; j < cs.Length; j++)
                {
                    cs[j] = resultMaterialTextureBlender.OnBlendTexturePixel(shaderPropertyName.name, cs[j]);
                }
                t.SetPixels(0, i, t.width, 1, cs);
            }
            t.Apply();
            return t;
        }

        //private bool HasFinished;
        public IEnumerator CopyScaledAndTiledToAtlas(MeshBakerMaterialTexture source, MB_TexSet sourceMaterial, ShaderTextureProperty shaderPropertyName, DRect srcSamplingRect, int targX, int targY, int targW, int targH, bool _fixOutOfBoundsUVs, int maxSize, Color[][] atlasPixels, int atlasWidth, bool isNormalMap, ProgressUpdateDelegate progressInfo = null)
        {
            //HasFinished = false;
            Texture2D t = source.GetTexture2D();
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("CopyScaledAndTiledToAtlas: " + t + " inAtlasX=" + targX + " inAtlasY=" + targY + " inAtlasW=" + targW + " inAtlasH=" + targH);
            float newWidth = targW;
            float newHeight = targH;
            float scx = (float)srcSamplingRect.width;
            float scy = (float)srcSamplingRect.height;
            float ox = (float)srcSamplingRect.x;
            float oy = (float)srcSamplingRect.y;
            int w = (int)newWidth;
            int h = (int)newHeight;

            if (t == null)
            {
                if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("No source texture creating a 16x16 texture.");
                t = _createTemporaryTexture(16, 16, TextureFormat.ARGB32, true);
                scx = 1;
                scy = 1;
                if (_considerNonTextureProperties && resultMaterialTextureBlender != null)
                {
                    Color col = resultMaterialTextureBlender.GetColorIfNoTexture(sourceMaterial.matsAndGOs.mats[0].mat, shaderPropertyName);
                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("Setting texture to solid color " + col);
                    MB_Utility.setSolidColor(t, col);
                }
                else
                {
                    Color col = GetColorIfNoTexture(shaderPropertyName);
                    MB_Utility.setSolidColor(t, col);
                }
            }
            if (_considerNonTextureProperties && resultMaterialTextureBlender != null)
            {
                t = TintTextureWithTextureCombiner(t, sourceMaterial, shaderPropertyName);
            }
            t = _addWatermark(t);
            for (int i = 0; i < w; i++)
            {

                if (progressInfo != null && w > 0) progressInfo("CopyScaledAndTiledToAtlas " + (((float)i / (float)w) * 100f).ToString("F0"), .2f);
                for (int j = 0; j < h; j++)
                {
                    float u = i / newWidth * scx + ox;
                    float v = j / newHeight * scy + oy;
                    atlasPixels[targY + j][targX + i] = t.GetPixelBilinear(u, v);
                }
            }
            int atlasPaddingH = _atlasPadding;
            int atlasPaddingW = _atlasPadding;
            if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal) atlasPaddingW = 0;
            if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical) atlasPaddingH = 0;
            //bleed the border colors into the padding
            for (int i = 0; i < w; i++)
            {
                for (int j = 1; j <= atlasPaddingH; j++)
                {
                    //top margin
                    atlasPixels[(targY - j)][targX + i] = atlasPixels[(targY)][targX + i];
                    //bottom margin
                    atlasPixels[(targY + h - 1 + j)][targX + i] = atlasPixels[(targY + h - 1)][targX + i];
                }
            }
            for (int j = 0; j < h; j++)
            {
                for (int i = 1; i <= atlasPaddingW; i++)
                {
                    //left margin
                    atlasPixels[(targY + j)][targX - i] = atlasPixels[(targY + j)][targX];
                    //right margin
                    atlasPixels[(targY + j)][targX + w + i - 1] = atlasPixels[(targY + j)][targX + w - 1];
                }
            }
            //corners
            for (int i = 1; i <= atlasPaddingW; i++)
            {
                for (int j = 1; j <= atlasPaddingH; j++)
                {
                    atlasPixels[(targY - j)][targX - i] = atlasPixels[targY][targX];
                    atlasPixels[(targY + h - 1 + j)][targX - i] = atlasPixels[(targY + h - 1)][targX];
                    atlasPixels[(targY + h - 1 + j)][targX + w + i - 1] = atlasPixels[(targY + h - 1)][targX + w - 1];
                    atlasPixels[(targY - j)][targX + w + i - 1] = atlasPixels[targY][targX + w - 1];
                    yield return null;
                }
                yield return null;
            }
            //			Debug.Log("copyandscaledatlas finished too!");
            //HasFinished = true;
            yield break;
        }

        //used to track temporary textures that were created so they can be destroyed
        public Texture2D _createTemporaryTexture(int w, int h, TextureFormat texFormat, bool mipMaps)
        {
            Texture2D t = new Texture2D(w, h, texFormat, mipMaps);
            MB_Utility.setSolidColor(t, Color.clear);
            _temporaryTextures.Add(t);
            return t;
        }

        internal Texture2D _createTextureCopy(Texture2D t)
        {
            Texture2D tx = MB_Utility.createTextureCopy(t);
            _temporaryTextures.Add(tx);
            return tx;
        }

        Texture2D _resizeTexture(Texture2D t, int w, int h)
        {
            Texture2D tx = MB_Utility.resampleTexture(t, w, h);
            _temporaryTextures.Add(tx);
            return tx;
        }

        void _destroyTemporaryTextures()
        {
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Destroying " + _temporaryTextures.Count + " temporary textures");
            for (int i = 0; i < _temporaryTextures.Count; i++)
            {
                MB_Utility.Destroy(_temporaryTextures[i]);
            }
            _temporaryTextures.Clear();
        }

        public void _addProceduralMaterial(ProceduralMaterial pm)
        {
            ProceduralMaterialInfo pmi = new ProceduralMaterialInfo();
            pm.isReadable = pm.isReadable;
            pmi.proceduralMat = pm;
            _proceduralMaterials.Add(pmi);
        }

        public void _restoreProceduralMaterials()
        {
            for (int i = 0; i < _proceduralMaterials.Count; i++)
            {
                ProceduralMaterialInfo pmi = _proceduralMaterials[i];
                pmi.proceduralMat.isReadable = pmi.originalIsReadableVal;
                pmi.proceduralMat.RebuildTexturesImmediately();
            }
            _proceduralMaterials.Clear();
        }

        public void SuggestTreatment(List<GameObject> objsToMesh, Material[] resultMaterials, List<ShaderTextureProperty> _customShaderPropNames)
        {
            this._customShaderPropNames = _customShaderPropNames;
            StringBuilder sb = new StringBuilder();
            Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>(); //cache results
            for (int i = 0; i < objsToMesh.Count; i++)
            {
                GameObject obj = objsToMesh[i];
                if (obj == null) continue;
                Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
                if (ms.Length > 1)
                { // and each material is not mapped to its own layer
                    sb.AppendFormat("\nObject {0} uses {1} materials. Possible treatments:\n", objsToMesh[i].name, ms.Length);
                    sb.AppendFormat("  1) Collapse the submeshes together into one submesh in the combined mesh. Each of the original submesh materials will map to a different UV rectangle in the atlas(es) used by the combined material.\n");
                    sb.AppendFormat("  2) Use the multiple materials feature to map submeshes in the source mesh to submeshes in the combined mesh.\n");
                }
                Mesh m = MB_Utility.GetMesh(obj);

                MB_Utility.MeshAnalysisResult[] mar;
                if (!meshAnalysisResultsCache.TryGetValue(m.GetInstanceID(), out mar))
                {
                    mar = new MB_Utility.MeshAnalysisResult[m.subMeshCount];
                    MB_Utility.doSubmeshesShareVertsOrTris(m, ref mar[0]);
                    for (int j = 0; j < m.subMeshCount; j++)
                    {
                        MB_Utility.hasOutOfBoundsUVs(m, ref mar[j], j);
                        //DRect outOfBoundsUVRect = new DRect(mar[j].uvRect);
                        mar[j].hasOverlappingSubmeshTris = mar[0].hasOverlappingSubmeshTris;
                        mar[j].hasOverlappingSubmeshVerts = mar[0].hasOverlappingSubmeshVerts;
                    }
                    meshAnalysisResultsCache.Add(m.GetInstanceID(), mar);
                }

                for (int j = 0; j < ms.Length; j++)
                {
                    if (mar[j].hasOutOfBoundsUVs)
                    {
                        DRect r = new DRect(mar[j].uvRect);
                        sb.AppendFormat("\nObject {0} submesh={1} material={2} uses UVs outside the range 0,0 .. 1,1 to create tiling that tiles the box {3},{4} .. {5},{6}. This is a problem because the UVs outside the 0,0 .. 1,1 " +
                                        "rectangle will pick up neighboring textures in the atlas. Possible Treatments:\n", obj, j, ms[j], r.x.ToString("G4"), r.y.ToString("G4"), (r.x + r.width).ToString("G4"), (r.y + r.height).ToString("G4"));
                        sb.AppendFormat("    1) Ignore the problem. The tiling may not affect result significantly.\n");
                        sb.AppendFormat("    2) Use the 'fix out of bounds UVs' feature to bake the tiling and scale the UVs to fit in the 0,0 .. 1,1 rectangle.\n");
                        sb.AppendFormat("    3) Use the Multiple Materials feature to map the material on this submesh to its own submesh in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
                        sb.AppendFormat("    4) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
                    }
                }
                if (mar[0].hasOverlappingSubmeshVerts)
                {
                    sb.AppendFormat("\nObject {0} has submeshes that share vertices. This is a problem because each vertex can have only one UV coordinate and may be required to map to different positions in the various atlases that are generated. Possible treatments:\n", objsToMesh[i]);
                    sb.AppendFormat(" 1) Ignore the problem. The vertices may not affect the result.\n");
                    sb.AppendFormat(" 2) Use the Multiple Materials feature to map the submeshs that overlap to their own submeshs in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
                    sb.AppendFormat(" 3) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
                }
            }
            Dictionary<Material, List<GameObject>> m2gos = new Dictionary<Material, List<GameObject>>();
            for (int i = 0; i < objsToMesh.Count; i++)
            {
                if (objsToMesh[i] != null)
                {
                    Material[] ms = MB_Utility.GetGOMaterials(objsToMesh[i]);
                    for (int j = 0; j < ms.Length; j++)
                    {
                        if (ms[j] != null)
                        {
                            List<GameObject> lgo;
                            if (!m2gos.TryGetValue(ms[j], out lgo))
                            {
                                lgo = new List<GameObject>();
                                m2gos.Add(ms[j], lgo);
                            }
                            if (!lgo.Contains(objsToMesh[i])) lgo.Add(objsToMesh[i]);
                        }
                    }
                }
            }

            List<ShaderTextureProperty> texPropertyNames = new List<ShaderTextureProperty>();
            for (int i = 0; i < resultMaterials.Length; i++)
            {
                _CollectPropertyNames(resultMaterials[i], texPropertyNames);
                foreach (Material m in m2gos.Keys)
                {
                    for (int j = 0; j < texPropertyNames.Count; j++)
                    {
                        //						Texture2D tx = null;
                        //						Vector2 scale = Vector2.one;
                        //						Vector2 offset = Vector2.zero;
                        //						Vector2 obUVscale = Vector2.one;
                        //						Vector2 obUVoffset = Vector2.zero; 
                        if (m.HasProperty(texPropertyNames[j].name))
                        {
                            Texture txx = m.GetTexture(texPropertyNames[j].name);
                            if (txx != null)
                            {
                                Vector2 o = m.GetTextureOffset(texPropertyNames[j].name);
                                Vector3 s = m.GetTextureScale(texPropertyNames[j].name);
                                if (o.x < 0f || o.x + s.x > 1f ||
                                    o.y < 0f || o.y + s.y > 1f)
                                {
                                    sb.AppendFormat("\nMaterial {0} used by objects {1} uses texture {2} that is tiled (scale={3} offset={4}). If there is more than one texture in the atlas " +
                                                        " then Mesh Baker will bake the tiling into the atlas. If the baked tiling is large then quality can be lost. Possible treatments:\n", m, PrintList(m2gos[m]), txx, s, o);
                                    sb.AppendFormat("  1) Use the baked tiling.\n");
                                    sb.AppendFormat("  2) Use the Multiple Materials feature to map the material on this object/submesh to its own submesh in the combined mesh. No other materials should map to this submesh. The original material can be applied to this submesh.\n");
                                    sb.AppendFormat("  3) Combine only meshes that use the same (or subset of) the set of textures on this mesh. The original material can be applied to the result.\n");
                                }
                            }
                        }
                    }
                }
            }
            string outstr = "";
            if (sb.Length == 0)
            {
                outstr = "====== No problems detected. These meshes should combine well ====\n  If there are problems with the combined meshes please report the problem to digitalOpus.ca so we can improve Mesh Baker.";
            }
            else
            {
                outstr = "====== There are possible problems with these meshes that may prevent them from combining well. TREATMENT SUGGESTIONS (copy and paste to text editor if too big) =====\n" + sb.ToString();
            }
            Debug.Log(outstr);
        }

        TextureBlender FindMatchingTextureBlender(string shaderName)
        {
            for (int i = 0; i < textureBlenders.Length; i++)
            {
                if (textureBlenders[i].DoesShaderNameMatch(shaderName))
                {
                    return textureBlenders[i];
                }
            }
            return null;
        }

        //If we are switching from a Material that uses color properties to
        //using atlases don't want some properties such as _Color to be copied
        //from the original material because the atlas texture will be multiplied
        //by that color
        void AdjustNonTextureProperties(Material mat, List<ShaderTextureProperty> texPropertyNames, List<MB_TexSet> distinctMaterialTextures, bool considerTintColor, MB2_EditorMethodsInterface editorMethods)
        {
            if (mat == null || texPropertyNames == null) return;
            if (_considerNonTextureProperties)
            {
                //try to use a texture blender if we can find one to set the non-texture property values
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Adjusting non texture properties using TextureBlender for shader: " + mat.shader.name);
                resultMaterialTextureBlender.SetNonTexturePropertyValuesOnResultMaterial(mat);
                return;
            }
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Adjusting non texture properties on result material");
            for (int i = 0; i < texPropertyNames.Count; i++)
            {
                string nm = texPropertyNames[i].name;
                if (nm.Equals("_MainTex"))
                {
                    if (mat.HasProperty("_Color"))
                    {
                        try
                        {
                            if (considerTintColor)
                            {
                                //tint color was baked into atlas so set to white;
                                mat.SetColor("_Color", Color.white);
                            }
                            else
                            {
                                //mat.SetColor("_Color", distinctMaterialTextures[0].ts[i].tintColor);
                            }
                        }
                        catch (Exception) { }
                    }
                }
                if (nm.Equals("_BumpMap"))
                {
                    if (mat.HasProperty("_BumpScale"))
                    {
                        try
                        {
                            mat.SetFloat("_BumpScale", 1f);
                        }
                        catch (Exception) { }
                    }
                }
                if (nm.Equals("_ParallaxMap"))
                {
                    if (mat.HasProperty("_Parallax"))
                    {
                        try
                        {
                            mat.SetFloat("_Parallax", .02f);
                        }
                        catch (Exception) { }
                    }
                }
                if (nm.Equals("_OcclusionMap"))
                {
                    if (mat.HasProperty("_OcclusionStrength"))
                    {
                        try
                        {
                            mat.SetFloat("_OcclusionStrength", 1f);
                        }
                        catch (Exception) { }
                    }
                }
                if (nm.Equals("_EmissionMap"))
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        try
                        {
                            mat.SetColor("_EmissionColor", new Color(0f, 0f, 0f, 0f));
                        }
                        catch (Exception) { }
                    }
                    if (mat.HasProperty("_EmissionScaleUI"))
                    {
                        try
                        {
                            mat.SetFloat("_EmissionScaleUI", 1f);
                        }
                        catch (Exception) { }
                    }
                }
            }
            if (editorMethods != null)
            {
                editorMethods.CommitChangesToAssets();
            }
        }

        public static Color GetColorIfNoTexture(ShaderTextureProperty texProperty)
        {
            if (texProperty.isNormalMap)
            {
                return new Color(.5f, .5f, 1f);
            }
            else if (texProperty.name.Equals("_MetallicGlossMap"))
            {
                return new Color(0f, 0f, 0f, 1f);
            }
            else if (texProperty.name.Equals("_ParallaxMap"))
            {
                return new Color(0f, 0f, 0f, 0f);
            }
            else if (texProperty.name.Equals("_OcclusionMap"))
            {
                return new Color(1f, 1f, 1f, 1f);
            }
            else if (texProperty.name.Equals("_EmissionMap"))
            {
                return new Color(0f, 0f, 0f, 0f);
            }
            else if (texProperty.name.Equals("_DetailMask"))
            {
                return new Color(0f, 0f, 0f, 0f);
            }
            return new Color(1f, 1f, 1f, 0f);
        }

        /* 
        Unity uses a non-standard format for storing normals for some platforms. Imagine the standard format is English, Unity's is French
        When the normal-map checkbox is ticked on the asset importer the normal map is translated into french. When we build the normal atlas
        we are reading the french. When we save and click the normal map tickbox we are translating french -> french. A double transladion that
        breaks the normal map. To fix this we need to "unconvert" the normal map to english when saving the atlas as a texture so that unity importer
        can do its thing properly. 
        */
        Color32 ConvertNormalFormatFromUnity_ToStandard(Color32 c)
        {
            Vector3 n = Vector3.zero;
            n.x = c.a * 2f - 1f;
            n.y = c.g * 2f - 1f;
            n.z = Mathf.Sqrt(1 - n.x * n.x - n.y * n.y);
            //now repack in the regular format
            Color32 cc = new Color32();
            cc.a = 1;
            cc.r = (byte)((n.x + 1f) * .5f);
            cc.g = (byte)((n.y + 1f) * .5f);
            cc.b = (byte)((n.z + 1f) * .5f);
            return cc;
        }

        float GetSubmeshArea(Mesh m, int submeshIdx)
        {
            if (submeshIdx >= m.subMeshCount || submeshIdx < 0)
            {
                return 0f;
            }
            Vector3[] vs = m.vertices;
            int[] tris = m.GetIndices(submeshIdx);
            float area = 0f;
            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v0 = vs[tris[i]];
                Vector3 v1 = vs[tris[i + 1]];
                Vector3 v2 = vs[tris[i + 2]];
                Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0);
                area += cross.magnitude / 2f;
            }
            return area;
        }

        string PrintList(List<GameObject> gos)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < gos.Count; i++)
            {
                sb.Append(gos[i] + ",");
            }
            return sb.ToString();
        }

    }
}
