using System;

namespace Maui.Controls.Sample
{
	[AttributeUsage(
		AttributeTargets.Class |
		AttributeTargets.Method,
		AllowMultiple = true)]
	public class IssueAttribute : Attribute
	{
		public IssueAttribute(IssueTracker issueTracker, int issueNumber, string description,
			NavigationBehavior navigationBehavior = NavigationBehavior.Default, int issueTestNumber = 0)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber.ToString();
			Description = description;
			PlatformsAffected = PlatformAffected.Default;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
		}

		public IssueAttribute(IssueTracker issueTracker, string issueNumber, string description,
			NavigationBehavior navigationBehavior = NavigationBehavior.Default, int issueTestNumber = 0)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber;
			Description = description;
			PlatformsAffected = PlatformAffected.Default;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
		}

		public IssueAttribute(IssueTracker issueTracker, int issueNumber, string description,
			PlatformAffected platformsAffected, NavigationBehavior navigationBehavior = NavigationBehavior.Default,
			int issueTestNumber = 0)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber.ToString();
			Description = description;
			PlatformsAffected = platformsAffected;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
		}

		public IssueAttribute(IssueTracker issueTracker, string issueNumber, string description,
			PlatformAffected platformsAffected, NavigationBehavior navigationBehavior = NavigationBehavior.Default,
			int issueTestNumber = 0)
		{
			IssueTracker = issueTracker;
			IssueNumber = issueNumber;
			Description = description;
			PlatformsAffected = platformsAffected;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
		}

		public IssueTracker IssueTracker { get; }

		public string IssueNumber { get; }

		public int IssueTestNumber { get; }

		public string Description { get; }

		public PlatformAffected PlatformsAffected { get; }

		public NavigationBehavior NavigationBehavior { get; }

		public string DisplayName => IssueTestNumber == 0
			? $"{IssueTracker.ToString().Substring(0, 1)}{IssueNumber}"
			: $"{IssueTracker.ToString().Substring(0, 1)}{IssueNumber} ({IssueTestNumber})";
	}
}