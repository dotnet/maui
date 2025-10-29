using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class BPNotResolvedOnSubClass : ContentPage
	{
		public static readonly BindableProperty ShadowColorProperty =
			BindableProperty.CreateAttached("ShadowColor", typeof(Color), typeof(BPNotResolvedOnSubClass), null);

		public static Color GetShadowColor(Element bindable) // Change to Element instead of BindableObject o make fail
		{
			return (Color)bindable.GetValue(ShadowColorProperty);
		}

		public BPNotResolvedOnSubClass()
		{
			InitializeComponent();
		}

		public BPNotResolvedOnSubClass(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public class Tests
		{
			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public void CorrectlyResolveBPOnSubClasses(bool useCompiledXaml)
			{
				var layout = new BPNotResolvedOnSubClass(useCompiledXaml);
				var style = (Style)layout.Resources["Microsoft.Maui.Controls.Button"];
				Assert.NotNull(style);

				var button = new Button();
				button.Style = style;

				Assert.Equal(Color.FromArgb("#dddddd"), button.GetValue(ShadowColorProperty));
			}
		}
	}
}

