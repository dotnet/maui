﻿using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32865, "On FlyoutPage trying to change Icon of Flyout page doesn\'t work if another view is pushed and Image is renderer in blue", PlatformAffected.iOS)]
	public class Bugzilla32865 : TestFlyoutPage
	{
		public static Bugzilla32865 Mdp;

		protected override void Init()
		{
			Mdp = this;

			Flyout = new ContentPage { Title = "Flyout" };
			Detail = new NavigationPage(new DetailView32865());
		}

		public void ChangeIcon()
		{
			Flyout.IconImageSource = "settings";
		}
		public void ChangeIcon2()
		{
			Flyout.IconImageSource = "menuIcon";
		}
	}

	[Preserve(AllMembers = true)]
	public class DetailView32865 : ContentPage
	{
		public DetailView32865()
		{
			Title = "Page1";

			var label = new Label
			{
				Text = "Push a page and then change master icon. The icon should be changeable from any page on the navigation stack.",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};

			var button = new Button()
			{
				Text = "Icon 1",
			};
			button.Clicked += Button_Clicked;
			var button2 = new Button()
			{
				Text = "Icon 2",
			};
			button2.Clicked += Button2_Clicked;
			var button3 = new Button()
			{
				Text = "Push Page",
			};
			button3.Clicked += Button3_Clicked;

			var layout = new StackLayout()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = { label, button, button2, button3 },
			};
			Content = layout;
		}
		void Button3_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new DetailView232865());
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Bugzilla32865.Mdp.ChangeIcon();
		}

		void Button2_Clicked(object sender, EventArgs e)
		{
			Bugzilla32865.Mdp.ChangeIcon2();
		}
	}

	[Preserve(AllMembers = true)]
	public class DetailView232865 : ContentPage
	{
		public DetailView232865()
		{
			Title = "Page2";

			var button = new Button()
			{
				Text = "Icon 1",
			};
			button.Clicked += Button_Clicked;

			var button2 = new Button()
			{
				Text = "Icon 2",
			};
			button2.Clicked += Button2_Clicked;

			var layout = new StackLayout()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = { button, button2 },
			};

			Content = layout;
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Bugzilla32865.Mdp.ChangeIcon();
		}

		void Button2_Clicked(object sender, EventArgs e)
		{
			Bugzilla32865.Mdp.ChangeIcon2();
		}
	}
}