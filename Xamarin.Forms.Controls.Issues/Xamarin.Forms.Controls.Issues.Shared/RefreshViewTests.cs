using System.Linq;
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
	[Category(UITestCategories.RefreshView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Refresh View Tests", PlatformAffected.All)]
	public class RefreshViewTests : TestContentPage
	{
		RefreshView _refreshView;
		public RefreshViewTests()
		{
		}

		protected override void Init()
		{
			Title = "Refresh View Tests";
			var scrollViewContent =
				new StackLayout()
				{
				};

			Enumerable.Range(0, 10).Select(_ => new Label() { HeightRequest = 200, Text = "Pull me down to refresh me" })
				.ForEach(x => scrollViewContent.Children.Add(x));

			_refreshView = new RefreshView()
			{
				Content = new ScrollView()
				{
					HeightRequest = 2000,
					BackgroundColor = Color.Green,
					Content = scrollViewContent,
					AutomationId = "LayoutContainer"
				},
				Command = new Command(async () =>
				{
					await Task.Delay(2000);
					_refreshView.IsRefreshing = false;
				})
			};

			var isRefreshingLabel = new Label();

			var label = new Label { BindingContext = _refreshView };
			isRefreshingLabel.SetBinding(Label.TextProperty, new Binding("IsRefreshing", stringFormat: "IsRefreshing: {0}", source: _refreshView));

			Content = new StackLayout()
			{
				Children =
				{
					isRefreshingLabel,
					new Button()
					{
						Text = "Toggle Refresh",
						Command = new Command(() =>
						{
							_refreshView.IsRefreshing = !_refreshView.IsRefreshing;
						})
					},
					_refreshView
				}
			};
		}
#if UITEST
		[Test]
		public void IsRefreshingAndCommandTest()
		{
			RunningApp.Tap(q => q.Button("Toggle Refresh"));
			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: True"));
			RunningApp.Screenshot("Refreshing");
			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: False"));
			RunningApp.Screenshot("Refreshed");
		}

		[Test]
		public void IsRefreshingAndCommandTest_SwipeDown()
		{
			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: False"));

			var container = RunningApp.WaitForElement("LayoutContainer")[0];

			RunningApp.Pan(new Drag(container.Rect, Drag.Direction.TopToBottom, Drag.DragLength.Medium));

			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: True"));
			RunningApp.Screenshot("Refreshing");
			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: False"));
			RunningApp.Screenshot("Refreshed");
		}
#endif
	}
}