using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui26977 : ContentPage
{
	public Maui26977() => InitializeComponent();

	[Collection("Issue")]
	public class SetterTargetNameWithControlTemplateTests : IDisposable
	{
		public SetterTargetNameWithControlTemplateTests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		// When a ContentPage has a ControlTemplate, the ContentPresenter creates a namescope
		// boundary. Setter.TargetName should still resolve elements across that boundary.
		internal void SetterTargetNameWithControlTemplateShouldNotCrash(XamlInflator inflator)
		{
			var ex = Record.Exception(() => new Maui26977(inflator));
			Assert.Null(ex);
		}

		[Theory]
		[XamlInflatorData]
		internal void SetterTargetNameWithControlTemplateShouldApplyValue(XamlInflator inflator)
		{
			var page = new Maui26977(inflator);

			VisualStateManager.GoToState(page.RootStackLayout, "State1");
			Assert.Equal("State1Text", page.TargetLabel.Text);

			VisualStateManager.GoToState(page.RootStackLayout, "State2");
			Assert.Equal("State2Text", page.TargetLabel.Text);
		}
	}
}
