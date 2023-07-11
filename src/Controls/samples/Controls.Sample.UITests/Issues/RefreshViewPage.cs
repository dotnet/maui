using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.None, 0, "Refresh View Tests", PlatformAffected.All)]
	public class RefreshViewPage : TestContentPage
	{
		RefreshView _refreshView;
		Command _refreshCommand;

		public RefreshViewPage()
		{
		}

		protected override void Init()
		{
			Title = "Refresh View Tests";
			var scrollViewContent = new StackLayout();

			Enumerable
				.Range(0, 10)
				.Select(_ => new Label() { HeightRequest = 200, Text = "Pull me down to refresh me" })
				.ToList()
				.ForEach(scrollViewContent.Children.Add);


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

				await Task.Delay(4000); // Appium catalyst driver seems to be slow, have a longer delay here because of that
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
					BackgroundColor = Colors.Green,
					Content = scrollViewContent,
					AutomationId = "LayoutContainer"
				},
				Command = _refreshCommand,
				CommandParameter = true
			};

			var isRefreshingLabel = new Label { AutomationId = "IsRefreshingLabel", BindingContext = _refreshView };
			isRefreshingLabel.SetBinding(Label.TextProperty, new Binding("IsRefreshing", stringFormat: "IsRefreshing: {0}", source: _refreshView));

			var commandEnabledLabel = new Label { AutomationId = "IsEnabledLabel", BindingContext = _refreshView };
			commandEnabledLabel.SetBinding(Label.TextProperty, new Binding("IsEnabled", stringFormat: "IsEnabled: {0}", source: _refreshView));

			Content = new StackLayout()
			{
				Children =
				{
					isRefreshingLabel,
					commandEnabledLabel,
					new Button()
					{
						AutomationId = "ToggleRefresh",
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
	}
}
