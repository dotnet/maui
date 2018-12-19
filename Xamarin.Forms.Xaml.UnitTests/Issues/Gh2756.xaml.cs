using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh2756 : ContentPage
	{
		public Gh2756() => InitializeComponent();
		public Gh2756(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void UnescapedBraces([Values(false, true)]bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<XamlParseException>(() => MockCompiler.Compile(typeof(Gh2756)));
				else
					Assert.Throws<XamlParseException>(() => new Gh2756(useCompiledXaml));
			}
		}
	}
}
