namespace maui_app;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
