using System;
using System.Diagnostics.CodeAnalysis;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages
{
	public partial class TitleBarPage : ContentPage
	{
		TitleBarSampleViewModel _viewModel;
		TitleBar _customTitleBar;

		[RequiresUnreferencedCode("TitleBarPage uses LoadFromXaml which may require unreferenced code")]
		public TitleBarPage()
		{
			InitializeComponent();

			_viewModel = new TitleBarSampleViewModel();
			BindingContext = _viewModel;

			string titleBarXaml =
				"""
				<TitleBar
					xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					Title="{Binding Title}"
					Subtitle="{Binding Subtitle}"
					IsVisible="{Binding ShowTitleBar}"/>
				""";

			_customTitleBar = new TitleBar().LoadFromXaml(titleBarXaml);
			_customTitleBar.BindingContext = _viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Application.Current!.Windows[0].TitleBar = _customTitleBar;
		}

		private void SetIconCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.Icon = "tb_appicon.png";
			}
			else
			{
				_customTitleBar.Icon = "";
			}
		}

		private void ColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(ColorTextBox.Text, out var color))
			{
				_customTitleBar.BackgroundColor = color;
			}
		}

		private void ForegroundColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(ForegroundColorTextBox.Text, out var color))
			{
				_customTitleBar.ForegroundColor = color;
			}
		}

		private void LeadingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.LeadingContent = new Button()
				{
					Text = "Leading"
				};
			}
			else
			{
				_customTitleBar.LeadingContent = null;
			}
		}

		private void ContentCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.Content = new SearchBar()
				{
					Placeholder = "Search",
					MinimumWidthRequest = 200,
					MaximumWidthRequest = 500,
					HeightRequest = 32
				};
			}
			else
			{
				_customTitleBar.Content = null;
			}
		}

		[RequiresUnreferencedCode("Calls Maui.Controls.Sample.Pages.TitleBarPage.TitleBarPage()")]
		async void PushNewTitleBarPage_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new TitleBarPage());
		}

		void ToggleTitleBarOnWindow_Clicked(object sender, EventArgs e)
		{
			if (Window.TitleBar is not null)
				Window.TitleBar = null;
			else
				Window.TitleBar = _customTitleBar;
		}

		void ToggleHasNavigationBar_Clicked(object sender, EventArgs eventArgs)
		{
			if (Shell.GetNavBarIsVisible(this))
			{
				Shell.SetNavBarIsVisible(this, false);
				NavigationPage.SetHasNavigationBar(this, false);
			}
			else
			{
				Shell.SetNavBarIsVisible(this, true);
				NavigationPage.SetHasNavigationBar(this, true);
			}
		}

		private void TrailingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.TrailingContent = new Border()
				{
					WidthRequest = 32,
					HeightRequest = 32,
					StrokeShape = new Ellipse() { WidthRequest = 32, HeightRequest = 32 },
					StrokeThickness = 0,
					BackgroundColor = Microsoft.Maui.Graphics.Colors.Azure,
					Content = new Label()
					{
						Text = "MC",
						TextColor = Microsoft.Maui.Graphics.Colors.Black,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 10
					}
				};
			}
			else
			{
				_customTitleBar.TrailingContent = null;
			}
		}

		private void TallModeCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
#if MACCATALYST
				_customTitleBar.HeightRequest = 60;
#else
				_customTitleBar.HeightRequest = 48;
#endif
			}
			else
			{
#if MACCATALYST
				_customTitleBar.HeightRequest = 36;
#else
				_customTitleBar.HeightRequest = 32;
#endif
			}
		}
	}
}
