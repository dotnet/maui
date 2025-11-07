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


	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void ByteEnum(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Maui32056), out var methodDef, out var hasLoggedErrors);
				Assert.False(hasLoggedErrors);
			}
			var page = new Maui32056(inflator);
			Assert.Equal(Maui32056Enum.A.ToString(), page.label0.Text);
		}

		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}
	}
}
