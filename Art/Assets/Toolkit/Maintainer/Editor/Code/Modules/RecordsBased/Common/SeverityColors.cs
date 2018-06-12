#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer
{
	public class SeverityColors
	{
		private static Dictionary<RecordSeverity, Color32> severityColorsDarkSkin;
		private static Dictionary<RecordSeverity, Color32> severityColorsLightSkin;

		public static Color32 GetColor(RecordSeverity severity)
		{
			Init();
			return EditorGUIUtility.isProSkin ? severityColorsDarkSkin[severity] : severityColorsLightSkin[severity];
		}

		public static string GetHtmlColor(RecordSeverity severity)
		{
			Init();
			Color32 color32 = GetColor(severity);
			return color32.r.ToString("x2") + color32.g.ToString("x2") + color32.b.ToString("x2") + color32.a.ToString("x2");
		}

		private static void Init()
		{
			if (severityColorsDarkSkin == null)
			{
				severityColorsDarkSkin = new Dictionary<RecordSeverity, Color32>
				{
					{RecordSeverity.Info, new Color32(122, 213, 255, 255)},
					{RecordSeverity.Warning, new Color32(255, 255, 90, 255)},
					{RecordSeverity.Error, new Color32(255, 90, 90, 255)}
				};

				severityColorsLightSkin = new Dictionary<RecordSeverity, Color32>
				{
					{RecordSeverity.Info, new Color32(25, 170, 237, 255)},
					{RecordSeverity.Warning, new Color32(255, 150, 0, 255)},
					{RecordSeverity.Error, new Color32(255, 50, 50, 255)}
				};
			}
		}
	}
}

#endif