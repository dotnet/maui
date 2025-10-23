using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ByteEnum([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Maui32056), out var methodDef, out var hasLoggedErrors);
				Assert.IsFalse(hasLoggedErrors);
			}
			else
			{
				var page = new Maui32056(inflator);
				Assert.AreEqual(Maui32056Enum.A.ToString(), page.label0.Text);			
			}

		}
	}
}