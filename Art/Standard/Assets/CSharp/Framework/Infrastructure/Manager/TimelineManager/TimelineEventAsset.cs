using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework.Infrastructure
{
    [System.Serializable]
    public class TimelineEventAsset : PlayableAsset, IPropertyPreview
    {
        public ExposedReference<TimelineControl> controlRefer;
        public TimelineEventBehaviour behavior;
        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<TimelineEventBehaviour>.Create(graph, behavior);
            behavior = playable.GetBehaviour();
            behavior.control = controlRefer.Resolve(graph.GetResolver());
            return playable;
        }
        void IPropertyPreview.GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            driver.AddFromName<Transform>("m_LocalPosition.x");
            driver.AddFromName<Transform>("m_LocalPosition.y");
            driver.AddFromName<Transform>("m_LocalPosition.z");
        }
    }
}