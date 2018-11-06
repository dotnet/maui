using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	// This test covers the issue reported in https://github.com/xamarin/Xamarin.Forms/issues/2247
	// for NavigationBehavior.PushAsync. Coverage for NavigationBehavior.PushModalAsync is provided by Bugzilla33561.

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2247, 
		"[iOS] ListView.IsRefreshing not showing activity indicator on iOS", NavigationBehavior.PushAsync)]
	public class Issue2247 : TestContentPage
	{
		ListView _listView;
		bool _isRefreshing;

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "The ListView on this page should be displaying the 'refreshing' activity indicator." 
						+ " If it is not, the test has failed"
			};

			var template = new DataTemplate(typeof(TextCell));
			template.SetBinding(TextCell.TextProperty, ".");

			_listView = new ListView
			{
				IsPullToRefreshEnabled = true,
				ItemsSource = Enumerable.Range(0, 10).Select(no => $"Item {no}"),
				ItemTemplate = template,
				IsRefreshing = true
			};

			Content = new StackLayout
			{
				Children = {  instructions, _listView }
			};
		}
	}
}
