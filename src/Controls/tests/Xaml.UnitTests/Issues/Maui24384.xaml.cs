using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24384 : ContentPage
{
	public static System.Collections.Immutable.ImmutableArray<string> StaticLetters => ["A", "B", "C"];

	public Maui24384()
	{
		InitializeComponent();
	}


	public Maui24384(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
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
		public void ImmutableToIList([Values] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui24384));
			// Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui24384)));
			var page = new Maui24384(useCompiledXaml);
			var picker = page.Content as Picker;
			Assert.That(picker.ItemsSource, Is.EquivalentTo(Maui24384.StaticLetters));
		}
	}
}