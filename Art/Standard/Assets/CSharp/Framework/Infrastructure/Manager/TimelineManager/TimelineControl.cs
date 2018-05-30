using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework.Infrastructure
{
    public class TimelineControl : MonoBehaviour,ITimeline
    {
        public PlayableDirector playableDirector;
        public Transform follow;
        public Transform lookAt;

        int ITimeline.index { get; set; }
        bool ITimeline.canBreak { get; set; }
        Transform ITimeline.followTrans
        {
            get
            {
                return follow;
            }
            set { }
        }
        Transform ITimeline.lookAtTrans
        {
            get
            {
                return lookAt;
            }
            set { }
        }
        PlayableDirector ITimeline.director
        {
            get
            {
                return playableDirector;
            }
            set { }
        }
        GameObject ITimeline.gameObject
        {
            get
            {
                return this.gameObject;
            }
            set { }
        }

        private event TimelineEventHandler graphStartHandler;
        private event TimelineEventHandler graphEndHandler;
        private event TimelineEventHandler behaviorStartHandler;
        private event TimelineEventHandler behaviorEndHandler;
        private event TimelineEventHandler behaviorUpdateHandler;
        private TimelineHandler m_timelineOverHandler;
        void OnEnable()
        {
            graphStartHandler = null;
            graphEndHandler = null;
            behaviorStartHandler = null;
            behaviorEndHandler = null;
            behaviorUpdateHandler = null;
        }
        void OnDisable()
        {
            graphStartHandler = null;
            graphEndHandler = null;
            behaviorStartHandler = null;
            behaviorEndHandler = null;
            behaviorUpdateHandler = null;
            m_timelineOverHandler = null;
        }
        void Update()
        {
            if (playableDirector!=null && playableDirector.state==PlayState.Paused)
            {
                if (m_timelineOverHandler != null)
                    m_timelineOverHandler();
                TimelineManager.Free(this);
                m_timelineOverHandler = null;
            }
        }
        void ITimeline.Dispatch(ETimelineCycleType cycleType, ETimelineEventType type, string str)
        {
            switch (cycleType)
            {
                case ETimelineCycleType.GraphStart:
                    if (graphStartHandler != null)
                    {
                        graphStartHandler(type, str);
                    }
                    break;
                case ETimelineCycleType.GraphEnd:
                    if (graphEndHandler != null)
                    {
                        graphEndHandler(type, str);
                    }
                    break;
                case ETimelineCycleType.BehaviorStart:
                    if (behaviorStartHandler != null)
                    {
                        behaviorStartHandler(type, str);
                    }
                    break;
                case ETimelineCycleType.BehaviorEnd:
                    if (behaviorEndHandler != null)
                    {
                        behaviorEndHandler(type, str);
                    }
                    break;
                case ETimelineCycleType.BehaviorUpdate:
                    if (behaviorUpdateHandler != null)
                    {
                        behaviorUpdateHandler(type, str);
                    }
                    break;
                default:
                    break;
            }
        }
        void ITimeline.Create()
        {
            if (playableDirector==null)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }
        }
        void ITimeline.AddEvent(ETimelineCycleType type, TimelineEventHandler handler)
        {
            switch (type)
            {
                case ETimelineCycleType.GraphStart:
                    graphStartHandler += handler;
                    break;
                case ETimelineCycleType.GraphEnd:
                    graphEndHandler += handler;
                    break;
                case ETimelineCycleType.BehaviorStart:
                    behaviorStartHandler += handler;
                    break;
                case ETimelineCycleType.BehaviorEnd:
                    behaviorEndHandler += handler;
                    break;
                case ETimelineCycleType.BehaviorUpdate:
                    behaviorUpdateHandler += handler;
                    break;
                default:
                    break;
            }
        }
        void ITimeline.RemoveEvent(ETimelineCycleType type, TimelineEventHandler handler)
        {
            switch (type)
            {
                case ETimelineCycleType.GraphStart:
                    graphStartHandler -= handler;
                    break;
                case ETimelineCycleType.GraphEnd:
                    graphEndHandler -= handler;
                    break;
                case ETimelineCycleType.BehaviorStart:
                    behaviorStartHandler -= handler;
                    break;
                case ETimelineCycleType.BehaviorEnd:
                    behaviorEndHandler -= handler;
                    break;
                case ETimelineCycleType.BehaviorUpdate:
                    behaviorUpdateHandler -= handler;
                    break;
                default:
                    break;
            }
        }
        void ITimeline.Bind(Transform trans,ETimelineBindType bindType, Vector3 offset, bool canBreak,TimelineHandler onTimelineOver)
        {
            if (playableDirector!=null)
            {
                foreach (var item in playableDirector.playableAsset.outputs)
                {
                    if (item.streamName== "Cinemachine")
                    {
                        playableDirector.SetGenericBinding(item.sourceObject, CameraManager.cinemachineBrain);
                        break;
                    }
                }
            }
            if (bindType == ETimelineBindType.Local)
            {
                this.transform.SetParent(trans);
                this.transform.localPosition = Vector3.zero + offset;
            }
            else if (bindType == ETimelineBindType.Origin)
            {
                this.transform.SetParent(null);
                this.transform.position =  offset;
            }
            else
            {
                this.transform.SetParent(null);
                if (trans != null)
                {
                    this.transform.position = trans.position + offset;
                }
            }
            ((ITimeline)this).canBreak = canBreak;
            m_timelineOverHandler = onTimelineOver;
        }
        void ITimeline.ForceFree()
        {
            if (enabled)
            {
                if (m_timelineOverHandler != null)
                    m_timelineOverHandler();
                TimelineManager.Free(this);
                m_timelineOverHandler = null;
            }
        }
    }
}