using NUnit.Framework;

using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class TestSharedResourceDictionary : ContentPage
	{
		public TestSharedResourceDictionary ()
		{
			InitializeComponent ();
		}

		public TestSharedResourceDictionary (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void MergedResourcesAreFound (bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary (useCompiledXaml);
				Assert.AreEqual (Color.Pink, layout.label.TextColor);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void NoConflictsBetweenSharedRDs (bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary (useCompiledXaml);
				Assert.AreEqual (Color.Pink, layout.label.TextColor);
				Assert.AreEqual (Color.Purple, layout.label2.TextColor);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void ImplicitStyleCanBeSharedFromSharedRD (bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual(Color.Red, layout.implicitLabel.TextColor);
			}
		}
	}
}