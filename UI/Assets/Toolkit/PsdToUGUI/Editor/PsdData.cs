using System;
using System.Collections.Generic;

namespace lhFramework.Tools.PsdToUGUI
{
    public enum ImageSource
    {
        Common,
        Custom,
        Global,

        /// <summary>
        /// 自定义图集
        /// </summary>
        CustomAtlas,
    }
    public enum ImageType
    {
        Image,
        Texture,
        Label,
        SliceImage,
        LeftHalfImage,
        BottomHalfImage,
        QuarterImage,
    }
    public enum LayerType
    {
        Panel,
        Normal,
        ScrollView,
        Grid,
        Button,
        Lable,
        Toggle,
        Slider,
        Group,
        InputField,
        ScrollBar,
        LayoutElement,
        TabGroup,
    }
    public class PsdData
    {
        public Size psdSize;
        public Layer[] layers;
    }
    public class Layer
    {
        public string name;
        public LayerType type;
        public Layer[] layers;
        public string[] arguments;
        //public PSImage[] images;
        public PSImage image;
        public Size size;
        public Position position;
    }
    public class PSImage
    {
        public ImageType imageType;
        public ImageSource imageSource;
        public string name;
        public Position position;
        public Size size;

        public string[] arguments;

        /// <summary>
        /// 图层透明度
        /// </summary>
        public float opacity = -1;

        /// <summary>
        /// 渐变,这个需要自己写脚本支持,这里只是提供接口
        /// </summary>
        public string gradient = "";

        /// <summary>
        /// 描边
        /// </summary>
        public string outline = "";
    }
    public class Position
    {
        public float x;
        public float y;
    }
    public class Size
    {
        public float width;
        public float height;
    }
}
