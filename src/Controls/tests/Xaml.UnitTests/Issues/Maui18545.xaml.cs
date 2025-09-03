using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18545 : ContentPage
{
	public Maui18545() => InitializeComponent();

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void DynamicResourcesOnGradient([Values] XamlInflator inflator)
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
			Application.Current.Resources.MergedDictionaries.Add(lighttheme);
			var page = new Maui18545(inflator);
			Application.Current.MainPage = page;

			Assert.That(page.label.Background, Is.TypeOf<LinearGradientBrush>());
			var brush = (LinearGradientBrush)page.label.Background;
			Assert.That(brush.GradientStops[0].Color, Is.EqualTo(Colors.Red));

			Application.Current.Resources.MergedDictionaries.Remove(lighttheme);
			Application.Current.Resources.MergedDictionaries.Add(darktheme);
			page.Resources["GradientColorStart"] = Colors.Green;
			Assert.That(brush.GradientStops[0].Color, Is.EqualTo(Colors.Green));
		}
	}
}