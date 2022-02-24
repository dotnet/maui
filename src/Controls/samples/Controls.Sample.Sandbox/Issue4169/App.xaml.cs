using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Issue4169;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new MainPage();
	}
}
