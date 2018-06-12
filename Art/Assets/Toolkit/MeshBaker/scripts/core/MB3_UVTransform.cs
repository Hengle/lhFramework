using UnityEngine;
using System.Collections;
using System;

// A UV Transform is a transform considering only scale and offset it is used to represent the scaling and offset
// from UVs outside the 0,0..1,1 box and material tiling
// Rect objects are used to store the transform

namespace DigitalOpus.MB.Core
{
    public struct DRect
    {
        public double x;
        public double y;
        public double width;
        public double height;

        public DRect(Rect r)
        {
            x = r.x;
            y = r.y;
            width = r.width;
            height = r.height;
        }

        public DRect(Vector2 o, Vector2 s)
        {
            x = o.x;
            y = o.y;
            width = s.x;
            height = s.y;
        }

        public DRect(float xx, float yy, float w, float h)
        {
            x = xx;
            y = yy;
            width = w;
            height = h;
        }

        public DRect(double xx, double yy, double w, double h)
        {
            x = xx;
            y = yy;
            width = w;
            height = h;
        }

        public Rect GetRect()
        {
            return new Rect((float)x, (float)y, (float)width, (float)height);
        }

        public Vector2 min
        {
            get
            {
                return new Vector2((float)x, (float)y);
            }
        }

        public Vector2 max
        {
            get
            {
                return new Vector2((float)(x + width), (float)(y + width));
            }
        }

        public Vector2 size {
            get {
                return new Vector2((float)(width), (float)(width));
            }
        }

        public override bool Equals(object obj)
        {
            DRect dr = (DRect) obj;
            if (dr.x == x && dr.y == y && dr.width == width && dr.height == height)
            {
                return true;
            }
            return false;
        }

        public static bool operator ==(DRect a, DRect b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DRect a, DRect b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
                return String.Format("(x={0},y={1},w={2},h={3})", x.ToString("F5"), y.ToString("F5"), width.ToString("F5"), height.ToString("F5"));
        }


        public bool Encloses(DRect smallToTestIfFits)
        {
            double smnx = smallToTestIfFits.x;
            double smny = smallToTestIfFits.y;
            double smxx = smallToTestIfFits.x + smallToTestIfFits.width;
            double smxy = smallToTestIfFits.y + smallToTestIfFits.height;
            //expand slightly to deal with rounding errors
            double bmnx = this.x;
            double bmny = this.y;
            double bmxx = this.x + this.width;
            double bmxy = this.y + this.height;
            return bmnx <= smnx && smnx <= bmxx &&
                    bmnx <= smxx && smxx <= bmxx &&
                    bmny <= smny && smny <= bmxy &&
                    bmny <= smxy && smxy <= bmxy;
        }

		public override int GetHashCode ()
		{
			return x.GetHashCode() ^ y.GetHashCode() ^ width.GetHashCode() ^ height.GetHashCode();
		}
    }

    public class MB3_UVTransformUtility
    {
        public static void Test()
        {
            /*
            Debug.Log("Running test");
            DRect rawUV = new DRect(.25, .25, 1.5, 1.5);
            DRect rawMat = new DRect(.5, .5, 2, 2);
            DRect rawCombined = CombineTransforms(ref rawUV, ref rawMat);
            DRect matInvHierarchy = new DRect(rawMat.GetRect());
            InvertHierarchy(ref rawUV, ref matInvHierarchy);
            DRect invertHierarchyCombined = CombineTransforms(ref matInvHierarchy, ref rawUV);

            //These transforms should be the same
            Debug.Log("These should be same " + rawCombined + " " + invertHierarchyCombined);

            //New transform that should fit in  combined
            DRect otherUV = new DRect(1,1,1.5,1.5);
            DRect otherMat = new DRect(0, 0, 1, 1);
            DRect otherCombined = CombineTransforms(ref otherUV, ref otherMat);
            Debug.Log("Other : " + otherCombined);
            Debug.Log("Other fits = " + RectContains(ref rawCombined, ref otherCombined));

            DRect invOtherCombined = InverseTransform(ref otherCombined);
            Debug.Log(TransformPoint(ref otherCombined, new Vector2(0, 0)) + " " + TransformPoint(ref otherCombined, new Vector2(1, 1)));
            Debug.Log(TransformPoint(ref invOtherCombined, new Vector2(0,0)) + " " + TransformPoint(ref invOtherCombined, new Vector2(1,1)).ToString("f5")
                                         + " " + TransformPoint(ref invOtherCombined, new Vector2(2, 2)).ToString("f5")
                                          + " " + TransformPoint(ref invOtherCombined, new Vector2(3, 3)).ToString("f5"));

            DRect src2combined = CombineTransforms(ref invOtherCombined, ref rawCombined);

            Debug.Log(TransformPoint(ref src2combined, new Vector2(0, 0)) + " " + TransformPoint(ref src2combined, new Vector2(1, 1)).ToString("f5")
                                         + " " + TransformPoint(ref src2combined, new Vector2(2, 2)).ToString("f5")
                                          + " " + TransformPoint(ref src2combined, new Vector2(3, 3)).ToString("f5"));
            */
            //DRect rawUV = new DRect(0, 0, 1, 1);
            DRect rawMat = new DRect(.5, .5, 2, 2);
            DRect fullSample = new DRect(.25,.25,3,3);
            //DRect altasRect = new DRect(0, 0, 1, 1);

            DRect invRawMat = InverseTransform(ref rawMat);
            DRect invFullSample = InverseTransform(ref fullSample);
            DRect relativeTransform = CombineTransforms(ref rawMat, ref invFullSample);
            Debug.Log(invRawMat);
            Debug.Log(relativeTransform);
            Debug.Log("one mat trans " + TransformPoint(ref rawMat, new Vector2(1, 1)));
            Debug.Log("one inv mat trans " + TransformPoint(ref invRawMat, new Vector2(1, 1)).ToString("f4"));
            Debug.Log("zero " + TransformPoint(ref relativeTransform, new Vector2(0, 0)).ToString("f4"));
            Debug.Log("one " + TransformPoint(ref relativeTransform, new Vector2(1, 1)).ToString("f4"));
        }


        public static float TransformX(DRect r, double x)
        {
            return (float) (r.width * x + r.x);
        }

        public static DRect CombineTransforms(ref DRect r1, ref DRect r2)
        {
            DRect rCombined = new DRect(r1.x * r2.width + r2.x,
                        r1.y * r2.height + r2.y,
                        r1.width * r2.width,
                        r1.height * r2.height);
            //rCombined.x = rCombined.x - Mathf.FloorToInt(rCombined.x);
            //rCombined.y = rCombined.y - Mathf.FloorToInt(rCombined.y);
            return rCombined;
        }

        public static Rect CombineTransforms(ref Rect r1, ref Rect r2)
        {
            Rect rCombined = new Rect(r1.x * r2.width + r2.x,
                        r1.y * r2.height + r2.y,
                        r1.width * r2.width,
                        r1.height * r2.height);
            //rCombined.x = rCombined.x - Mathf.FloorToInt(rCombined.x);
            //rCombined.y = rCombined.y - Mathf.FloorToInt(rCombined.y);
            return rCombined;
        }

        //since the 0,0..1,1 box is tiled the offset should always be between 0 and 1
        public static void Canonicalize(ref DRect r, double minX, double minY)
        {
            r.x = r.x - Mathf.FloorToInt((float) r.x);
            if (r.x < minX) { r.x += Mathf.CeilToInt((float)minX); }
            r.y = r.y - Mathf.FloorToInt((float) r.y);
            if (r.y < minY) { r.y += Mathf.CeilToInt((float)minY); }
        }

        public static void Canonicalize(ref Rect r, float minX, float minY)
        {
            r.x = r.x - Mathf.FloorToInt(r.x);
            if (r.x < minX) { r.x += Mathf.CeilToInt(minX); }
            r.y = r.y - Mathf.FloorToInt(r.y);
            if (r.y < minY) { r.y += Mathf.CeilToInt(minY); }
        }

        public static DRect InverseTransform(ref DRect t)
        {
            DRect tinv = new DRect();
            tinv.x = -t.x / t.width;
            tinv.y = -t.y / t.height;
            tinv.width = 1f / t.width;
            tinv.height = 1f / t.height;
            return tinv;
        }

        public static DRect GetEncapsulatingRect(ref DRect uvRect1, ref DRect uvRect2)
        {
            double smnx = uvRect1.x;
            double smny = uvRect1.y;
            double smxx = uvRect1.x + uvRect1.width;
            double smxy = uvRect1.y + uvRect1.height;
            double bmnx = uvRect2.x;
            double bmny = uvRect2.y;
            double bmxx = uvRect2.x + uvRect2.width;
            double bmxy = uvRect2.y + uvRect2.height;
            double minx, miny, maxx, maxy;
            minx = maxx = smnx;
            miny = maxy = smny;
            if (bmnx < minx) minx = bmnx;
            if (smnx < minx) minx = smnx;
            if (bmny < miny) miny = bmny;
            if (smny < miny) miny = smny;
            if (bmxx > maxx) maxx = bmxx;
            if (smxx > maxx) maxx = smxx;
            if (bmxy > maxy) maxy = bmxy;
            if (smxy > maxy) maxy = smxy;
            DRect uvRectCombined = new DRect(minx, miny, maxx - minx, maxy - miny);
            return uvRectCombined;
        }

        /*
        public static void InvertHierarchy(ref DRect uvRect, ref DRect matRect)
        {
            matRect.x = (uvRect.x * matRect.width + matRect.x - uvRect.x) / uvRect.width;
            matRect.y = (uvRect.y * matRect.height + matRect.y - uvRect.y) / uvRect.height;
        }
        */

        public static bool RectContains(ref DRect bigRect, ref DRect smallToTestIfFits)
        {
            double smnx = smallToTestIfFits.x;
            double smny = smallToTestIfFits.y;
            double smxx = smallToTestIfFits.x + smallToTestIfFits.width;
            double smxy = smallToTestIfFits.y + smallToTestIfFits.height;
            //expand slightly to deal with rounding errors
            double bmnx = bigRect.x - 10e-3f;
            double bmny = bigRect.y - 10e-3f;
            double bmxx = bigRect.x + bigRect.width + 2f * 10e-3f;
            double bmxy = bigRect.y + bigRect.height + 2f * 10e-3f;
            //is smn in box
            /*
            string s = "";
            
            s += (bmnx <= smnx);
            s += (smnx <= bmxx);
            s += (bmnx <= smxx);
            s += String.Format("{0} {1} {2}",(smxx <= bmxx).ToString(), smxx.ToString("F5"), bmxx.ToString("F5"));
            s += String.Format("{0} {1} {2}",(bmny <= smny), bmny.ToString("F5"), smny.ToString("F5"));
            s += (smny <= bmxy);
            s += (bmny <= smxy);
            s += String.Format("{0} {1} {2}",(smxy <= bmxy).ToString(), smxy.ToString("F5"), bmxy.ToString("F5"));
            Debug.Log("==== " + bigRect + " " + smallToTestIfFits + "  " + s);
            */
            return bmnx <= smnx && smnx <= bmxx &&
                    bmnx <= smxx && smxx <= bmxx &&
                    bmny <= smny && smny <= bmxy &&
                    bmny <= smxy && smxy <= bmxy;
        }

        public static bool RectContains(ref Rect bigRect, ref Rect smallToTestIfFits)
        {
            float smnx = smallToTestIfFits.x;
            float smny = smallToTestIfFits.y;
            float smxx = smallToTestIfFits.x + smallToTestIfFits.width;
            float smxy = smallToTestIfFits.y + smallToTestIfFits.height;
            //expand slightly to deal with rounding errors
            float bmnx = bigRect.x - 10e-3f;
            float bmny = bigRect.y - 10e-3f;
            float bmxx = bigRect.x + bigRect.width + 2f * 10e-3f;
            float bmxy = bigRect.y + bigRect.height + 2f * 10e-3f;
            //is smn in box
            /*
            Debug.Log("==== " + bigRect + " " + smallToTestIfFits);
            Debug.Log(bmnx <= smnx);
            Debug.Log(smnx <= bmxx);
            Debug.Log(bmnx <= smxx);
            Debug.LogFormat("{0} {1} {2}", (smxx <= bmxx).ToString(), smxx.ToString("F5"), bmxx.ToString("F5"));
            Debug.LogFormat("{0} {1} {2}", (bmny <= smny), bmny.ToString("F5"), smny.ToString("F5"));
            Debug.Log(smny <= bmxy);
            Debug.Log(bmny <= smxy);
            Debug.LogFormat("{0} {1} {2}", (smxy <= bmxy).ToString(), smxy.ToString("F5"), bmxy.ToString("F5"));
            Debug.Log("----------------");
            */
            return bmnx <= smnx && smnx <= bmxx &&
                    bmnx <= smxx && smxx <= bmxx &&
                    bmny <= smny && smny <= bmxy &&
                    bmny <= smxy && smxy <= bmxy;
        }

        internal static Vector2 TransformPoint(ref DRect r, Vector2 p)
        {
            return new Vector2((float) (r.width * p.x + r.x),(float)(r.height * p.y + r.y));
        }
    }
}
