using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class ShellFeaturePage : NavigationPage
{
	public ShellFeaturePage() : base(new ShellFeatureMainPage())
	{
	}
}

public partial class ShellFeatureMainPage : ContentPage
{
	public ShellFeatureMainPage()
	{
		InitializeComponent();
	}

	private void OnShellFlyoutButtonClicked(object sender, EventArgs e)
	{
		Application.Current.MainPage = new ShellFlyoutControlPage();
	}

	private void OnShellTabbedButtonClicked(object sender, EventArgs e)
	{
		Application.Current.MainPage = new ShellTabbedControlPage();
	}

	private void OnShellPageButtonClicked(object sender, EventArgs e)
	{
		this.Window.Page = new ShellControlPage();
	}
  
  private void OnShellNavigationButtonClicked(object sender, EventArgs e)
	{
		this.Window.Page = new ShellNavigationControlPage();
	}
}
