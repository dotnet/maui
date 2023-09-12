using System.Diagnostics;
using System.Text.RegularExpressions;
using Xamarin.UITest.Queries;

namespace TestUtils.Appium.UITests
{
	internal class AppiumQuery
	{
		public static AppiumQuery FromMarked(string marked, string appId, string platform)
		{
			var realId = marked;
			QueryPlatform queryPlatform = GetQueryPlatform(platform);
			if (queryPlatform == QueryPlatform.Android)
				realId = $"{appId}:id/{marked}";

			return new AppiumQuery("*", realId, $"* '{realId}'", appId, queryPlatform);
		}

		public static AppiumQuery FromQuery(Func<AppQuery, AppQuery>? query, string appId, string platform)
		{
			var raw = GetRawQuery(query);
			return FromRaw(raw, appId, platform);
		}

		public static AppiumQuery FromRaw(string raw, string appId, string platform)
		{
			QueryPlatform queryPlatform = GetQueryPlatform(platform);
			return FromRaw(raw, appId, queryPlatform);
		}

		public static AppiumQuery FromRaw(string raw, string appId, QueryPlatform platform)
		{
			Debug.WriteLine($">>>>> Converting raw query '{raw}' to {nameof(AppiumQuery)}");

			var match = Regex.Match(raw, @"(.*)\s(marked|text):'(.*)'");

			var controlType = match.Groups[1].Captures[0].Value;
			var marked = match.Groups[3].Captures[0].Value;

			if (platform == QueryPlatform.Android)
				marked = $"{appId}:id/{marked}";

			// Just ignoring everything else for now (parent, index statements, etc)
			var result = new AppiumQuery(controlType, marked, raw, appId, platform);

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

		internal static QueryPlatform GetQueryPlatform(string platform)
		{
			var plat = platform.ToLowerInvariant();
			QueryPlatform queryPlatform = QueryPlatform.iOS;
			if (plat.Contains("android", StringComparison.InvariantCultureIgnoreCase))
				queryPlatform = QueryPlatform.Android;
			if (plat.Contains("windows", StringComparison.InvariantCultureIgnoreCase))
				queryPlatform = (QueryPlatform)2;
			if (plat.Contains("mac", StringComparison.InvariantCultureIgnoreCase))
				queryPlatform = (QueryPlatform)2;
			return queryPlatform;
		}

		AppiumQuery(string controlType, string marked, string raw, string appId, QueryPlatform platform)
		{
			ControlType = controlType;
			Marked = marked;
			Raw = raw;
			AppId = appId;
			Platform = platform;
		}

		public string ControlType { get; }

		public string Marked { get; }

		public string Raw { get; }

		public string AppId { get; }

		public QueryPlatform Platform { get; }

		public override string ToString()
		{
			return $"{nameof(ControlType)}: {ControlType}, {nameof(Marked)}: {Marked}";
		}
	}
}