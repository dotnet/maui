
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11501, "Making Fragment Changes While App is Backgrounded Fails", PlatformAffected.Android)]
	public class Issue11501 : TestContentPage
	{
		Func<Task> _currentTest;
		Page _mainPage;
		List<Page> _modalStack;
		Window _window;
		public Issue11501()
		{
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, EventArgs e)
		{
			_window = Window;
			_mainPage = Application.Current.MainPage;
			_modalStack = Navigation.ModalStack.ToList();
		}

		private async void OnWindowActivated(object sender, EventArgs e)
		{
			DisconnectFromWindow();
			if (_currentTest is not null)
			{
				await Task.Yield();
				await _currentTest();
				_currentTest = null;
			}
		}

		void ConnectToWindow()
		{
			_window.Stopped -= OnWindowActivated;
			_window.Stopped += OnWindowActivated;
		}

		void DisconnectFromWindow()
		{
			_window.Stopped -= OnWindowActivated;
		}

		protected override void Init()
		{
			Content = new VerticalStackLayout()
			{
				new Button()
				{
					Text = "Swap Main Page",
					AutomationId = "SwapMainPage",
					Command = new Command( () =>
					{
						Application.Current.MainPage =
							new ContentPage() { Title = "Test", Content = new Label() { AutomationId = "BackgroundMe", Text = "Background/Minimize the app" } };

						ConnectToWindow();
						_currentTest = () =>
						{
							Application.Current.MainPage = CreateDestinationPage();
							return Task.CompletedTask;
						};
					})
				},
				new Button()
				{
					Text = "Changing Details/Flyout on FlyoutPage in Background",
					AutomationId = "SwapFlyoutPage",
					Command = new Command(()=>
					{
						var flyoutPage = new FlyoutPage()
						{
							Flyout = new ContentPage() { Title = "Test", Content = new Label(){Text = "Background/Minimize the app" } },
							Detail = new NavigationPage(new ContentPage(){ Title = "Test", Content = new Label() { AutomationId = "BackgroundMe", Text = "Background/Minimize the app" } })
							{
								Title = "Test"
							},
						};

						Application.Current.MainPage = flyoutPage;
						ConnectToWindow();

						_currentTest = () =>
						{
							flyoutPage.Flyout = CreateDestinationPage();
							flyoutPage.Detail = new NavigationPage(CreateDestinationPage());
							return Task.CompletedTask;
						};
					})
				},

				new Button()
				{
					Text = "Swap Tabbed Page",
					AutomationId = "SwapTabbedPage",
					Command = new Command( () =>
					{
						Application.Current.MainPage = new ContentPage() { Title = "Test", Content = new Label() {AutomationId = "BackgroundMe", Text = "Background/Minimize the app" } };
						ConnectToWindow();
						_currentTest = () =>
						{
							Application.Current.MainPage = new TabbedPage()
							{
								Children =
								{
									new NavigationPage(CreateDestinationPage())
									{
										Title = "Test"
									},
									new ContentPage() { Title = "Test", Content = new Label(){Text = "Second Page" } },
								}
							};
							return Task.CompletedTask;
						};
					})
				},
				new Button()
				{
					Text = "Removing and Changing Tabs",
					AutomationId = "RemoveAddTabs",
					Command = new Command(() =>
					{
						var tabbedPage = new TabbedPage()
						{
							Children =
							{
								new ContentPage() { Title = "Test", Content = new Label() { AutomationId = "BackgroundMe", Text = "Background/Minimize the app" } },
								new NavigationPage(CreateDestinationPage())
								{
									Title = "Test"
								}
							}
						};

						Application.Current.MainPage = tabbedPage;
						ConnectToWindow();

						_currentTest = () =>
						{
							tabbedPage.Children.RemoveAt(0);
							tabbedPage.Children.Add(CreateDestinationPage());
							return Task.CompletedTask;
						};
					})
				},
			};
		}

		ContentPage CreateDestinationPage()
		{
			return new ContentPage()
			{
				Title = "Test",
				Content = new VerticalStackLayout()
				{
					new Button()
					{
						AutomationId = "Restore",
						Text = "Restore",
						Command = new Command(async ()=>
						{
							Application.Current.MainPage = _mainPage;

							await Task.Yield();

							foreach(var page in _modalStack)
							{
								await _mainPage.Navigation.PushModalAsync(page);
							}
						})
					}
				}
			};
		}
	}
}
