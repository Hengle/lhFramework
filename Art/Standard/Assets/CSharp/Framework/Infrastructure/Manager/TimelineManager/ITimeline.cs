using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace lhFramework.Infrastructure.Managers
{
    public interface ITimeline
    {
        Transform followTrans { get; set; }
        Transform lookAtTrans { get; set; }
        GameObject gameObject { get; set; }
        int index { get; set; }
        bool canBreak { get; set; }
        PlayableDirector director { get; set; }
        void Create();
        void Dispatch(ETimelineCycleType cycleType, ETimelineEventType type, string str);
        void AddEvent(ETimelineCycleType type, TimelineEventHandler handler);
        void RemoveEvent(ETimelineCycleType type, TimelineEventHandler handler);
        void Bind(Transform trans, ETimelineBindType bindType,Vector3 offset,bool canBreak, TimelineHandler onTimelineOver);
        void ForceFree();
    }
}