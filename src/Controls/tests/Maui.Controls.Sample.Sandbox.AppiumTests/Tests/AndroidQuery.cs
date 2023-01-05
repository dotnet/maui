using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.UITest.Queries;

namespace Maui.Controls.Sample.Sandbox.AppiumTests.Tests
{
	internal class AndroidQuery
	{
		public static AndroidQuery FromMarked(string appId, string marked)
		{
			var realId = $"{appId}/{marked}";
			return new AndroidQuery("*", realId, $"* '{realId}'");
		}

		public static AndroidQuery FromQuery(Func<AppQuery, AppQuery> query)
		{
			var raw = GetRawQuery(query);
			return FromRaw(raw);
		}


		public static AndroidQuery FromRaw(string raw)
		{
			Debug.WriteLine($">>>>> Converting raw query '{raw}' to {nameof(AndroidQuery)}");

			var match = Regex.Match(raw, @"(.*)\s(marked|text):'((.|\n)*)'");

			var controlType = match.Groups[1].Captures[0].Value;
			var marked = match.Groups[3].Captures[0].Value;

			// Just ignoring everything else for now (parent, index statements, etc)
			var result = new AndroidQuery(controlType, marked, raw);

			Debug.WriteLine($">>>>> AndroidQuery is: {result}");

			return result;
		}

		static string GetRawQuery(Func<AppQuery, AppQuery>? query = null)
		{
			if (query == null)
			{
				return string.Empty;
			}

			// When we pull out the iOS query it's got any instances of "'" escaped with "\", need to fix that up
			return query(new AppQuery(QueryPlatform.iOS)).ToString().Replace("\\'", "'", StringComparison.InvariantCultureIgnoreCase);
		}

		AndroidQuery(string controlType, string marked, string raw)
		{
			ControlType = controlType;
			Marked = marked;
			Raw = raw;
		}

		public string ControlType { get; }

		public string Marked { get; }

		public string Raw { get; }

		public override string ToString()
		{
			return $"{nameof(ControlType)}: {ControlType}, {nameof(Marked)}: {Marked}";
		}

	}
}
