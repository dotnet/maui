﻿namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17347, "Setting a new TitleView on an already created page crashes iOS", PlatformAffected.iOS)]
	public class Issue17347 : NavigationPage
	{
		public Issue17347() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var navPage = new NavigationPage(new TestPage());
				var label = new Label() { Text = "NavigatedTo Has Not Fired" };
				Content = new VerticalStackLayout()
				{
					label
				};

				NavigatedTo += Issue16499_NavigatedTo;

				async void Issue16499_NavigatedTo(object sender, NavigatedToEventArgs e)
				{
					label.Text = "Navigated to Has Fired";
					NavigatedTo -= Issue16499_NavigatedTo;

					await Navigation.PushModalAsync(navPage);
					await navPage.Navigation.PushAsync(new TestPage());
					await navPage.Navigation.PushAsync(new TestPage());
					await navPage.Navigation.PopAsync();
					await navPage.Navigation.PopAsync();
				}
			}
		}

		public class TestPage : ContentPage
		{
			Label TopView;
			static int i = 0;
			protected override void OnAppearing()
			{
				Content = new VerticalStackLayout()
				{
					new Button()
					{
						AutomationId = "PopMeButton",
						Command = new Command(async () =>
						{
							if (Navigation.NavigationStack.Count == 1)
								await Navigation.PopModalAsync();
							else
								await Navigation.PopAsync();
						}),
						Text = "Click to Pop This Page If Needed"
					}
				};

				var increment = $"{i++}";
				TopView = new()
				{
					AutomationId = "TitleViewLabel" + increment
				};

				TopView.SetBinding(Label.TextProperty, "AutomationId");
				TopView.BindingContext = TopView;

				TopView.WidthRequest = App.Current.Windows[0].Page.Width / 2;
				NavigationPage.SetTitleView(this, TopView);
				NavigationPage.SetHasNavigationBar(this, true);
				NavigationPage.SetHasBackButton(this, false);
				base.OnAppearing();
			}
		}
	}
}
