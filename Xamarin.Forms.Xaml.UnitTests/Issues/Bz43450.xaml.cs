using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz43450 : ContentPage
	{
		public Bz43450()
		{
			InitializeComponent();
		}

		public Bz43450(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void DoesNotAllowGridRowDefinition(bool useCompiledXaml)
			{
				if (!useCompiledXaml)
					Assert.Throws<XamlParseException>(() => new Bz43450(useCompiledXaml));
				else
					Assert.Throws<XamlParseException>(() => MockCompiler.Compile(typeof(Bz43450)));
			}
		}
	}
}