using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

static class Traits
{
	public const string AppleIntelligence = "AppleIntelligence";

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
			yield return "Category=AppleIntelligenceChatClient";
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
