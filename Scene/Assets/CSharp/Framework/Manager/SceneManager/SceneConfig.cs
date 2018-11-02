using System;
using System.Collections.Generic;
using ProtoBuf;
namespace lhFramework.Infrastructure.Managers
{
    [ProtoContract]
    public class SceneConfig
    {
        [ProtoMember(1)]
        public SceneFog fog;
        [ProtoMember(2)]
        public List<SceneLight> lights;
        [ProtoMember(3)]
        public List<SceneObj> objs;
        [ProtoMember(4)]
        public string skybox;
        [ProtoMember(5)]
        public SceneLightmap lightmap;
    }
    [ProtoContract]
    public class SceneObj
    {
        [ProtoMember(1)]
        public string name;
        [ProtoMember(2)]
        public float positionX;
        [ProtoMember(3)]
        public float positionT;
        [ProtoMember(4)]
        public float positionZ;
        [ProtoMember(5)]
        public float rotationX;
        [ProtoMember(6)]
        public float rotationY;
        [ProtoMember(7)]
        public float rotationZ;
        [ProtoMember(8)]
        public float scaleX;
        [ProtoMember(9)]
        public float scaleY;
        [ProtoMember(10)]
        public float scaleZ;

        [ProtoMember(11)]
        public int lightmapIndex;
        [ProtoMember(12)]
        public float lightmapScaleOffsetx;
        [ProtoMember(13)]
        public float lightmapScaleOffsety;
        [ProtoMember(14)]
        public float lightmapScaleOffsetz;
        [ProtoMember(15)]
        public float lightmapScaleOffsetw;

        [ProtoMember(16)]
        public List<string> parentnames;
    }
    [ProtoContract]
    public class SceneFog
    {
        [ProtoMember(1)]
        public float colorR;
        [ProtoMember(2)]
        public float colorG;
        [ProtoMember(3)]
        public float colorB;
        [ProtoMember(4)]
        public float colorA;
        [ProtoMember(5)]
        public int fogMode;
        [ProtoMember(6)]
        public float fogDensity;
        [ProtoMember(7)]
        public float fogStartDistance;
        [ProtoMember(8)]
        public float fogEndDistance;
    }
    [ProtoContract]
    public class SceneLight
    {
        [ProtoMember(1)]
        public float positionX;
        [ProtoMember(2)]
        public float positionY;
        [ProtoMember(3)]
        public float positionZ;
        [ProtoMember(4)]
        public float eulerAngleX;
        [ProtoMember(5)]
        public float eulerAngleY;
        [ProtoMember(6)]
        public float eulerAngleZ;
        [ProtoMember(7)]
        public int type;
        [ProtoMember(8)]
        public float colorR;
        [ProtoMember(9)]
        public float colorG;
        [ProtoMember(10)]
        public float colorB;
        [ProtoMember(11)]
        public float colorA;
        [ProtoMember(12)]
        public float intensity;
        //--------------spot & Point
        [ProtoMember(13)]
        public float range;
        [ProtoMember(14)]
        public float spotAngle;
        //--------------area
        [ProtoMember(15)]
        public float width;
        [ProtoMember(16)]
        public float height;
    }
    [ProtoContract]
    public class SceneLightmap
    {
        [ProtoMember(1)]
        public int mode;
        [ProtoMember(2)]
        public List<SceneLightmapData> datas;
    }
    [ProtoContract]
    public class SceneLightmapData
    {
        [ProtoMember(1)]
        public string color;
        [ProtoMember(2)]
        public string dir;
    }
}
