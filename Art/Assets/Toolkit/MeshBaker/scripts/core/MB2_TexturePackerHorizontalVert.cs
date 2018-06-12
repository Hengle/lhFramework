using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


/*
 TODO
    vertical as well as horizontal
    if images different heights should scale all of them to be the max

Tests For All Texture Packers
    can handle images larger than maxdim
    
*/
 
namespace DigitalOpus.MB.Core{
    // uses this algorithm http://blackpawn.com/texts/lightmaps/
    public class MB2_TexturePackerHorizontalVert : MB2_TexturePacker {
		
        public enum TexturePackingOrientation
        {
            horizontal,
            vertical
        }

        public TexturePackingOrientation packingOrientation;

        public bool stretchImagesToEdges = true;

        public override AtlasPackingResult[] GetRects(List<Vector2> imgWidthHeights, int maxDimension, int padding)
        {
            return GetRects(imgWidthHeights, maxDimension, padding, false);
        }

        public override AtlasPackingResult[] GetRects(List<Vector2> imgWidthHeights, int maxDimension, int padding, bool doMultiAtlas){
            if (doMultiAtlas)
            {
                if (packingOrientation == TexturePackingOrientation.vertical)
                {
                    return _GetRectsMultiAtlasVertical(imgWidthHeights, maxDimension, padding, 2 + padding * 2, 2 + padding * 2, 2 + padding * 2, 2 + padding * 2);
                } else
                {
                    return _GetRectsMultiAtlasHorizontal(imgWidthHeights, maxDimension, padding, 2 + padding * 2, 2 + padding * 2, 2 + padding * 2, 2 + padding * 2);
                }
            }
            else
            {
                AtlasPackingResult apr = _GetRectsSingleAtlas(imgWidthHeights, maxDimension, padding, 2 + padding * 2, 2 + padding * 2, 2 + padding * 2, 2 + padding * 2, 0);
                if (apr == null)
                {
                    return null;
                } else
                {
                    return new AtlasPackingResult[] { apr };
                }
            }
		}

        AtlasPackingResult _GetRectsSingleAtlas(List<Vector2> imgWidthHeights, int maxDimension, int padding, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY, int recursionDepth)
        {
            AtlasPackingResult res = new AtlasPackingResult();

            List<Rect> rects = new List<Rect>();
            int extent = 0;
            int maxh = 0;
            int maxw = 0;
            List<Image> images = new List<Image>();
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Packing rects for: " + imgWidthHeights.Count);
            for (int i = 0; i < imgWidthHeights.Count; i++)
            {
                Image im = new Image(i, (int) imgWidthHeights[i].x, (int) imgWidthHeights[i].y, padding, minImageSizeX, minImageSizeY);

                // if images are stacked horizontally then there is no padding at the top or bottom
                if (packingOrientation == TexturePackingOrientation.vertical)
                {
                    im.h -= padding * 2;
                    im.x = extent;
                    im.y = 0;
                    rects.Add(new Rect(im.w, im.h, extent, 0));
                    extent += im.w;
                    maxh = Mathf.Max(maxh, im.h);
                } else
                {
                    im.w -= padding * 2;
                    im.y = extent;
                    im.x = 0;
                    rects.Add(new Rect(im.w, im.h, 0, extent));
                    extent += im.h;
                    maxw = Mathf.Max(maxw, im.w);
                }
                images.Add(im);
            }
            //scale atlas to fit maxDimension
            Vector2 rootWH;
            if (packingOrientation == TexturePackingOrientation.vertical) { rootWH = new Vector2(extent, maxh); }
            else { rootWH = new Vector2(maxw,extent); }
            int outW = (int) rootWH.x;
            int outH = (int) rootWH.y;
            if (packingOrientation == TexturePackingOrientation.vertical) {
                if (atlasMustBePowerOfTwo)
                {
                    outW = Mathf.Min(CeilToNearestPowerOfTwo(outW), maxDimension);
                }
                else
                {
                    outW = Mathf.Min(outW, maxDimension);
                }
            } else
            {
                if (atlasMustBePowerOfTwo)
                {
                    outH = Mathf.Min(CeilToNearestPowerOfTwo(outH), maxDimension);
                }
                else
                {
                    outH = Mathf.Min(outH, maxDimension);
                }
            }

            float padX, padY;
            int newMinSizeX, newMinSizeY;
            if (!ScaleAtlasToFitMaxDim(rootWH, images, maxDimension, padding, minImageSizeX, minImageSizeY, masterImageSizeX, masterImageSizeY,
                ref outW, ref outH, out padX, out padY, out newMinSizeX, out newMinSizeY))
            {

                res = new AtlasPackingResult();
                res.rects = new Rect[images.Count];
                res.srcImgIdxs = new int[images.Count];
                res.atlasX = outW;
                res.atlasY = outH;
                res.usedW = -1;
                res.usedH = -1;
                for (int i = 0; i < images.Count; i++)
                {
                    Image im = images[i];
                    Rect r;
                    if (packingOrientation == TexturePackingOrientation.vertical)
                    {
                        r = res.rects[i] = new Rect((float)im.x / (float)outW + padX,
                                                             (float)im.y / (float)outH,
                                                             (float)im.w / (float)outW - padX * 2f,
                                                             stretchImagesToEdges ? 1f : (float)im.h / (float)outH); // all images are stretched to fill the height
                    } else
                    {
                        r = res.rects[i] = new Rect((float)im.x / (float)outW,
                                                             (float)im.y / (float)outH + padY,
                                                             (stretchImagesToEdges ? 1f : ((float)im.w / (float)outW)),
                                                             (float)im.h / (float)outH - padY * 2f); // all images are stretched to fill the height
                    }
                    res.srcImgIdxs[i] = im.imgId;
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Image: " + i + " imgID=" + im.imgId + " x=" + r.x * outW +
                               " y=" + r.y * outH + " w=" + r.width * outW +
                               " h=" + r.height * outH + " padding=" + padding + " outW=" + outW + " outH=" + outH);
                }
                return res;
            }
            Debug.Log("Packing failed returning null atlas result");
            return null;
        }

        AtlasPackingResult[] _GetRectsMultiAtlasVertical(List<Vector2> imgWidthHeights, int maxDimensionPassed, int padding, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY)
        {
            List<AtlasPackingResult> rs = new List<AtlasPackingResult>();
            int extent = 0;
            int maxh = 0;
            int maxw = 0;
            
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Packing rects for: " + imgWidthHeights.Count);
            
            List<Image> allImages = new List<Image>();
            for (int i = 0; i < imgWidthHeights.Count; i++)
            {
                Image im = new Image(i, (int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, padding, minImageSizeX, minImageSizeY);
                im.h -= padding * 2;
                allImages.Add(im);
            }
            allImages.Sort(new ImageWidthComparer());
            List<Image> images = new List<Image>();
            List<Rect> rects = new List<Rect>();
            int spaceRemaining = maxDimensionPassed;
            while (allImages.Count > 0 || images.Count > 0)
            {
                Image im = PopLargestThatFits(allImages, spaceRemaining, maxDimensionPassed, images.Count == 0);
                if (im == null)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Atlas filled creating a new atlas ");
                    AtlasPackingResult apr = new AtlasPackingResult();
                    apr.atlasX = maxw;
                    apr.atlasY = maxh;
                    Rect[] rss = new Rect[images.Count];
                    int[] srcImgIdx = new int[images.Count];
                    for (int j = 0; j < images.Count; j++)
                    {
                        Rect r = new Rect(images[j].x, images[j].y, 
                                        images[j].w, 
                                        stretchImagesToEdges ? maxh : images[j].h);
                        rss[j] = r;
                        srcImgIdx[j] = images[j].imgId;
                    }
                    apr.rects = rss;
                    apr.srcImgIdxs = srcImgIdx;

                    images.Clear();
                    rects.Clear();
                    extent = 0;
                    maxh = 0;
                    rs.Add(apr);
                    spaceRemaining = maxDimensionPassed;
                } else
                {
                    im.x = extent;
                    im.y = 0;
                    images.Add(im);
                    rects.Add(new Rect(extent, 0, im.w, im.h));
                    extent += im.w;
                    maxh = Mathf.Max(maxh, im.h);
                    maxw = extent;
                    spaceRemaining = maxDimensionPassed - extent;
                }
            }

            for (int i = 0; i < rs.Count; i++)
            {
                int outW = rs[i].atlasX;
                int outH = Mathf.Min(rs[i].atlasY, maxDimensionPassed);
                if (atlasMustBePowerOfTwo)
                {
                    outW = Mathf.Min(CeilToNearestPowerOfTwo(outW), maxDimensionPassed);
                }
                else
                {
                    outW = Mathf.Min(outW, maxDimensionPassed);
                }
                rs[i].atlasX = outW;
                //-------------------------------
                //scale atlas to fit maxDimension
                float padX, padY;
                int newMinSizeX, newMinSizeY;
                ScaleAtlasToFitMaxDim(new Vector2(rs[i].atlasX, rs[i].atlasY), images, maxDimensionPassed, padding, minImageSizeX, minImageSizeY, masterImageSizeX, masterImageSizeY,
                                     ref outW, ref outH, out padX, out padY, out newMinSizeX, out newMinSizeY);
            }

            

            //normalize atlases so that that rects are 0 to 1
            for (int i = 0; i < rs.Count; i++) {
                normalizeRects(rs[i], padding, 0);
            }
            //-----------------------------
            return rs.ToArray();
        }

        AtlasPackingResult[] _GetRectsMultiAtlasHorizontal(List<Vector2> imgWidthHeights, int maxDimensionPassed, int padding, int minImageSizeX, int minImageSizeY, int masterImageSizeX, int masterImageSizeY)
        {
            List<AtlasPackingResult> rs = new List<AtlasPackingResult>();
            int extent = 0;
            int maxh = 0;
            int maxw = 0;

            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Packing rects for: " + imgWidthHeights.Count);

            List<Image> allImages = new List<Image>();
            for (int i = 0; i < imgWidthHeights.Count; i++)
            {
                Image im = new Image(i, (int)imgWidthHeights[i].x, (int)imgWidthHeights[i].y, padding, minImageSizeX, minImageSizeY);
                im.w -= padding * 2;
                allImages.Add(im);
            }
            allImages.Sort(new ImageHeightComparer());
            List<Image> images = new List<Image>();
            List<Rect> rects = new List<Rect>();
            int spaceRemaining = maxDimensionPassed;
            while (allImages.Count > 0 || images.Count > 0)
            {
                Image im = PopLargestThatFits(allImages, spaceRemaining, maxDimensionPassed, images.Count == 0);
                if (im == null)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Atlas filled creating a new atlas ");
                    AtlasPackingResult apr = new AtlasPackingResult();
                    apr.atlasX = maxw;
                    apr.atlasY = maxh;
                    Rect[] rss = new Rect[images.Count];
                    int[] srcImgIdx = new int[images.Count];
                    for (int j = 0; j < images.Count; j++)
                    {
                        Rect r = new Rect(images[j].x, images[j].y,
                                stretchImagesToEdges ? maxw : images[j].w,
                                images[j].h);
                        rss[j] = r;
                        srcImgIdx[j] = images[j].imgId;
                    }
                    apr.rects = rss;
                    apr.srcImgIdxs = srcImgIdx;

                    images.Clear();
                    rects.Clear();
                    extent = 0;
                    maxh = 0;
                    rs.Add(apr);
                    spaceRemaining = maxDimensionPassed;
                }
                else
                {
                    im.x = 0;
                    im.y = extent;
                    images.Add(im);
                    rects.Add(new Rect(0, extent, im.w, im.h));
                    extent += im.h;
                    maxw = Mathf.Max(maxw, im.w);
                    maxh = extent;
                    spaceRemaining = maxDimensionPassed - extent;
                }
            }

            for (int i = 0; i < rs.Count; i++)
            {
                int outH = rs[i].atlasY;
                int outW = Mathf.Min(rs[i].atlasX, maxDimensionPassed);
                if (atlasMustBePowerOfTwo)
                {
                    outH = Mathf.Min(CeilToNearestPowerOfTwo(outH), maxDimensionPassed);
                }
                else
                {
                    outH = Mathf.Min(outH, maxDimensionPassed);
                }
                rs[i].atlasY = outH;
                //-------------------------------
                //scale atlas to fit maxDimension
                float padX, padY;
                int newMinSizeX, newMinSizeY;
                ScaleAtlasToFitMaxDim(new Vector2(rs[i].atlasX, rs[i].atlasY), images, maxDimensionPassed, padding, minImageSizeX, minImageSizeY, masterImageSizeX, masterImageSizeY,
                                     ref outW, ref outH, out padX, out padY, out newMinSizeX, out newMinSizeY);
            }



            //normalize atlases so that that rects are 0 to 1
            for (int i = 0; i < rs.Count; i++)
            {
                normalizeRects(rs[i], padding, 0);
            }
            //-----------------------------
            return rs.ToArray();
        }

        Image PopLargestThatFits(List<Image> images, int spaceRemaining, int maxDim, bool emptyAtlas)
        {
            //pop single images larger than maxdim into their own atlas
            int imageDim;
            if (images.Count == 0)
            {
                return null;
            } 

            if (packingOrientation == TexturePackingOrientation.vertical)
            {
                imageDim = images[0].w;
            } else
            {
                imageDim = images[0].h;
            }
            if (images.Count > 0 && imageDim >= maxDim)
            {
                if (emptyAtlas)
                {
                    Image im = images[0];
                    images.RemoveAt(0);
                    return im;
                } else
                {
                    return null;
                }
            }

            // now look for images that will fit
            int i = 0;
            while (i < images.Count && imageDim >= spaceRemaining)
            {
                i++;
            }
            if (i < images.Count)
            {
                Image im = images[i];
                images.RemoveAt(i);
                return im;
            } else
            {
                return null;
            }
        }
    }

}