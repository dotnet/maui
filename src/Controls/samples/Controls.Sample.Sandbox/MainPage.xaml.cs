using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		private MainPageVM vm;
		public MainPage()
		{
			InitializeComponent();
			BindingContext = this.vm = new MainPageVM();
			var cookieContainer = new System.Net.CookieContainer();
			cookieContainer.Add(new System.Uri("https://learn.microsoft.com/dotnet/maui"), new System.Net.Cookie("name", "value"));
			this.BaseWebView.Cookies = cookieContainer;
		}

		private void MenuItem_OnClicked(object? sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}

	public class MainPageVM  
{
	int i = 10;
	public ICommand TestCommand {get ; private set; }

	public MainPageVM()
	{
		TestCommand = new Command(()=> Update(), canExecute: () => true);
	}

	void Update()
	{
		i++;
		System.Diagnostics.Debug.WriteLine($"Updated - Times{i}");
		//System.Console.WriteLine($"Updated - Times{i}");
	}
}
}