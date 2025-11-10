using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleSheet : ContentPage
{
	public StyleSheet() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests()
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
		public void EmbeddedStyleSheetsAreLoaded(XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.True(layout.Resources.StyleSheets[0].Styles.Count >= 1);
		}

		[Theory]
		[Values]
		public void StyleSheetsAreApplied(XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.Equal(Colors.Azure, layout.label0.TextColor);
			Assert.Equal(Colors.AliceBlue, layout.label0.BackgroundColor);
		}
	}
}