using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25608
{
	public Maui25608()
	{
		InitializeComponent();
	}


	public class Test : IDisposable
	{
		EventHandler<BindingBaseErrorEventArgs> _bindingFailureHandler;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			if (_bindingFailureHandler is not null)
			{
				BindingDiagnostics.BindingFailed -= _bindingFailureHandler;
				Application.Current = null;
				DispatcherProvider.SetCurrent(null);
			}

			AppInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void TestValidBindingWithRelativeSource(XamlInflator inflator)
		{
			bool bindingFailureReported = false;
			_bindingFailureHandler = (sender, args) => bindingFailureReported = true;
			BindingDiagnostics.BindingFailed += _bindingFailureHandler;

			var page = new Maui25608(inflator);

			Assert.Equal(25, page.Image.HeightRequest);
			Assert.False(bindingFailureReported);
		}
	}
}
