using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{ 
    public static class StaticExternal
    {
        public static int GetClipLength(this Animator animator,string name)
        {
            if (animator == null || string.IsNullOrEmpty(name) || animator.runtimeAnimatorController == null) return 0;
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            AnimationClip[] clips = ac.animationClips;
            if (clips == null || clips.Length <= 0) return 0;
            AnimationClip clip=null;
            for (int i = 0,l=clips.Length; i < l; i++)
            {
                clip = ac.animationClips[i];
                if (clip != null && clip.name == name)
                    return (int)(clip.length*1000);
            }
            return 0;
        }
    }
}
