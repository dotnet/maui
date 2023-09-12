using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14557, "CollectionView header and footer not displaying on Windows", PlatformAffected.UWP)]
	class Issue14557 : TestContentPage
	{
		protected override void Init()
		{
			var scrollView = new ScrollView();

			var headerLabel = new Label { AutomationId = "headerLabel", Text = "Foo", };
			var footerLabel = new Label { AutomationId = "footerLabel", Text = "Bar" };

			var collectionView = new CollectionView { Header = headerLabel, Footer = footerLabel, AutomationId = "collectionView" };

			scrollView.Content = collectionView;

			Content = scrollView;
		}
	}
}
