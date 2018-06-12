using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DigitalOpus.MB.Core
{
    /**
    A TextureBlender will attempt to blend non-texture properties with textures so that the result material looks the same as source material.
    */
    public interface TextureBlender
    {
        /** 
        The shader name that must be matched on the result material in order for this TextureBlender to be used. This should return something like "Legacy/Bumped Difuse"
        */
        bool DoesShaderNameMatch(string shaderName);

        /**
        This is called to prepare the TextureBlender before any calls to OnBlendTexturePixel
        Use this to grab the non-texture property values from the material that will be used to alter the Pixel color in the texture.
        Note that the sourceMat may not use a shader matching ShaderName. It may not have expected properties. Check that properties exist
        before grabing them.
        */
        void OnBeforeTintTexture(Material sourceMat, string shaderTexturePropertyName);

        /**
        Called once for each pixel in the texture to alter the pixel color. For efficiency don't check shaderPropertyName every call. Instead use OnBeforeTintTexture
        to prepare this textrure blender for a batch of OnBlendTexturePixel calls.
        */
        Color OnBlendTexturePixel(string shaderPropertyName, Color pixelColor);

        /**
        Material a & b may have the same set of textures but different non-texture properties (colorTint etc...)
        If so then they need to be put into separate rectangels in the atlas. This method should check the non-texture properties 
        and return false if they are different. Note that material a and b may use a different shader than GetShaderName so your code
        should handle the case where properties do not exist.
        */
        bool NonTexturePropertiesAreEqual(Material a, Material b);

        /**
        Sets the non texture properties on the result materail after textures have been baked. If for example _Color has been blended with 
        the _Albedo textures then the _Color property on the result material should probably be set to white.
        */
        void SetNonTexturePropertyValuesOnResultMaterial(Material resultMaterial);

        /**
        Some textures may not be assigned for a material. This method should return a color that will used to create a small solid color texture
        to be used in these cases.
        */
        Color GetColorIfNoTexture(Material m, ShaderTextureProperty texPropertyName);
    }
}
