using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace lhFramework.Tools.PsdToUGUI
{
    public enum EFileType
    {
        Texture,
        Font,
        Common,
        Panel
    }
    public class PsdToManager
    {
        public class Data
        {
            public string name;
            public string assetPath;
        }
        public class SourceData
        {
            public string name;
            public EFileType type;
            public List<Data> sprites=new List<Data>();
            public Data xml;
            public Data prefab;
            public Data psd;
            public Data spriteAtlas;
            public EVariantType variant;
        }
        public List<SourceData> panels=new List<SourceData>();
        public List<SourceData> commons=new List<SourceData>();

        public List<SourceData> fonts=new List<SourceData>();
        public List<SourceData> textures=new List<SourceData>();

        public PsdToManager(string versionFolderName)
        {
            var versions = Directory.GetDirectories(Path.Combine(Application.dataPath, "Package/"+versionFolderName),"*",SearchOption.TopDirectoryOnly);
            for (int i = 0; i < versions.Length; i++)
            {
                var datas = Directory.GetDirectories(versions[i], "*", SearchOption.TopDirectoryOnly);
                for (int j = 0; j < datas.Length; j++)
                {
                    EFileType type = EFileType.Common;
                    if (datas[j].Contains("Common"))
                        type = EFileType.Common;
                    else if (datas[j].Contains("Font"))
                        type = EFileType.Font;
                    else if (datas[j].Contains("Image"))
                        type = EFileType.Texture;
                    else if (datas[j].Contains("Panel"))
                        type = EFileType.Panel;
                    else
                        UnityEngine.Debug.LogWarning("Dont has this fileType:" + datas[j]);
                    var fileDirs = Directory.GetDirectories(datas[j], "*", SearchOption.TopDirectoryOnly);
                    for (int h = 0; h < fileDirs.Length; h++)
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(fileDirs[h]);
                        SourceData d = new SourceData();
                        var pngs = Directory.GetFiles(fileDirs[h], "*.png", SearchOption.AllDirectories);
                        var prefabs = Directory.GetFiles(fileDirs[h], "*.prefab", SearchOption.AllDirectories);
                        var psds = Directory.GetFiles(fileDirs[h], "*.psd", SearchOption.AllDirectories);
                        var xmls = Directory.GetFiles(fileDirs[h], "*.xml", SearchOption.AllDirectories);
                        var spriteAtlas = Directory.GetFiles(fileDirs[h], "*.spriteatlas", SearchOption.AllDirectories);
                        d.type = type;
                        d.name = dirInfo.Name;
                        for (int m = 0; m < pngs.Length; m++)
                        {
                            FileInfo info = new FileInfo(pngs[m]);
                            d.sprites.Add(new Data()
                            {
                                name= info.Name,
                                assetPath =("Assets"+ pngs[m].Replace(Application.dataPath,""))
                            });
                        }
                        if (prefabs != null && prefabs.Length > 0)
                        {
                            FileInfo info = new FileInfo(prefabs[0]);
                            d.prefab=new Data()
                            {
                                name = info.Name,
                                assetPath = ("Assets" + prefabs[0].Replace(Application.dataPath, ""))
                            };
                        }
                        if (psds != null && psds.Length > 0)
                        {
                            FileInfo info = new FileInfo(psds[0]);
                            d.psd = new Data()
                            {
                                name = info.Name,
                                assetPath = ("Assets" + psds[0].Replace(Application.dataPath, ""))
                            };
                        }
                        if (xmls != null && xmls.Length > 0)
                        {
                            FileInfo info = new FileInfo(xmls[0]);
                            d.xml = new Data()
                            {
                                name = info.Name,
                                assetPath = ("Assets" + xmls[0].Replace(Application.dataPath, ""))
                            };
                        }
                        if (spriteAtlas!=null && spriteAtlas.Length>0)
                        {
                            FileInfo info = new FileInfo(spriteAtlas[0]);
                            d.spriteAtlas = new Data()
                            {
                                name = info.Name,
                                assetPath = ("Assets" + spriteAtlas[0].Replace(Application.dataPath, ""))
                            };
                        }
                        if (type == EFileType.Common)
                        {
                            commons.Add(d);
                        }
                        else if (type == EFileType.Font)
                        {
                            fonts.Add(d);
                        }
                        else if (type == EFileType.Texture)
                        {
                            textures.Add(d);
                        }
                        else if (type == EFileType.Panel)
                        {
                            panels.Add(d);
                        }
                    }
                }
            }
        }
        public void XmlToPrefab()
        {

        }
        public void BuildPack()
        {

        }
    }
}
