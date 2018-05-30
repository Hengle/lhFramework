using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;
namespace Framework.Infrastructure
{
    // A behaviour that is attached to a playable
    [Serializable]
    public class TimelineEventBehaviour : PlayableBehaviour
    {
        public ITimeline control;
        private bool m_isPlay;
        [Serializable]
        public class KeyValue
        {
            public ETimelineEventType key;
            public string value;
        }
        public List<KeyValue> graphStartEvents = new List<KeyValue>();
        public List<KeyValue> graphEndEvents = new List<KeyValue>();
        public List<KeyValue> behaviorStartEvents = new List<KeyValue>();
        public List<KeyValue> behaviorEndEvents = new List<KeyValue>();
        public List<KeyValue> behaviorUpdateEvents = new List<KeyValue>();
        // Called when the owning graph starts playing
        public override void OnGraphStart(Playable playable)
        {
            if (Application.isPlaying)
            {
                if (control!=null)
                {
                    for (int i = 0; i < graphStartEvents.Count; i++)
                    {
                        control.Dispatch(ETimelineCycleType.GraphStart,graphStartEvents[i].key, graphStartEvents[i].value);
                    }
                }
            }
        }

        // Called when the owning graph stops playing
        public override void OnGraphStop(Playable playable)
        {
            if (Application.isPlaying)
            {
                if (control!=null)
                {
                    for (int i = 0; i < graphEndEvents.Count; i++)
                    {
                        control.Dispatch(ETimelineCycleType.GraphEnd, graphEndEvents[i].key, graphEndEvents[i].value);
                    }
                }
            }
        }

        // Called when the state of the playable is set to Play
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Application.isPlaying)
            {
                if (control != null)
                {
                    for (int i = 0; i < behaviorStartEvents.Count; i++)
                    {
                        control.Dispatch(ETimelineCycleType.BehaviorStart, behaviorStartEvents[i].key, behaviorStartEvents[i].value);
                    }
                }
                m_isPlay = true;
            }
        }

        // Called when the state of the playable is set to Paused
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (Application.isPlaying)
            {
                if (m_isPlay)
                {
                    if (control != null)
                    {
                        for (int i = 0; i < behaviorEndEvents.Count; i++)
                        {
                            control.Dispatch(ETimelineCycleType.BehaviorEnd, behaviorEndEvents[i].key, behaviorEndEvents[i].value);
                        }
                    }
                }
                m_isPlay = false;
            }
        }

        // Called each frame while the state is set to Play
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < behaviorUpdateEvents.Count; i++)
                {
                    control.Dispatch(ETimelineCycleType.BehaviorUpdate,behaviorUpdateEvents[i].key, behaviorUpdateEvents[i].value);
                }
            }
        }
    }
}