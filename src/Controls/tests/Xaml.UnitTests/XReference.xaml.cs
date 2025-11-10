using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XReference : ContentPage
{
	public XReference() => InitializeComponent();


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
		public void SupportsXReference(XamlInflator inflator)
		{
			var layout = new XReference(inflator);
			Assert.Same(layout.image, layout.imageView.Content);
		}

		[Theory]
		[Values]
		public void XReferenceAsCommandParameterToSelf(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			var button = layout.aButton;
			bool commandExecutedCorrectly = false;
			button.BindingContext = new
			{
				ButtonClickCommand = new Command(o =>
				{
					if (o == button)
						commandExecutedCorrectly = true;
				})
			};
			((IButtonController)button).SendClicked();
			Assert.True(commandExecutedCorrectly, "Command should have been executed with button as parameter");
		}

		[Theory]
		[Values]
		public void XReferenceAsBindingSource(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.Equal("foo", layout.entry.Text);
			Assert.Equal("bar", layout.entry.Placeholder);
		}

		[Theory]
		[Values]
		public void CrossXReference(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.Same(layout.label0, layout.label1.BindingContext);
			Assert.Same(layout.label1, layout.label0.BindingContext);
		}
	}
}
