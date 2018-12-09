using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh4572 : ContentPage
	{
		public Gh4572() => InitializeComponent();
		public Gh4572(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void BindingAsElement(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh4572)));
				var layout = new Gh4572(useCompiledXaml) { BindingContext = new { labeltext = "Foo" } };
				Assert.That(layout.label.Text, Is.EqualTo("Foo"));
			}
		}
	}
}
