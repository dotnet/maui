using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh2574 : ContentPage
	{
		public Gh2574()
		{
			InitializeComponent();
		}

		public Gh2574(bool useCompiledXaml)
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

			[TestCase(false),TestCase(true)]
			public void xNameOnRoot(bool useCompiledXaml)
			{
				var layout = new Gh2574(useCompiledXaml);
				Assert.That(layout.page, Is.EqualTo(layout));
			}
		}
	}
}
