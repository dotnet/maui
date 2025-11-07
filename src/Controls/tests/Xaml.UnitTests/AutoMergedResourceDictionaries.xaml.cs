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


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => Application.Current = new MockApplication();

		[Theory]
		[Values]
		public void AutoMergedRd(XamlInflator inflator)
		{
			var layout = new AutoMergedResourceDictionaries(inflator);
			Assert.Equal(Colors.Purple, layout.label.TextColor);
			Assert.Equal(Color.FromArgb("#FF96F3"), layout.label.BackgroundColor);
		}
	}
}
