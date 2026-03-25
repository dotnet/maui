using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class AutoMergedResourceDictionaries : ContentPage
{
	public AutoMergedResourceDictionaries()
	{
		InitializeComponent();
	}

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => Application.Current = new MockApplication();
		public void Dispose() => Application.Current = null;

		[Theory]
		[XamlInflatorData]
		internal void AutoMergedRd(XamlInflator inflator)
		{
			var layout = new AutoMergedResourceDictionaries(inflator);
			Assert.Equal(Colors.Purple, layout.label.TextColor);
			Assert.Equal(Color.FromArgb("#FF96F3"), layout.label.BackgroundColor);
		}
	}
}
