using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh12025NavPage : NavigationPage
	{
		public static new readonly BindableProperty IconColorProperty = BindableProperty.CreateAttached("IconColor", typeof(Color), typeof(Page), Color.Default);
		public static void SetIconColor(Page page, Color barTintColor) => page.SetValue(IconColorProperty, barTintColor);
		public static Color GetIconColor(Page page) => (Color)page.GetValue(IconColorProperty);
	}

	public partial class Gh12025 : ContentPage
	{
		public Gh12025() => InitializeComponent();
		public Gh12025(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void FindMostDerivedABP([Values(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh12025)));
				var layout = new Gh12025(useCompiledXaml);
				Assert.That(NavigationPage.GetIconColor(layout), Is.EqualTo(NavigationPage.IconColorProperty.DefaultValue));
				Assert.That(Gh12025NavPage.GetIconColor(layout), Is.EqualTo(Color.HotPink));
			}
		}
	}
}