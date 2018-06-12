#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal struct UIHelpers : IDisposable
	{
		// ----------------------------------------------------------------------------
		// static tooling
		// ----------------------------------------------------------------------------

		public static GUIStyle richLabel;
		public static GUIStyle compactButton;
		public static GUIStyle recordButton;
		public static GUIStyle richWordWrapLabel;
		public static GUIStyle hyperlinkLabel;
		public static GUIStyle richFoldout;
		public static GUIStyle centeredLabel;
		public static GUIStyle line;
		public static GUIStyle panelWithBackground;

		public static void SetupStyles()
		{
			if (richLabel != null) return;

			richLabel = new GUIStyle(GUI.skin.label);
			richLabel.richText = true;

			compactButton = new GUIStyle(GUI.skin.button);
			compactButton.margin = richLabel.margin;
			compactButton.overflow = richLabel.overflow;
			compactButton.padding = new RectOffset(5, 5, 1, 4);
			compactButton.margin = new RectOffset(2,2,3,2);
			compactButton.richText = true;

			recordButton = new GUIStyle(compactButton);
			recordButton.fixedWidth = 70;

			richWordWrapLabel = new GUIStyle(richLabel);
			richWordWrapLabel.wordWrap = true;

			/*hyperlinkLabel = new GUIStyle(GUI.skin.label);
				hyperlinkLabel.richText = true;
				hyperlinkLabel.hover.textColor = Color.blue;*/

			richFoldout = new GUIStyle(EditorStyles.foldout);
			richFoldout.active = richFoldout.focused = richFoldout.normal;
			richFoldout.onActive = richFoldout.onFocused = richFoldout.onNormal;
			richFoldout.richText = true;

			centeredLabel = new GUIStyle(richLabel);
			centeredLabel.alignment = TextAnchor.MiddleCenter;

			line = new GUIStyle(GUI.skin.box);
			line.border.top = line.border.bottom = 1;
			line.margin.top = line.margin.bottom = 1;
			line.padding.top = line.padding.bottom = 1;

			panelWithBackground = new GUIStyle(GUI.skin.box);
			panelWithBackground.padding = new RectOffset();
		}

		public static void Separator()
		{
			GUILayout.Box(GUIContent.none, line, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
		}

		public static void VerticalSeparator()
		{
			GUILayout.Box(GUIContent.none, line, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
		}

		public static void Indent(int level = 5, int topPadding = 2)
		{
			GUILayout.Space(topPadding);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(level * 4);
			EditorGUILayout.BeginVertical();
		}

		public static void UnIndent(int bottomPadding = 5)
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(bottomPadding);
		}

		public static bool ToggleFoldout(ref bool toggle, ref bool foldout, GUIContent caption)
		{
			GUILayout.BeginHorizontal();
			toggle = EditorGUILayout.ToggleLeft("", toggle, GUILayout.Width(12));
			foldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), foldout, caption, true, richFoldout);
			GUILayout.EndHorizontal();

			return toggle;
		}

		public static void DrawPrefabIcon()
		{
			Texture icon = EditorGUIUtility.FindTexture("PrefabNormal Icon");
			Rect iconArea = EditorGUILayout.GetControlRect(GUILayout.Width(20), GUILayout.Height(20));
			Rect iconRect = new Rect(iconArea);
			iconRect.width = iconRect.height = 20;

			//iconRect.x -= 5;
			//iconRect.y -= 5;
			GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleAndCrop);
		}

		// ----------------------------------------------------------------------------
		// tooling for "using" keyword
		// ----------------------------------------------------------------------------

		private readonly LayoutMode mode;

		public static UIHelpers Horizontal(params GUILayoutOption[] options)
		{
			return Horizontal(GUIStyle.none, options);
		}

		public static UIHelpers Horizontal(GUIStyle style, params GUILayoutOption[] options)
		{
			return new UIHelpers(LayoutMode.Horizontal, style, options);
		}

		public static UIHelpers Vertical(params GUILayoutOption[] options)
		{
			return Vertical(GUIStyle.none, options);
		}

		public static UIHelpers Vertical(GUIStyle style, params GUILayoutOption[] options)
		{
			return new UIHelpers(LayoutMode.Vertical, style, options);
		}

		private UIHelpers(LayoutMode layoutMode, GUIStyle style, params GUILayoutOption[] options)
		{
			mode = layoutMode;

			if (mode == LayoutMode.Horizontal)
			{
				GUILayout.BeginHorizontal(style, options);
			}
			else
			{
				GUILayout.BeginVertical(style, options);
			}
		}

		public void Dispose()
		{
			if (mode == LayoutMode.Horizontal)
			{
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.EndVertical();
			}
		}

		private enum LayoutMode : byte
		{
			Horizontal,
			Vertical
		}
	}
}

#endif