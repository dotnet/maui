using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28212, "Using CollectionView.EmptyView results in an Exception on Windows", PlatformAffected.WinPhone)]
	public class Issue28212 : NavigationPage
	{
		Issue28212_Page1 _issue28212_Page1;
		public Issue28212()
		{
			_issue28212_Page1 = new Issue28212_Page1();
			this.PushAsync(_issue28212_Page1);
		}
	}

	public class Issue28212_Page1 : ContentPage
	{
		VerticalStackLayout verticalStackLayout;
		Issue28212_Page2 _issue28212_Page2;
		Button button;
		public Issue28212_Page1()
		{
			_issue28212_Page2 = new Issue28212_Page2();
			button = new Button();
			button.Text = "Click to Navigate";
			button.AutomationId = "ButtonId";
			button.Clicked += Button_Clicked;
			button.HeightRequest = 100;
			verticalStackLayout = new VerticalStackLayout();
			verticalStackLayout.Children.Add(button);
			this.Content = verticalStackLayout;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(_issue28212_Page2);
		}
	}
}
