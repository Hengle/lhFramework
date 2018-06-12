#if UNITY_EDITOR

using System;
using System.Text;

namespace CodeStage.Maintainer
{
	[Serializable]
	public abstract class RecordBase
	{
		public RecordSeverity severity;
		public RecordLocation location;

		public string headerExtra;
		public string bodyExtra;
		public string headerFormatArgument;

		protected StringBuilder cachedHeader;
		protected StringBuilder cachedBody;

		public string GetHeader()
		{
			if (cachedHeader != null) return cachedHeader.ToString();

			cachedHeader = new StringBuilder();
			cachedHeader.Append("<b><size=14>");

			ConstructHeader(cachedHeader);

			if (!string.IsNullOrEmpty(headerExtra))
			{
				cachedHeader.Append(' ').Append(headerExtra);
			}

			cachedHeader.Append("</size></b>");

			return cachedHeader.ToString();
		}

		public string GetBody()
		{
			if (cachedBody != null) return cachedBody.ToString();

			cachedBody = new StringBuilder();

			ConstructBody(cachedBody);

			if (!string.IsNullOrEmpty(bodyExtra))
			{
				cachedBody.Append("\n").Append(bodyExtra);
			}

			return cachedBody.ToString();
		}

		public override string ToString()
		{
			return GetHeader() + "\n" + GetBody();
		}

		public string ToString(bool clearHtml)
		{
			return StripTagsCharArray(ToString());
		}

		protected abstract void ConstructHeader(StringBuilder cachedHeader);
        protected abstract void ConstructBody(StringBuilder text);

		// source: http://www.dotnetperls.com/remove-html-tags
		private static string StripTagsCharArray(string input)
		{
			int arrayIndex = 0;
			bool inside = false;
			int len = input.Length;

			char[] array = new char[len];

			for (int i = 0; i < len; i++)
			{
				char let = input[i];

				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}

				if (inside) continue;

				array[arrayIndex] = @let;
				arrayIndex++;
			}
			return new string(array, 0, arrayIndex);
		}
	}
}

#endif