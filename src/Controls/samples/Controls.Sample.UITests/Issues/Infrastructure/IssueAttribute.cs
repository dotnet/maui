using System;
using System.Linq;

namespace Maui.Controls.Sample
{
	[AttributeUsage(
		AttributeTargets.Class |
		AttributeTargets.Method,
		AllowMultiple = true)]
	public class IssueAttribute : Attribute
	{
		//bool _modal;

		public IssueAttribute(IssueTracker issueTracker, int issueNumber, string description,
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
			IssueNumber = issueNumber;
			Description = description;
			PlatformsAffected = platformsAffected;
			NavigationBehavior = navigationBehavior;
			IssueTestNumber = issueTestNumber;
		}

		public IssueTracker IssueTracker { get; }

		public string IssueTrackerKey =>
			string.Concat(IssueTracker.ToString().Split("_").Select(x => x[..1]));

		public int IssueNumber { get; }

		public int IssueTestNumber { get; }

		public string Description { get; }

		public PlatformAffected PlatformsAffected { get; }

		public NavigationBehavior NavigationBehavior { get; }

		public string DisplayName => IssueTestNumber == 0
			? $"{IssueTrackerKey}{IssueNumber}"
			: $"{IssueTrackerKey}{IssueNumber} ({IssueTestNumber})";
	}
}