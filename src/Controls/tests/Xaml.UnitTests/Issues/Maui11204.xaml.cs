using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11204 : ContentPage
{
	public Maui11204() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void VSMSetterOverrideManualValues(XamlInflator inflator)
		{
			var page = new Maui11204(inflator);
			Assert.Equal(Colors.FloralWhite, page.border.BackgroundColor);
			VisualStateManager.GoToState(page.border, "State1");
			Assert.Equal(2, page.border.StrokeThickness);
			Assert.Equal(Colors.Blue, page.border.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void StyleVSMSetterOverrideManualValues(XamlInflator inflator)
		{
			var page = new Maui11204(inflator);
			Assert.Equal(Colors.HotPink, page.borderWithStyleClass.BackgroundColor);
			VisualStateManager.GoToState(page.borderWithStyleClass, "State1");
			Assert.Equal(2, page.borderWithStyleClass.StrokeThickness);
			Assert.Equal(Colors.Blue, page.borderWithStyleClass.BackgroundColor);
		}
	}
}