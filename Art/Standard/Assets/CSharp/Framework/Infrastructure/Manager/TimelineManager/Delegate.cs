using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    public delegate void TimelineEventHandler(ETimelineEventType type, string value);
    public delegate void TimelineHandler();
}