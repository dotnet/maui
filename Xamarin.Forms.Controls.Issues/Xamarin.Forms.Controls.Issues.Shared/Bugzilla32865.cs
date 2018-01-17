using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32865, "On MasterDetailPage trying to change Icon of Master page doesn\'t work if another view is pushed and Image is renderer in blue", PlatformAffected.iOS)]
	public class Bugzilla32865 : TestMasterDetailPage 
	{
		public static Bugzilla32865 Mdp;

		protected override void Init()
		{
			Mdp = this;

			Master = new ContentPage {Title = "Master"};
			Detail = new NavigationPage(new DetailView32865());
		}

		public void ChangeIcon()
		{
			Master.Icon = "settings";
		}
		public void ChangeIcon2()
		{
			Master.Icon = "menuIcon";
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