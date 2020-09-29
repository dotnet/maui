using System;
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
		Command _refreshCommand;

		public RefreshViewTests()
		{
		}

		protected override void Init()
		{
			Title = "Refresh View Tests";
			var scrollViewContent = new StackLayout();

			Enumerable
				.Range(0, 10)
				.Select(_ => new Label() { HeightRequest = 200, Text = "Pull me down to refresh me" })
				.ForEach(x => scrollViewContent.Children.Add(x));


			bool canExecute = true;
			_refreshCommand = new Command(async (parameter) =>
			{
				if (!_refreshView.IsRefreshing)
				{
					throw new Exception("IsRefreshing should be true when command executes");
				}

				if (parameter != null && !(bool)parameter)
				{
					throw new Exception("Refresh command incorrectly firing with disabled parameter");
				}

				await Task.Delay(2000);
				_refreshView.IsRefreshing = false;
			}, (object parameter) =>
			{
				return parameter != null && canExecute && (bool)parameter;
			});

			_refreshView = new RefreshView()
			{
				Content = new ScrollView()
				{
					HeightRequest = 2000,
					BackgroundColor = Color.Green,
					Content = scrollViewContent,
					AutomationId = "LayoutContainer"
				},
				Command = _refreshCommand,
				CommandParameter = true
			};

			var isRefreshingLabel = new Label();

			var label = new Label { BindingContext = _refreshView };
			isRefreshingLabel.SetBinding(Label.TextProperty, new Binding("IsRefreshing", stringFormat: "IsRefreshing: {0}", source: _refreshView));

			var commandEnabledLabel = new Label { BindingContext = _refreshView };
			commandEnabledLabel.SetBinding(Label.TextProperty, new Binding("IsEnabled", stringFormat: "IsEnabled: {0}", source: _refreshView));

			Content = new StackLayout()
			{
				Children =
				{
					isRefreshingLabel,
					commandEnabledLabel,
					new Button()
					{
						Text = "Toggle Refresh",
						Command = new Command(() =>
						{
							_refreshView.IsRefreshing = !_refreshView.IsRefreshing;
						})
					},
					new Button()
					{
						Text = "Toggle Can Execute",
						Command = new Command(() =>
						{
							canExecute = !canExecute;
							_refreshCommand.ChangeCanExecute();
						}),
						AutomationId = "ToggleCanExecute"
					},
					new Button()
					{
						Text = "Toggle Can Execute Parameter",
						Command = new Command(() =>
						{
							_refreshView.CommandParameter = !((bool)_refreshView.CommandParameter);
							_refreshCommand.ChangeCanExecute();
						}),
						AutomationId = "ToggleCanExecuteParameter"
					},
					new Button()
					{
						Text = "Toggle Command Being Set",
						Command = new Command(() =>
						{
							if(_refreshView.Command != null)
								_refreshView.Command = null;
							else
								_refreshView.Command = _refreshCommand;
						}),
						AutomationId = "ToggleCommandBeingSet"
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
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void IsRefreshingAndCommandTest_SwipeDown()
		{
			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: False"));

			TriggerRefresh();
			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: True"));
			RunningApp.Screenshot("Refreshing");
			RunningApp.WaitForElement(q => q.Marked("IsRefreshing: False"));
			RunningApp.Screenshot("Refreshed");
		}

		[Test]
		[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
		public void RefreshDisablesWithCommand()
		{
			RunningApp.WaitForElement("IsRefreshing: False");
			RunningApp.Tap("ToggleCanExecute");
			RunningApp.WaitForElement("IsEnabled: False");
			TriggerRefresh();

			var results = RunningApp.Query("IsRefreshing: True");
			Assert.AreEqual(0, results.Length);
			results = RunningApp.Query("IsRefreshing: True");
			Assert.AreEqual(0, results.Length);
		}

		void TriggerRefresh()
		{
			var container = RunningApp.WaitForElement("LayoutContainer")[0];
			RunningApp.Pan(new Drag(container.Rect, Drag.Direction.TopToBottom, Drag.DragLength.Medium));

		}
#endif
	}
}