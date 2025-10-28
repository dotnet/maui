using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh13209 : ContentPage
	{
		public Gh13209() => InitializeComponent();
		public Gh13209(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => ResourceDictionary.ClearCache();

			[InlineData(true), TestCase(false)]
			public void RdWithSource(bool useCompiledXaml)
			{
				var layout = new Gh13209(useCompiledXaml);
				Assert.Equal(Colors.Chartreuse, layout.MyRect.BackgroundColor);
				Assert.Equal(1, layout.Root.Resources.Count);
				Assert.Equal(0, layout.Root.Resources.MergedDictionaries.Count);

				Assert.NotNull(layout.Root.Resources["Color1"]);
				Assert.True(layout.Root.Resources.Remove("Color1"));
				Assert.Throws<KeyNotFoundException>(() =>
				{
					var _ = layout.Root.Resources["Color1"];
				});

			}
		}
	}
}
