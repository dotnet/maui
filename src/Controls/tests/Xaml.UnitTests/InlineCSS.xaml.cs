using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class InlineCSS : ContentPage
{
	public InlineCSS() => InitializeComponent();


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
		public void InlineCSSParsed(XamlInflator inflator)
		{
			var layout = new InlineCSS(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
		}

		[Theory]
		[Values]
		public void InitialValue(XamlInflator inflator)
		{
			var layout = new InlineCSS(inflator);
			Assert.Equal(Colors.Green, layout.BackgroundColor);
			Assert.Equal(Colors.Green, layout.stack.BackgroundColor);
			Assert.Equal(Colors.Green, layout.button.BackgroundColor);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, layout.label.BackgroundColor);
			Assert.Equal(TextTransform.Uppercase, layout.label.TextTransform);
		}
	}
}
