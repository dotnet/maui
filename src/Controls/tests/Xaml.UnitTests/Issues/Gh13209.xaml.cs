using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh13209 : ContentPage
{
	public Gh13209() => InitializeComponent();

	public class Tests : IDisposable
	{
		public void Dispose()
		{
			ResourceDictionary.ClearCache();
		}

		[Theory]
		[Values]
		public void RdWithSource(XamlInflator inflator)
		{
			var layout = new Gh13209(inflator);
			Assert.Equal(Colors.Chartreuse, layout.MyRect.BackgroundColor);
			Assert.Single(layout.Root.Resources);
			Assert.Empty(layout.Root.Resources.MergedDictionaries);

			Assert.NotNull(layout.Root.Resources["Color1"]);
			Assert.True(layout.Root.Resources.Remove("Color1"));
			Assert.Throws<KeyNotFoundException>(() =>
			{
				var _ = layout.Root.Resources["Color1"];
			});

		}
	}
}
