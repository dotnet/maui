using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class XStaticException : ContentPage
	{
		public XStaticException()
		{
			InitializeComponent();
		}

		public XStaticException(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			//{x:Static Member=prefix:typeName.staticMemberName}
			//{x:Static prefix:typeName.staticMemberName}

			//The code entity that is referenced must be one of the following:
			// - A constant
			// - A static property
			// - A field
			// - An enumeration value
			// All other cases should throw

			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();

				//there's a test not resetting the values correctly, but can't find which one...
#pragma warning disable 0618
				Xamarin.Forms.Internals.ResourceLoader.ExceptionHandler = null;
				Xamarin.Forms.Xaml.Internals.XamlLoader.DoNotThrowOnExceptions = false;
#pragma warning restore 0618
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ThrowOnInstanceProperty(bool useCompiledXaml)
			{
				if (!useCompiledXaml)
					Assert.Throws(new XamlParseExceptionConstraint(7, 6), () => new XStaticException(useCompiledXaml));
				else
					Assert.Throws(new XamlParseExceptionConstraint(7, 6), () => MockCompiler.Compile(typeof(XStaticException)));
			}
		}
	}
}