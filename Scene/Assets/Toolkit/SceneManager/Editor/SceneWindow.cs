using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using lhFramework.Infrastructure.Managers;
using System.IO;

namespace lhFramework.Tools.Scene
{
    using Infrastructure.Core;
    public class SceneWindow : EditorWindow
    {
        private static SceneWindow window;
        private SceneConfig m_config;
        private string m_rootName="[Root]";
        private string m_lightName="[Light]";
        private string m_packageFolder="Assets/Package/";
        private string m_bundlePath="Assets/StreamingAssets/";
        [MenuItem("Tools/SceneManager/Window")]
        static void Init()
        {
            window = (SceneWindow)EditorWindow.GetWindow<SceneWindow>(false, "SceneWindow");
            window.Initialize();
            window.Show();
        }
        [MenuItem("Tools/SceneManager/Generate Current")]
        static void GenerateCurrent()
        {
            window.ToGenerate();
            EditorUtility.DisplayDialog("导出成功", "场景物件配置导出完成！", "关闭");
            AssetDatabase.Refresh();
        }
        [MenuItem("Tools/SceneManager/Build Clear")]
        static void ClearBuildName()
        {
            string[] str = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < str.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(str[i], true);
            }
        }
        [MenuItem("Tools/SceneManager/Generate Selected")]
        static void GenerateSelected()
        {
            var objs = Selection.objects;
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] is SceneAsset)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Art/Battle/Unitys/" + objs[i].name + ".unity");
                    window.ToGenerate();
                }
            }
            EditorUtility.DisplayDialog("导出成功", "场景物件配置导出完成！", "关闭");
            AssetDatabase.Refresh();
        }
        void Initialize()
        {

        }
        void OnGUI()
        {
            m_rootName = EditorGUILayout.TextField(m_rootName);
            m_lightName = EditorGUILayout.TextField(m_lightName);
            m_packageFolder = EditorGUILayout.TextField(m_packageFolder);
            if (GUILayout.Button("Generate Current Scene"))
            {
                GenerateCurrent();
            }
            if (GUILayout.Button("Generate Selected Scene"))
            {
                GenerateSelected();
            }
        }
        private void ToGenerateVariant(GameObject root,EVariantType variantType)
        {
            m_config = new SceneConfig();
            //var root = GameObject.Find(m_rootName);
            string folderName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (System.IO.Directory.Exists(m_packageFolder + folderName + "/" + variantType.ToString()))
            {
                DelectDir(m_packageFolder + folderName + "/" + variantType.ToString());
            }
            Directory.CreateDirectory(m_packageFolder + folderName + "/" + variantType.ToString());
            MeshRenderer[] meshRenders = root.GetComponentsInChildren<MeshRenderer>();
            List<MeshRenderer> renlist = new List<MeshRenderer>();
            List<MeshFilter> filList = new List<MeshFilter>();
            for (int i = 0; i < meshRenders.Length; i++)
            {
                var meshRender = meshRenders[i];
                var meshFilter = meshRender.GetComponent<MeshFilter>();
                if (meshFilter == null) continue;
                if (meshRender.sharedMaterials.Length <= 0 || meshRender.sharedMaterial == null) continue;
                bool has = false;
                for (int j = 0; j < renlist.Count; j++)
                {
                    if (filList[j].mesh == meshFilter.sharedMesh && renlist[j].sharedMaterials.Length == meshRender.sharedMaterials.Length && renlist[i].sharedMaterial == meshRender.sharedMaterial)
                    {
                        has = true;
                        break;
                    }
                }
                if (has)
                {
                    continue;
                }
                var f = meshRender.GetComponent<MeshFilter>();
                if (f == null) continue;
                renlist.Add(meshRender);
                filList.Add(f); ;
                var obj = GameObject.Instantiate(meshRender.gameObject);
                var ani = obj.GetComponent<Animator>();
                if (ani != null && ani.runtimeAnimatorController == null)
                {
                    UnityEngine.Object.DestroyImmediate(ani);
                }
                var a = obj.GetComponent<Animation>();
                if (a != null && a.GetClipCount() <= 0)
                {
                    UnityEngine.Object.DestroyImmediate(a);
                }
                var prefab = PrefabUtility.CreateEmptyPrefab(m_packageFolder + folderName + "/" + variantType.ToString() + "/" + meshRender.gameObject.name + ".prefab");
                PrefabUtility.ReplacePrefab(obj, prefab);
                UnityEngine.Object.DestroyImmediate(obj);
            }
            var cloneRoot = Object.Instantiate(root);
            m_config.objs = new List<SceneObj>();
            TraverseChildNode(cloneRoot.transform);

            var rootPrefab = PrefabUtility.CreateEmptyPrefab(m_packageFolder + folderName + "/" + variantType.ToString() + "/root.prefab");
            PrefabUtility.ReplacePrefab(cloneRoot, rootPrefab);
            UnityEngine.Object.DestroyImmediate(cloneRoot);

            if (RenderSettings.fog)
            {
                m_config.fog = new SceneFog();
                m_config.fog.colorR = RenderSettings.fogColor.r;
                m_config.fog.colorG = RenderSettings.fogColor.g;
                m_config.fog.colorB = RenderSettings.fogColor.b;
                m_config.fog.colorA = RenderSettings.fogColor.a;
                m_config.fog.fogMode = (int)RenderSettings.fogMode;
                m_config.fog.fogDensity = RenderSettings.fogDensity;
                m_config.fog.fogStartDistance = RenderSettings.fogStartDistance;
                m_config.fog.fogEndDistance = RenderSettings.fogEndDistance;
            }
            if (RenderSettings.skybox != null)
            {
                if (!RenderSettings.skybox.name.Contains("Default-"))
                {
                    AssetDatabase.CopyAsset(UnityEditor.AssetDatabase.GetAssetPath(RenderSettings.skybox), m_packageFolder + folderName + "/" + variantType.ToString() + "/" + m_config.skybox + ".mat");
                    m_config.skybox = RenderSettings.skybox.name;
                }
            }
            if (LightmapSettings.lightmaps != null)
            {
                m_config.lightmap = new SceneLightmap();
                m_config.lightmap.mode = (int)LightmapSettings.lightmapsMode;
                m_config.lightmap.datas = new List<SceneLightmapData>();
                for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
                {
                    Texture2D color = LightmapSettings.lightmaps[i].lightmapColor;
                    Texture2D dir = LightmapSettings.lightmaps[i].lightmapDir;
                    string colorName = color != null ? color.name.Trim() : null;
                    if (!string.IsNullOrEmpty(colorName))
                    {
                        AssetDatabase.CopyAsset(UnityEditor.AssetDatabase.GetAssetPath(color), m_packageFolder + folderName + "/" + variantType.ToString() + "/" + colorName + ".exr");
                    }
                    string dirName = dir != null ? dir.name.Trim() : null;
                    if (string.IsNullOrEmpty(dirName))
                    {
                        AssetDatabase.CopyAsset(UnityEditor.AssetDatabase.GetAssetPath(dir), m_packageFolder + folderName +"/" + variantType.ToString() + "/" + dirName + ".exr");
                    }
                    m_config.lightmap.datas.Add(new SceneLightmapData()
                    {
                        color = colorName,
                        dir = dirName
                    });
                }
            }
            var lightObj = GameObject.Find(m_lightName);
            if (lightObj != null)
            {
                m_config.lights = new List<SceneLight>();
                var lights = lightObj.GetComponentsInChildren<UnityEngine.Light>();
                for (int i = 0; i < lights.Length; i++)
                {
                    var light = lights[i];
                    var lightConfig = new SceneLight();
                    lightConfig.colorR = light.color.r;
                    lightConfig.colorG = light.color.g;
                    lightConfig.colorB = light.color.b;
                    lightConfig.colorA = light.color.a;
                    lightConfig.eulerAngleX = light.transform.eulerAngles.x;
                    lightConfig.eulerAngleY = light.transform.eulerAngles.y;
                    lightConfig.eulerAngleZ = light.transform.eulerAngles.z;
                    lightConfig.positionX = light.transform.position.x;
                    lightConfig.positionY = light.transform.position.y;
                    lightConfig.positionZ = light.transform.position.z;
                    lightConfig.intensity = light.intensity;
                    lightConfig.type = (int)light.type;
                    lightConfig.range = light.range;
                    lightConfig.spotAngle = light.spotAngle;
                    lightConfig.width = light.areaSize.x;
                    lightConfig.height = light.areaSize.y;
                    m_config.lights.Add(lightConfig);
                }
            }
            byte[] bytes = Serialize<SceneConfig>(m_config);
            using (FileStream stream = new FileStream(m_packageFolder + folderName + "/" + variantType.ToString() + "/config.bytes", FileMode.Create, FileAccess.Write))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            AssetDatabase.Refresh();
            ToBundle(m_packageFolder + folderName, variantType);
        }
        private void ToGenerate()
        {
            var length=System.Enum.GetValues(typeof(EVariantType)).Length;
            for (int i = 0; i < length; i++)
            {
                var root = GameObject.Find(m_rootName + "-" + ((EVariantType)i).ToString());
                if(root!=null)
                {
                    ToGenerateVariant(root, (EVariantType)i);
                }
            }
        }
        private void ToBundle(string path ,EVariantType variantType)
        {
            var files = Directory.GetFiles(path+ "/" + variantType.ToString());
            for (int i = 0; i < files.Length; i++)
            {
                string bundlePath = files[i];
                if (bundlePath.Contains(".meta")) continue;
                AssetImporter importer = AssetImporter.GetAtPath(bundlePath);
                if (importer != null) {
                    importer.assetBundleName = path.Substring(path.LastIndexOf('/')+1);
                    importer.assetBundleVariant = variantType.ToString();
                }
            }
            string bundleFolder = m_bundlePath + "/Scene";
            if (!Directory.Exists(bundleFolder))
                Directory.CreateDirectory(bundleFolder);
            BuildPipeline.BuildAssetBundles(bundleFolder, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        }
        private void TraverseChildNode(Transform root)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    var obj = new SceneObj();
                    obj.parentnames = new List<string>();
                    GetParentNames(child, ref obj.parentnames);
                    if (mr.transform.childCount>0)
                    {
                        UnityEngine.Debug.LogError("MeshRender has childs ,is error :"+ obj.parentnames);
                        continue;
                    }
                    obj.name = child.name;
                    obj.positionX = child.localPosition.x;
                    obj.positionT = child.localPosition.y;
                    obj.positionZ = child.localPosition.z;

                    obj.rotationX = child.transform.localEulerAngles.x;
                    obj.rotationY = child.transform.localEulerAngles.y;
                    obj.rotationZ = child.transform.localEulerAngles.z;

                    obj.scaleX = child.transform.localScale.x;
                    obj.scaleY = child.transform.localScale.y;
                    obj.scaleZ = child.transform.localScale.z;

                    obj.lightmapIndex = mr.lightmapIndex;
                    obj.lightmapScaleOffsetx = mr.lightmapScaleOffset.x;
                    obj.lightmapScaleOffsety = mr.lightmapScaleOffset.y;
                    obj.lightmapScaleOffsetz = mr.lightmapScaleOffset.z;
                    obj.lightmapScaleOffsetw = mr.lightmapScaleOffset.w;
                    m_config.objs.Add(obj);
                    UnityEngine.Object.DestroyImmediate(mr.gameObject);
                }
                else
                {
                    TraverseChildNode(child);
                }
            }
        }
        private void GetParentNames(Transform root, ref List<string> list)
        {
            if (root.parent != null)
            {
                list.Insert(0, root.parent.name);
                GetParentNames(root.parent, ref list);
            }
        }
        private void DelectDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (System.Exception e)
            {
                throw;
            }
        }
        private byte[] Serialize<T>(T instance)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, instance);
                bytes = ms.ToArray();
            }
            return bytes;
        }
    }
}