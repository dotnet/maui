using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Validates that the AllowedDomains property is consistent with WKAppBoundDomains in Info.plist on iOS/MacCatalyst.
	/// </summary>
	internal static class WebViewPlistValidator
	{
		/// <summary>
		/// Reads the WKAppBoundDomains array from the app's Info.plist.
		/// </summary>
		internal static IReadOnlyList<string> ReadWKAppBoundDomainsFromPlist()
		{
			var domains = new List<string>();
			var key = new NSString("WKAppBoundDomains");

			if (NSBundle.MainBundle.InfoDictionary is not null &&
				NSBundle.MainBundle.InfoDictionary.ContainsKey(key) &&
				NSBundle.MainBundle.InfoDictionary[key] is NSArray array)
			{
				for (nuint i = 0; i < array.Count; i++)
				{
					if (array.GetItem<NSString>(i) is NSString domain)
						domains.Add(domain.ToString());
				}
			}

			return domains;
		}

		/// <summary>
		/// Validates that AllowedDomains is consistent with WKAppBoundDomains in Info.plist.
		/// Logs warnings for mismatches so developers can fix their configuration.
		/// </summary>
		internal static void ValidateAllowedDomainsAgainstPlist(IList<string>? allowedDomains, ILogger? logger)
		{
			if (allowedDomains is null || allowedDomains.Count == 0)
				return;

			var plistDomains = ReadWKAppBoundDomainsFromPlist();

			var allowedSet = new HashSet<string>(allowedDomains, StringComparer.OrdinalIgnoreCase);
			var plistSet = new HashSet<string>(plistDomains, StringComparer.OrdinalIgnoreCase);

			if (plistDomains.Count == 0)
			{
				logger?.LogWarning(
					"AllowedDomains is set but WKAppBoundDomains is not declared in Info.plist. " +
					"Navigation-level blocking will work, but sub-resource blocking (images, scripts, CSS) " +
					"requires WKAppBoundDomains in Info.plist. Add the following to your Info.plist: " +
					"<key>WKAppBoundDomains</key><array>{Domains}</array>",
					string.Join("", allowedDomains.Select(d => $"<string>{d}</string>")));
				return;
			}

			if (plistDomains.Count > 10)
			{
				logger?.LogWarning(
					"WKAppBoundDomains contains {Count} domains but Apple enforces a maximum of 10. " +
					"Only the first 10 will be enforced by the OS.",
					plistDomains.Count);
			}

			var missingFromPlist = allowedSet.Except(plistSet).ToList();
			var extraInPlist = plistSet.Except(allowedSet).ToList();

			if (missingFromPlist.Count > 0)
			{
				logger?.LogWarning(
					"AllowedDomains contains domains not in WKAppBoundDomains: {Domains}. " +
					"Sub-resource blocking via WKAppBoundDomains will NOT cover these domains. " +
					"Navigation-level blocking will still work.",
					string.Join(", ", missingFromPlist));
			}

			if (extraInPlist.Count > 0)
			{
				logger?.LogWarning(
					"WKAppBoundDomains contains domains not in AllowedDomains: {Domains}. " +
					"These domains are allowed at the OS level but will be blocked by the AllowedDomains property.",
					string.Join(", ", extraInPlist));
			}
		}
	}
}
