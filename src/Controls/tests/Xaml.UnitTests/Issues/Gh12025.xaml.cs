using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh12025NavPage : NavigationPage
	{
		public static new readonly BindableProperty IconColorProperty = BindableProperty.CreateAttached("IconColor", typeof(Color), typeof(Page), null);
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
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh12025)));
				var layout = new Gh12025(useCompiledXaml);
				Assert.Equal(NavigationPage.IconColorProperty.DefaultValue, NavigationPage.GetIconColor(layout));
				Assert.Equal(Colors.HotPink, Gh12025NavPage.GetIconColor(layout));
			}
		}
	}
}