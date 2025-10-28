using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class AutoMergedResourceDictionaries : ContentPage
	{
		public AutoMergedResourceDictionaries()
		{
			InitializeComponent();
		}

		public AutoMergedResourceDictionaries(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void AutoMergedRd(bool useCompiledXaml)
			{
				var layout = new AutoMergedResourceDictionaries(useCompiledXaml);
				Assert.Equal(Colors.Purple, layout.label.TextColor);
				Assert.Equal(Color.FromArgb("#FF96F3", layout.label.BackgroundColor));
			}
		}
	}
}
