using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class HotReloadStaticResourceException : ContentPage
{
	public HotReloadStaticResourceException()
	{
		InitializeComponent();
	}


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
			Controls.Internals.ResourceLoader.ResourceProvider2 = null;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = null;
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}

#if DEBUG
		[Fact(Skip = "TODO: Needs inflator parameter, convert to [Theory] [Values]")]
		public void MissingResourceExceptionAreHandled() // TODO: was (XamlInflator inflator), needs to be Theory with [Values]
		{
			// TODO: This test needs the XamlInflator parameter restored
			var inflator = XamlInflator.Runtime;
			Controls.Internals.ResourceLoader.ExceptionHandler2 = (ex) =>
			{
				var (exception, filepath) = ex;

			};
			var page = new HotReloadStaticResourceException(inflator);
		}
#endif
	}
}