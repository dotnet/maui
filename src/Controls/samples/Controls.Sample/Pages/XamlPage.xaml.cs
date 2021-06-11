using System;
using System.Diagnostics;
using Maui.Controls.Sample.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class XamlPage : BasePage
	{
		public XamlPage()
		{
			InitializeComponent();

			foreach (var x in MyLayout)
			{
				Debug.WriteLine($"{x}");
			}
		}

		void OnClick(object sender, EventArgs e)
		{
			Application.Current.MainPage = Application.Current.MainPage.Handler.MauiContext.Services.GetRequiredService<Pages.XamlPage>();
		}
	}
}