using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10699, "RefreshView IsEnabled Binding not updating", PlatformAffected.iOS)]
	public class Issue10699 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var refreshView = new RefreshView();

			var layout = new StackLayout
			{
				Padding = new Thickness(30)
			};

			var checkbox = new CheckBox
			{
				IsChecked = refreshView.IsEnabled
			};

			checkbox.CheckedChanged += (s, e) =>
			{
				refreshView.IsEnabled = checkbox.IsChecked;
			};

			layout.Children.Add(new Label { Text = "Pull to refresh, then uncheck and check the checkbox. Trying pull to refresh again should work" });
			layout.Children.Add(checkbox);

			refreshView.Content = new ScrollView
			{
				Content = layout
			};

			refreshView.Command = new Command(async () =>
			{
				await Task.Delay(1000);
				refreshView.IsRefreshing = false;
			});

			Content = refreshView;
		}
	}
}