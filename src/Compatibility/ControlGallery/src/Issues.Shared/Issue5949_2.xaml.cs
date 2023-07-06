using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	public partial class Issue5949_2 : ContentPage
	{
		public const string BackButton = "5949GoBack";
		public const string ToolBarItem = "Login";

		public Issue5949_2()
		{
#if APP
			InitializeComponent();
			ToolbarItems.Add(new ToolbarItem(ToolBarItem, null, () => Navigation.PushAsync(LoginPage())));
			BindingContext = new _5949ViewModel();
#endif
		}

		[Preserve(AllMembers = true)]
		class _5949ViewModel
		{
			public _5949ViewModel()
			{
				Items = new List<string>
				{
					"one", "two", "three"
				};
			}

			public List<string> Items { get; set; }
		}

		ContentPage LoginPage()
		{
			var page = new ContentPage
			{
				Title = "Issue 5949"
			};

			var button = new Button { Text = "Back", AutomationId = BackButton };

			button.Clicked += ButtonClicked;

			page.Content = button;

			return page;
		}

		private void ButtonClicked(object sender, EventArgs e)
		{
			Application.Current.MainPage = new Issue5949_1();
		}
	}
}