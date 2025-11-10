using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18545 : ContentPage
{
	public Maui18545() => InitializeComponent();


	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void DynamicResourcesOnGradient(XamlInflator inflator)
		{
			var lighttheme = new ResourceDictionary
			{
				["GradientColorStart"] = Colors.Red,
				["GradientColorEnd"] = Colors.Blue
			};
			var darktheme = new ResourceDictionary
			{
				["GradientColorStart"] = Colors.Green,
				["GradientColorEnd"] = Colors.Yellow
			};
			var app = Application.Current;
			app.Resources.MergedDictionaries.Add(lighttheme);
			var page = new Maui18545(inflator);
			app.MainPage = page;

			Assert.IsType<LinearGradientBrush>(page.label.Background);
			var brush = (LinearGradientBrush)page.label.Background;
			Assert.Equal(Colors.Red, brush.GradientStops[0].Color);

			app.Resources.MergedDictionaries.Remove(lighttheme);
			app.Resources.MergedDictionaries.Add(darktheme);
			page.Resources["GradientColorStart"] = Colors.Green;
			Assert.Equal(Colors.Green, brush.GradientStops[0].Color);
		}
	}
}
