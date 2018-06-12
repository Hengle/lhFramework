#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class AboutTab
	{
		private const string LOGO_DARK_NAME = "LogoDark.png";
		private const string LOGO_LIGHT_NAME = "LogoLight.png";
		private const string UAS_LINK = "https://www.assetstore.unity3d.com/#!/content/32199";
		private const string UAS_PROFILE_LINK = "https://www.assetstore.unity3d.com/#!/search/page=1/sortby=popularity/query=publisher:3918";
		private const string HOMEPAGE = "http://blog.codestage.ru/unity-plugins/maintainer/";
		private const string SUPPORT_LINK = "http://blog.codestage.ru/contacts/";
		private const string CHANGELOG_LINK = "http://codestage.ru/unity/maintainer/changelog.txt";

		private readonly Dictionary<string, Texture2D> cachedLogos = new Dictionary<string, Texture2D>();

		internal void Draw(MaintainerWindow parentWindow)
		{
			using (UIHelpers.Horizontal())
			{
				/* logo */

				using (UIHelpers.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
				{
					GUILayout.FlexibleSpace();

					using (UIHelpers.Horizontal())
					{
						GUILayout.FlexibleSpace();

						Texture2D logo = GetLogoTexture(EditorGUIUtility.isProSkin ? LOGO_DARK_NAME : LOGO_LIGHT_NAME);
						if (logo != null)
						{
							Rect logoRect = EditorGUILayout.GetControlRect(GUILayout.Width(logo.width), GUILayout.Height(logo.height));
							GUI.DrawTexture(logoRect, logo);
							GUILayout.Space(5);
						}

						GUILayout.FlexibleSpace();
					}

					GUILayout.FlexibleSpace();
				}

				/* buttons and stuff */

				using (UIHelpers.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
				{
					GUILayout.Space(10);
					GUILayout.Label("<size=18>Maintainer v.<b>" + Maintainer.VERSION + "</b></size>", UIHelpers.centeredLabel);
					GUILayout.Space(5);
					GUILayout.Label("Developed by Dmitriy Yukhanov\n" +
					                "Logo by Daniele Giardini", UIHelpers.centeredLabel);
					GUILayout.Space(10);
					UIHelpers.Separator();
					GUILayout.Space(5);
					if (GUILayout.Button("Homepage"))
					{
						Application.OpenURL(HOMEPAGE);
					}
					GUILayout.Space(5);
					if (GUILayout.Button("Support contacts"))
					{
						Application.OpenURL(SUPPORT_LINK);
					}
					GUILayout.Space(5);
					if (GUILayout.Button("Full changelog (online)"))
					{
						Application.OpenURL(CHANGELOG_LINK);
					}
					GUILayout.Space(5);

					//GUILayout.Space(10);
					//GUILayout.Label("Asset Store links", UIHelpers.centeredLabel);
					UIHelpers.Separator();
					GUILayout.Space(5);
					if (GUILayout.Button(new GUIContent("Plugin at Unity Asset Store")))
					{
						Application.OpenURL(UAS_LINK);
					}
					GUILayout.Label("It's really important to know your opinion,\n rates & reviews are <b>greatly appreciated!</b>", UIHelpers.centeredLabel);
					GUILayout.Space(5);
					if (GUILayout.Button("My profile at Unity Asset Store"))
					{
						Application.OpenURL(UAS_PROFILE_LINK);
					}
					GUILayout.Label("Check all my plugins!", UIHelpers.centeredLabel);
				}
			}
		}

		private Texture2D GetLogoTexture(string fileName)
		{
			Texture2D texture;
			if (cachedLogos.ContainsKey(fileName))
			{
				texture = cachedLogos[fileName];
			}
			else
			{
				texture = AssetDatabase.LoadAssetAtPath(Maintainer.Directory + "/Images/" + fileName, typeof(Texture2D)) as Texture2D;
				if (texture == null)
				{
					Debug.LogError(Maintainer.LOG_PREFIX + "Some error occurred while looking for logo image!");
				}
				else
				{
					cachedLogos[fileName] = texture;
				}
			}
			return texture;
		}
	}
}

#endif