using System;

namespace Microsoft.Maui.Controls.CustomAttributes
{

	[AttributeUsage(
		AttributeTargets.Class |
		AttributeTargets.Event |
		AttributeTargets.Property |
		AttributeTargets.Method |
		AttributeTargets.Delegate,
		AllowMultiple = true
		)]
	public class PlatformAttribute : Attribute
	{
		readonly string _platform;
		public PlatformAttribute(object platform)
		{
			_platform = platform.ToString();
		}

		public string Platform => "Issue: " + _platform;
	}

	public enum IssueTracker
	{
		Github,
		Bugzilla,
		ManualTest,
		None
	}

	public enum NavigationBehavior
	{
		PushAsync,
		PushModalAsync,
		SetApplicationRoot,
		Default
	}


	[AttributeUsage(
		AttributeTargets.Class |
		AttributeTargets.Method,
		AllowMultiple = true
		)]
	public class IssueAttribute : Attribute
	{

		public IssueAttribute(IssueTracker issueTracker, int issueNumber, string description,
			NavigationBehavior navigationBehavior = NavigationBehavior.Default, int issueTestNumber = 0, bool isInternetRequired = false)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber.ToString();
			Description = description;
			PlatformsAffected = PlatformAffected.Default;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
			IsInternetRequired = isInternetRequired;
		}

		public IssueAttribute(IssueTracker issueTracker, string issueNumber, string description,
			NavigationBehavior navigationBehavior = NavigationBehavior.Default, int issueTestNumber = 0, bool isInternetRequired = false)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber;
			Description = description;
			PlatformsAffected = PlatformAffected.Default;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
			IsInternetRequired = isInternetRequired;
		}

		public IssueAttribute(IssueTracker issueTracker, int issueNumber, string description,
			PlatformAffected platformsAffected, NavigationBehavior navigationBehavior = NavigationBehavior.Default,
			int issueTestNumber = 0, bool isInternetRequired = false)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber.ToString();
			Description = description;
			PlatformsAffected = platformsAffected;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
			IsInternetRequired = isInternetRequired;
		}

		public IssueAttribute(IssueTracker issueTracker, string issueNumber, string description,
			PlatformAffected platformsAffected, NavigationBehavior navigationBehavior = NavigationBehavior.Default,
			int issueTestNumber = 0, bool isInternetRequired = false)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber;
			Description = description;
			PlatformsAffected = platformsAffected;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
			IsInternetRequired = isInternetRequired;
		}

		public IssueTracker IssueTracker { get; }

		public string IssueNumber { get; }

		public int IssueTestNumber { get; }

		public string Description { get; }

		public PlatformAffected PlatformsAffected { get; }

		public NavigationBehavior NavigationBehavior { get; }

		public bool IsInternetRequired { get; }

		public string DisplayName => IssueTestNumber == 0
			? $"{IssueTracker.ToString().Substring(0, 1)}{IssueNumber}"
			: $"{IssueTracker.ToString().Substring(0, 1)}{IssueNumber} ({IssueTestNumber})";
	}


	public class UiTestExemptAttribute : Attribute
	{
		// optional string reason
		readonly string _exemptReason;
		readonly string _description;

		public UiTestExemptAttribute(ExemptReason exemptReason, string description = null)
		{
			_exemptReason = Enum.GetName(typeof(ExemptReason), exemptReason);
			_description = description;
		}

		public string ExemptReason => "Exempt: " + _exemptReason;

		public string Description => "Description: " + _description;
	}


	public class UiTestFragileAttribute : Attribute
	{
		// optional string reason
		readonly string _description;

		public UiTestFragileAttribute(string description = null)
		{
			_description = description;
		}

		public string Description => "Description: " + _description;
	}


	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class UiTestBrokenAttribute : Attribute
	{
		// optional string reason
		readonly string _exemptReason;
		readonly string _description;

		public UiTestBrokenAttribute(BrokenReason exemptReason, string description = null)
		{
			_exemptReason = Enum.GetName(typeof(ExemptReason), exemptReason);
			_description = description;
		}

		public string ExemptReason => "Exempt: " + _exemptReason;

		public string Description => "Description: " + _description;
	}

	[Flags]
	public enum PlatformAffected
	{
		iOS = 1 << 0,
		Android = 1 << 1,
		WinPhone = 1 << 2,
		WinRT = 1 << 3,
		UWP = 1 << 4,
		WPF = 1 << 5,
		macOS = 1 << 6,
		Gtk = 1 << 7,
		All = ~0,
		Default = 0
	}

	public enum ExemptReason
	{
		None,
		AbstractClass,
		IsUnitTested,
		NeedsUnitTest,
		BaseClass,
		TimeConsuming,
		CannotTest
	}

	public enum BrokenReason
	{
		UITestBug,
		CalabashBug,
		CalabashUnsupported,
		CalabashiOSUnsupported,
		CalabashAndroidUnsupported,
	}
}