using System;
using NUnit.Framework;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TypeConverter(typeof(ThicknessTypeConverter))]
	public struct Bz55862Bar
	{
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz55862 : ContentPage
	{
		public Bz55862Bar Foo { get; set; }
		public Bz55862()
		{
			InitializeComponent();
		}

		public Bz55862(bool useCompiledXaml)
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
			public void BindingContextWithConverter(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Bz55862)));
				else
					Assert.Throws<XamlParseException>(() => new Bz55862(useCompiledXaml));
			}
		}
	}
}
