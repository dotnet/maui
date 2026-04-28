using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

static class Traits
{
	public const string AppleIntelligenceChatClient = "AppleIntelligenceChatClient";
	public const string NLEmbeddingGenerator = "NLEmbeddingGenerator";
	public const string OpenAIChatClient = "OpenAIChatClient";
	public const string OpenAIEmbeddingGenerator = "OpenAIEmbeddingGenerator";

	internal static class FeatureSupport
	{
		public const string Supported = "Supported";
		public const string NotSupported = "NotSupported";

		internal static string ToExclude(bool hasFeature) =>
			hasFeature ? NotSupported : Supported;
	}

	internal static IEnumerable<string> GetSkipTraits(IEnumerable<string>? additionalFilters = null)
	{
#if IOS || MACCATALYST
		// Apple Intelligence (FoundationModels) requires iOS/MacCatalyst 26+
		if (!OperatingSystem.IsIOSVersionAtLeast(26) && !OperatingSystem.IsMacCatalystVersionAtLeast(26))
		{
			yield return $"Category={AppleIntelligenceChatClient}";
		}

		// Read TestFilter from environment (set by Helix/XHarness via --set-env)
		// Supports TestFilter=SkipCategories=X,Y,Z format
		string? testFilter = null;
		foreach (var en in Foundation.NSProcessInfo.ProcessInfo.Environment)
		{
			if ($"{en.Key}" == "TestFilter")
			{
				testFilter = $"{en.Value}";
				break;
			}
		}

		if (!string.IsNullOrEmpty(testFilter) && testFilter.StartsWith("SkipCategories=", StringComparison.Ordinal))
		{
			var parts = testFilter.Substring("SkipCategories=".Length)
				.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var part in parts)
			{
				var cat = part.Trim();
				if (!string.IsNullOrWhiteSpace(cat))
					yield return $"Category={cat}";
			}
		}
#endif

		if (additionalFilters != null)
		{
			foreach (var filter in additionalFilters)
			{
				yield return filter;
			}
		}
	}
}
