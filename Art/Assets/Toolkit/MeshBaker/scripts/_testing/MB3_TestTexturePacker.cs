using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalOpus.MB.Core;

public class MB3_TestTexturePacker : MonoBehaviour {

    public enum PackerType
    {
        regular,
        horizontal,
        vertical
    }

    public PackerType packerType;

    MB2_TexturePacker texturePacker;

    public int numTex = 32;
    public int minX = 126;
    public int maxX = 2046;
    public int minY = 128;
    public int maxY = 256;
    public float xMult = 1;
    public float yMult = 1;
    public bool imgsMustBePowerOfTwo;
    public List<Vector2> imgsToAdd = new List<Vector2>();

    public int padding = 1;
    public int maxDim = 4096;
    public bool atlasMustBePowerOfTwo = true;
    public bool doMultiAtlas;
    public MB2_LogLevel logLevel;
    public string res;

    public AtlasPackingResult[] rs;

    [ContextMenu("Generate List Of Images To Add")]
    public void GenerateListOfImagesToAdd()
    {
        imgsToAdd = new List<Vector2>();
        for (int i = 0; i < numTex; i++)
        {
            Vector2 img = new Vector2(Mathf.RoundToInt(UnityEngine.Random.Range(minX, maxX) * xMult), Mathf.RoundToInt(UnityEngine.Random.Range(minY, maxY) * yMult));
            if (imgsMustBePowerOfTwo)
            {
                img.x = MB2_TexturePacker.RoundToNearestPositivePowerOfTwo((int)img.x);
                img.y = MB2_TexturePacker.RoundToNearestPositivePowerOfTwo((int)img.y);
            }
            imgsToAdd.Add(img);
        }
    }

    [ContextMenu("Run")]
    public void RunTestHarness()
    {
        if (packerType == PackerType.regular)
        {
            texturePacker = new MB2_TexturePackerRegular();
        } else if (packerType == PackerType.horizontal)
        {
            MB2_TexturePackerHorizontalVert tp = new MB2_TexturePackerHorizontalVert();
            tp.packingOrientation = MB2_TexturePackerHorizontalVert.TexturePackingOrientation.horizontal;
            texturePacker = tp;
        } else if (packerType == PackerType.vertical)
        {
            MB2_TexturePackerHorizontalVert tp = new MB2_TexturePackerHorizontalVert();
            tp.packingOrientation = MB2_TexturePackerHorizontalVert.TexturePackingOrientation.vertical;
            texturePacker = tp;
        }

        texturePacker.atlasMustBePowerOfTwo = atlasMustBePowerOfTwo;
        texturePacker.LOG_LEVEL = logLevel;

        rs = texturePacker.GetRects(imgsToAdd, maxDim, padding, doMultiAtlas);
        if (rs != null)
        {
            Debug.Log("NumAtlas= " + rs.Length);
            for (int i = 0; i < rs.Length; i++)
            {
                Debug.Log("AtlasSize " + rs[i].atlasX + " mxY= " + rs[i].atlasY);
                for (int j = 0; j < rs[i].rects.Length; j++)
                {
                    Rect r = rs[i].rects[j];
                    r.x *= rs[i].atlasX;
                    r.y *= rs[i].atlasY;
                    r.width *= rs[i].atlasX;
                    r.height *= rs[i].atlasY;
                    Debug.Log(r.ToString("f5"));
                }
                Vector2 offset = new Vector2((i * 1.5f) * maxDim, 0);
                AtlasPackingResult apr = rs[i];
                Vector2 center = new Vector2(offset.x + apr.atlasX / 2, offset.y + apr.atlasY / 2);
                Vector2 sz = new Vector2(apr.atlasX, apr.atlasY);
                
                Debug.Log("===============");
            }
            if (rs.Length > 0) res = "mxX= " + rs[0].atlasX + " mxY= " + rs[0].atlasY;
        } else
        {
            res = "ERROR: PACKING FAILED";
        }
    }

    private void OnDrawGizmos()
    {
        if (rs != null)
        {
            float scale = .01f;
            for (int i = 0; i < rs.Length; i++)
            {
                Vector2 offset = new Vector2((i * 1.5f) * maxDim, 0);
                AtlasPackingResult apr = rs[i];
                Vector2 center = (new Vector2(offset.x + apr.atlasX/2, offset.y + apr.atlasY/2)) * scale;
                Vector2 sz = (new Vector2(apr.atlasX, apr.atlasY)) * scale;
                Gizmos.DrawWireCube(center, new Vector3(sz.x, sz.y, .1f));
                
                for (int j = 0; j < rs[i].rects.Length; j++)
                {
                    Rect r = rs[i].rects[j];
                    Gizmos.color = new Color(Random.value, Random.value, Random.value);
                    center = (new Vector2(offset.x + (r.x + r.width / 2f) * rs[i].atlasX, offset.y + (r.y + r.height / 2f) * rs[i].atlasY)) * scale;
                    Vector2 szz = (new Vector2(r.width * rs[i].atlasX, r.height * rs[i].atlasY)) * scale;
                    Gizmos.DrawCube(center, szz);
                }
                
            }
        }
    }

    [ContextMenu("Test1")]
    // multiAtlas=true, powerOfTwo, padding=8 in X
    void Test1()
    {
        texturePacker = new MB2_TexturePackerRegular();
        texturePacker.atlasMustBePowerOfTwo = true;
        List<Vector2> images = new List<Vector2>();
        images.Add(new Vector2(450, 200));
        images.Add(new Vector2(450, 200));
        images.Add(new Vector2(450, 80));
        texturePacker.LOG_LEVEL = logLevel;
        rs = texturePacker.GetRects(images, 512, 8, true);
        Debug.Assert(rs.Length == 2);
        /*
        Debug.Assert(rs[0].atlasX == 512, rs[0].atlasX);
        Debug.Assert(rs[0].atlasY == 512, rs[0].atlasY);
        Debug.Assert(rs[0].usedW == 466, rs[0].usedW);
        Debug.Assert(rs[0].usedH == 432, rs[0].usedH);
        Debug.Assert(rs[1].atlasX == 512, rs[1].atlasX);
        Debug.Assert(rs[1].atlasY == 256, rs[1].atlasY);
        Debug.Assert(rs[1].usedW == 466, rs[1].usedW);
        Debug.Assert(rs[1].usedH == 96, rs[1].usedH);
        */
        Debug.Log("Success! ");
    }

    [ContextMenu("Test2")]
    // multiAtlas=true, powerOfTwo, padding=8 in Y
    void Test2()
    {
        texturePacker = new MB2_TexturePackerRegular();
        texturePacker.atlasMustBePowerOfTwo = true;
        List<Vector2> images = new List<Vector2>();
        images.Add(new Vector2(200, 450));
        images.Add(new Vector2(200, 450));
        images.Add(new Vector2(80, 450));
        texturePacker.LOG_LEVEL = logLevel;
        rs = texturePacker.GetRects(images, 512, 8, true);
        Debug.Assert(rs.Length == 2);
        /*
        Debug.Assert(rs[0].atlasX == 512, rs[0].atlasX);
        Debug.Assert(rs[0].atlasY == 512, rs[0].atlasY);
        Debug.Assert(rs[0].usedW == 432, rs[0].usedW);
        Debug.Assert(rs[0].usedH == 466, rs[0].usedH);
        Debug.Assert(rs[1].atlasX == 256, rs[1].atlasX);
        Debug.Assert(rs[1].atlasY == 512, rs[1].atlasY);
        Debug.Assert(rs[1].usedW == 96, rs[1].usedW);
        Debug.Assert(rs[1].usedH == 466, rs[1].usedH);
        */
        Debug.Log("Success! ");
    }

    [ContextMenu("Test3")]
    // multiAtlas=true, powerOfTwo=false, padding=8 in X
    void Test3()
    {
        texturePacker = new MB2_TexturePackerRegular();
        texturePacker.atlasMustBePowerOfTwo = false;
        List<Vector2> images = new List<Vector2>();
        images.Add(new Vector2(450, 200));
        images.Add(new Vector2(450, 200));
        images.Add(new Vector2(450, 80));
        texturePacker.LOG_LEVEL = logLevel;
        rs = texturePacker.GetRects(images, 512, 8, true);
        Debug.Assert(rs.Length == 2);
        /*
        Debug.Assert(rs[0].atlasX == 466, rs[0].atlasX);
        Debug.Assert(rs[0].atlasY == 432, rs[0].atlasY);
        Debug.Assert(rs[0].usedW == 466, rs[0].usedW);
        Debug.Assert(rs[0].usedH == 432, rs[0].usedH);
        Debug.Assert(rs[1].atlasX == 466, rs[1].atlasX);
        Debug.Assert(rs[1].atlasY == 96, rs[1].atlasY);
        Debug.Assert(rs[1].usedW == 466, rs[1].usedW);
        Debug.Assert(rs[1].usedH == 96, rs[1].usedH);
        */
        Debug.Log("Success! ");
    }

    [ContextMenu("Test4")]
    // multiAtlas=true, powerOfTwo=false, padding=8 in Y
    void Test4()
    {
        texturePacker = new MB2_TexturePackerRegular();
        texturePacker.atlasMustBePowerOfTwo = false;
        List<Vector2> images = new List<Vector2>();
        images.Add(new Vector2(200, 450));
        images.Add(new Vector2(200, 450));
        images.Add(new Vector2(80, 450));
        texturePacker.LOG_LEVEL = logLevel;
        rs = texturePacker.GetRects(images, 512, 8, true);
        Debug.Assert(rs.Length == 2);
        /*
        Debug.Assert(rs[0].atlasX == 432, rs[0].atlasX);
        Debug.Assert(rs[0].atlasY == 466, rs[0].atlasY);
        Debug.Assert(rs[0].usedW == 432, rs[0].usedW);
        Debug.Assert(rs[0].usedH == 466, rs[0].usedH);
        Debug.Assert(rs[1].atlasX == 96, rs[1].atlasX);
        Debug.Assert(rs[1].atlasY == 466, rs[1].atlasY);
        Debug.Assert(rs[1].usedW == 96, rs[1].usedW);
        Debug.Assert(rs[1].usedH == 466, rs[1].usedH);
        */
        Debug.Log("Success! ");
    }
}
