using Xamarin.Forms;

using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	//this covers Issue2125 as well
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue2450 : ContentPage
	{
		public Issue2450 ()
		{
			InitializeComponent ();
		}

		public Issue2450 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup ()
			{
				Device.PlatformServices = new MockPlatformServices ();
			}

			[TestCase (false)]
			[TestCase (true)]
			public void ThrowMeaningfulExceptionOnDuplicateXName (bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new XamlParseExceptionConstraint(8, 10, m => m == "An element with the name \"label0\" already exists in this NameScope"),
								  () => MockCompiler.Compile(typeof(Issue2450)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(8, 10, m => m == "An element with the name \"label0\" already exists in this NameScope"),
								  () => new Issue2450(useCompiledXaml));
			}
		}
	}
}