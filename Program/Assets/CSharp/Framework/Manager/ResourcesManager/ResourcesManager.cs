using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
    using System.IO;
    using System.Threading.Tasks;

    public class ResourcesManager:Singleton<ResourcesManager>
    {
        public static ISource source { get; private set; }
        public override async Task Initialize(System.Action onInitialOver = null)
        {
#if DEVELOPMENT
            source=new LocalSource();
#else
            source = new BundleSource();
#endif
            source.Initialize();
        }
        public void Update()
        {
            source.Update();
        }
        public void LateUpdate()
        {
            source.LateUpdate();
        }
        public override void Dispose()
        {
            source.Dispose();
            source = null;
            base.Dispose();
        }
        public static void LoadAsset(int assetId, DataHandler<UnityEngine.Object> onLoadOver,EVariantType variant=EVariantType.n,bool toAsync=true)
        {
            source.Load(assetId, onLoadOver, variant, toAsync);
        }
        public static void LoadAsset(int assetId, DataHandler<UnityEngine.Object[]> onLoadOver, EVariantType variant = EVariantType.n,bool toAsync=true)
        {
            source.Load(assetId, onLoadOver, variant, toAsync);
        }
        public static string LoadFile(string path)
        {
            var filePath =Path.Combine( Define.sourceUrl ,path);
            string file = null;
            using (FileStream fileStream=new FileStream(filePath,FileMode.Open,FileAccess.Read))
            {
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                file = System.Text.Encoding.UTF8.GetString(bytes);
            }
            return file;
        }
        public static string LoadAbsoluteFile(string filePath)
        {
            string file = null;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                file = System.Text.Encoding.UTF8.GetString(bytes);
            }
            return file;
        }
        public static byte[] LoadBytes(string path)
        {
            var filePath =Path.Combine( Define.sourceBundleUrl ,path);
            byte[] bytes = null;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
            }
            return bytes;
        }
        public static bool Exists(string path)
        {
            return File.Exists(Path.Combine(Define.sourceUrl, path));
        }
        public static FileStream LoadStream(string path)
        {
            var filePath = Define.sourceBundleUrl + path;
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        public static FileStream LoadAbsoluteStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        public static void Destroy(int assetId,EVariantType variant=EVariantType.n)
        {
            source.Destroy(assetId);
        }
        public static void Unload(int assetId,EVariantType variant = EVariantType.n)
        {
            source.UnLoad(assetId, variant);
        }
        public static void UnloadUnusedAsset()
        {
            source.UnloadUnusedAsset();
        }
    }
}
