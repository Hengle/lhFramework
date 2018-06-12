#if UNITY_EDITOR

using UnityEngine;

namespace CodeStage.Maintainer.UI.Ignores
{
	internal abstract class TabBase
	{
		internal string name = "Untitled tab";

		internal Event currentEvent;
		internal EventType currentEventType;

		protected IgnoresWindow window;

		protected Vector2 ignoresScrollPosition;

		internal virtual void Show(IgnoresWindow hostingWindow)
		{
			window = hostingWindow;
            ignoresScrollPosition = Vector2.zero;
		}

		internal void Draw()
		{
			using (UIHelpers.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
			{
				GUILayout.Space(5);
				DrawTabContents();
			}
		}

		internal abstract void DrawTabContents();

		internal abstract void ProcessDrags();
	}
}

#endif