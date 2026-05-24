using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

    public enum Maui32056Enum : byte
    {
        A = 0,
        B = 1
    }

public partial class Maui32056 : ContentPage
{
	public Maui32056()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ByteEnum(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Maui32056), out var methodDef, out var hasLoggedErrors);
				Assert.False(hasLoggedErrors);
			}
			var page = new Maui32056(inflator);
			Assert.Equal(Maui32056Enum.A.ToString(), page.label0.Text);			
		}
	}
}