using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DigitalOpus.MB.Core
{
    public class TextureBlenderStandardSpecular : TextureBlender
    {
        private enum Prop
        {
            doColor,
            doSpecular,
            doEmission,
            doNone,
        }

        Color m_tintColor;
        //float m_smoothness;
        //Color m_specular;
        Color m_emission;
        Prop propertyToDo = Prop.doNone; //this just makes things more efficient so we arn't doing a string comparison for each pixel.

        Color m_defaultColor = Color.white;
        Color m_defaultSpecular = Color.black;
        float m_defaultGlossiness = .5f;
        Color m_defaultEmission = Color.black;

		public bool DoesShaderNameMatch(string shaderName)
		{
			return shaderName.Equals("Standard (Specular setup)");
		}

        public void OnBeforeTintTexture(Material sourceMat, string shaderTexturePropertyName)
        {
            if (shaderTexturePropertyName.Equals("_MainTex"))
            {
                propertyToDo = Prop.doColor;
                if (sourceMat.HasProperty(shaderTexturePropertyName))
                {
                    m_tintColor = sourceMat.GetColor("_Color");
                }
                else
                {
                    m_tintColor = m_defaultColor;
                }
            }
            else if (shaderTexturePropertyName.Equals("_MetallicGlossMap"))
            {
                propertyToDo = Prop.doSpecular;
				/*
                if (sourceMat.HasProperty(shaderTexturePropertyName))
                {
                    m_smoothness = sourceMat.GetFloat("_Glossiness");
                    m_specular = sourceMat.GetColor("_SpecColor");
                }
                else
                {
                    m_smoothness = m_defaultGlossiness;
                    m_specular = m_defaultSpecular;
                }
				*/
            }


            else if (shaderTexturePropertyName.Equals("_EmissionMap"))
            {
                propertyToDo = Prop.doEmission;
                if (sourceMat.HasProperty(shaderTexturePropertyName))
                {
                    m_emission = sourceMat.GetColor("_EmissionColor");
                }
                else
                {
                    m_emission = m_defaultEmission;
                }

            }
            else
            {
                propertyToDo = Prop.doNone;
            }
        }


        public Color OnBlendTexturePixel(string propertyToDoshaderPropertyName, Color pixelColor)
        {
            if (propertyToDo == Prop.doColor)
            {
                return new Color(pixelColor.r * m_tintColor.r, pixelColor.g * m_tintColor.g, pixelColor.b * m_tintColor.b, pixelColor.a * m_tintColor.a);
            }
            else if (propertyToDo == Prop.doSpecular)
            {
                return pixelColor;
            }
            else if (propertyToDo == Prop.doEmission)
            {
                return new Color(pixelColor.r * m_emission.r, pixelColor.g * m_emission.g, pixelColor.b * m_emission.b, pixelColor.a * m_emission.a);
            }
            return pixelColor;
        }

        public bool NonTexturePropertiesAreEqual(Material a, Material b)
        {
            if (!TextureBlenderFallback._compareColor(a, b, m_defaultColor, "_Color"))
            {
                return false;
            }

            if (!TextureBlenderFallback._compareColor(a, b, m_defaultSpecular, "_SpecColor"))
            {
                return false;
            }

            if (!TextureBlenderFallback._compareFloat(a, b, m_defaultGlossiness, "_Glossiness"))
            {
                return false;
            }
            if (!TextureBlenderFallback._compareColor(a, b, m_defaultEmission, "_EmissionColor"))
            {
                return false;
            }
            return true;
        }

        public void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial)
        {
            resultMaterial.SetColor("_Color", m_defaultColor);
            resultMaterial.SetColor("_SpecColor", m_defaultSpecular);
            resultMaterial.SetFloat("_Glossiness", m_defaultGlossiness);
            if (resultMaterial.GetTexture("_EmissionMap") == null)
            {
                resultMaterial.SetColor("_EmissionColor", Color.black);
            }
            else {
                resultMaterial.SetColor("_EmissionColor", Color.white);
            }
        }


        public Color GetColorIfNoTexture(Material mat, ShaderTextureProperty texPropertyName)
        {
            if (texPropertyName.name.Equals("_BumpMap"))
            {
                return new Color(.5f, .5f, 1f);
            }
            else if (texPropertyName.name.Equals("_MainTex"))
            {
                if (mat != null && mat.HasProperty("_Color"))
                {
                    try
                    { //need try because can't garantee _Color is a color
                        return mat.GetColor("_Color");
                    }
                    catch (Exception) { }
                }
            }
            else if (texPropertyName.name.Equals("_SpecGlossMap"))
            {
                bool success = false;
                if (mat != null && mat.HasProperty("_SpecColor"))
                {
                    try
                    { //need try because can't garantee _Color is a color
                        Color c = mat.GetColor("_SpecColor");
                        if (mat.HasProperty("_Glossiness"))
                        {
                            try
                            {
                                success = true;
                                c.a = mat.GetFloat("_Glossiness");
                            }
                            catch (Exception) { }
                        }
                        Debug.LogWarning(c);
                        return c;
                    }
                    catch (Exception) { }
                }
                if (!success)
                {
                    return m_defaultSpecular;
                }
            }
            else if (texPropertyName.name.Equals("_ParallaxMap"))
            {
                return new Color(0f, 0f, 0f, 0f);
            }
            else if (texPropertyName.name.Equals("_OcclusionMap"))
            {
                return new Color(1f, 1f, 1f, 1f);
            }
            else if (texPropertyName.name.Equals("_EmissionMap"))
            {
                if (mat != null)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        try
                        {
                            return mat.GetColor("_EmissionColor");
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        return Color.black;
                    }
                }
            }
            else if (texPropertyName.name.Equals("_DetailMask"))
            {
                return new Color(0f, 0f, 0f, 0f);
            }
            return new Color(1f, 1f, 1f, 0f);
        }

    }
}
