#if UNITY_EDITOR

using System;
using System.Text;
using UnityEditor;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public class BuildSettingsIssueRecord : IssueRecord, IShowableRecord
	{
		public void Show()
		{
			EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
		}

		internal static BuildSettingsIssueRecord Create(RecordType type, string body)
		{
            return new BuildSettingsIssueRecord(type, body);
		}

		protected BuildSettingsIssueRecord(RecordType type, string body):base(type, RecordLocation.BuildSettings)
		{
			bodyExtra = body;
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Build Settings</b> issue");
		}
	}
}

#endif