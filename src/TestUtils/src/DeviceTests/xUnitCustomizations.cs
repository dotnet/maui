#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Microsoft.Maui
{
	/// <summary>
	/// Skips a test on the specified iOS or Mac Catalyst version and later.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class SkipOnIOSVersionAttribute : BeforeAfterTestAttribute
	{
		public int Major { get; }
		public int Minor { get; }
		public int Build { get; }
		public string Reason { get; }

		public SkipOnIOSVersionAttribute(int major, string reason = "")
			: this(major, 0, 0, reason) { }

		public SkipOnIOSVersionAttribute(int major, int minor, string reason = "")
			: this(major, minor, 0, reason) { }

		public SkipOnIOSVersionAttribute(int major, int minor, int build, string reason = "")
		{
			Major = major;
			Minor = minor;
			Build = build;
			Reason = reason;
		}

		public bool ShouldSkip()
		{
#if IOS || MACCATALYST
			return OperatingSystem.IsIOSVersionAtLeast(Major, Minor, Build)
				|| OperatingSystem.IsMacCatalystVersionAtLeast(Major, Minor, Build);
#else
			return false;
#endif
		}

		public string GetSkipReason() => string.IsNullOrEmpty(Reason)
			? $"Test skipped on iOS/MacCatalyst {Major}.{Minor}.{Build} or higher"
			: Reason;

		public override void Before(MethodInfo methodUnderTest, IXunitTest test)
		{
			if (ShouldSkip())
				Assert.Skip(GetSkipReason());
		}
	}

	/// <summary>
	/// Adds categories to a test method or class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class CategoryAttribute : Attribute, ITraitAttribute
	{
		readonly string[] _categories;

		public CategoryAttribute(params string[] categories) => _categories = categories;

		public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
			_categories.Select(category => new KeyValuePair<string, string>("Category", category)).ToArray();
	}

	/// <summary>
	/// Preserves category and renderer/handler variant names during discovery.
	/// </summary>
	public class FactDiscoverer : Xunit.v3.FactDiscoverer
	{
		public override ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
			ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
		{
			if (factAttribute is FactAttribute fact)
				fact.SetDisplayName(testMethod);
			return base.Discover(discoveryOptions, testMethod, factAttribute);
		}
	}

	/// <summary>
	/// Preserves variant names without replacing xUnit's theory data handling.
	/// </summary>
	public class TheoryDiscoverer : Xunit.v3.TheoryDiscoverer
	{
		public override ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
			ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
		{
			if (factAttribute is TheoryAttribute theory)
				theory.SetDisplayName(testMethod);
			return base.Discover(discoveryOptions, testMethod, factAttribute);
		}
	}

	/// <summary>
	/// Uses the test method's name with category and variant prefixes.
	/// </summary>
	[XunitTestCaseDiscoverer(typeof(FactDiscoverer))]
	public class FactAttribute : Xunit.FactAttribute
	{
		string? _originalDisplayName;

		public FactAttribute([CallerMemberName] string displayName = "",
			[CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0)
			: base(sourceFilePath, sourceLineNumber) => DisplayName = displayName;

		internal void SetDisplayName(IXunitTestMethod method)
		{
			_originalDisplayName ??= DisplayName ?? method.Method.Name;
			DisplayName = DeviceTestNames.GetPrefix(method) + _originalDisplayName;
		}
	}

	/// <summary>
	/// Uses the test method's name with category and variant prefixes.
	/// </summary>
	[XunitTestCaseDiscoverer(typeof(TheoryDiscoverer))]
	public class TheoryAttribute : Xunit.TheoryAttribute
	{
		string? _originalDisplayName;

		public TheoryAttribute([CallerMemberName] string displayName = "",
			[CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0)
			: base(sourceFilePath, sourceLineNumber) => DisplayName = displayName;

		internal void SetDisplayName(IXunitTestMethod method)
		{
			_originalDisplayName ??= DisplayName ?? method.Method.Name;
			DisplayName = DeviceTestNames.GetPrefix(method) + _originalDisplayName;
		}
	}

	static class DeviceTestNames
	{
		public static string GetPrefix(IXunitTestMethod method)
		{
			var traits = method.Traits;
			var prefix = traits.TryGetValue("Category", out var categories)
				? $"[{string.Join(", ", categories)}] "
				: string.Empty;

#if IOS || MACCATALYST
			prefix += Variant("NavigationViewVariant", "NavigationViewHandler", "NavigationRenderer");
			prefix += Variant("FlyoutViewVariant", "FlyoutViewHandler", "PhoneFlyoutPageRenderer");
			prefix += Variant("TabbedViewVariant", "TabbedViewHandler", "TabbedRenderer");
#elif ANDROID
			prefix += Variant("Variant", "Handler", "Renderer");
#endif
			return prefix;

#if ANDROID || IOS || MACCATALYST
			string Variant(string key, string handler, string renderer)
			{
				if (!traits.TryGetValue(key, out var variants))
					return string.Empty;
				if (variants.Contains(handler))
					return $"[{handler}] ";
				return variants.Contains(renderer) ? $"[{renderer}] " : string.Empty;
			}
#endif
		}
	}
}
