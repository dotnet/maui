using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz44213 : ContentPage
	{
		public Bz44213()
		{
			InitializeComponent();
		}

		public Bz44213(bool useCompiledXaml)
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
			public void BindingInOnPlatform(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
				var p = new Bz44213(useCompiledXaml);
				p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
				Assert.AreEqual("Foo", p.label.Text);
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
				p = new Bz44213(useCompiledXaml);
				p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
				Assert.AreEqual("Bar", p.label.Text);
			}
		}
	}
}