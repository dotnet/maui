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
	internal class AppiumQuery
	{
		public static AppiumQuery FromMarked(string appId, string marked, bool isAndroid)
		{
			var realId = marked;
			if (isAndroid)
				realId = $"{appId}:id/{marked}";
			return new AppiumQuery("*", realId, $"* '{realId}'", isAndroid ? QueryPlatform.Android : QueryPlatform.iOS);
		}

		public static AppiumQuery FromQuery(Func<AppQuery, AppQuery> query, bool isAndroid)
		{
			var raw = GetRawQuery(query);
			return FromRaw(raw, isAndroid);
		}


		public static AppiumQuery FromRaw(string raw, bool isAndroid)
		{
			Debug.WriteLine($">>>>> Converting raw query '{raw}' to {nameof(AppiumQuery)}");

			var match = Regex.Match(raw, @"(.*)\s(marked|text):'((.|\n)*)'");

			var controlType = match.Groups[1].Captures[0].Value;
			var marked = match.Groups[3].Captures[0].Value;

			// Just ignoring everything else for now (parent, index statements, etc)
			var result = new AppiumQuery(controlType, marked, raw, isAndroid ? QueryPlatform.Android : QueryPlatform.iOS);

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

		AppiumQuery(string controlType, string marked, string raw, QueryPlatform platform)
		{
			ControlType = controlType;
			Marked = marked;
			Raw = raw;
			Platform = platform;
		}

		public string ControlType { get; }

		public string Marked { get; }

		public string Raw { get; }

		public QueryPlatform Platform { get; }

		public override string ToString()
		{
			return $"{nameof(ControlType)}: {ControlType}, {nameof(Marked)}: {Marked}";
		}

	}
}
