using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh5330 : ContentPage
	{
		public Gh5330() => InitializeComponent();
		public Gh5330(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void DoesntFailOnxType([Values(true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh5330)));
			}

			[Test]
			public void CompiledBindingWithxType([Values(true)] bool useCompiledXaml)
			{
				var layout = new Gh5330(useCompiledXaml) { BindingContext = new Button { Text = "Foo" } };
				Assert.That(layout.label.Text, Is.EqualTo("Foo"));
			}
		}
	}
}
