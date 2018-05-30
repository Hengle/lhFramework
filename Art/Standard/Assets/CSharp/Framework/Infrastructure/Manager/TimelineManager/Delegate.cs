using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
    public delegate void TimelineEventHandler(ETimelineEventType type, string value);
    public delegate void TimelineHandler();
}