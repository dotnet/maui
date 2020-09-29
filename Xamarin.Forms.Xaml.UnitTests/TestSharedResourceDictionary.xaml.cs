using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class TestSharedResourceDictionary : ContentPage
	{
		public TestSharedResourceDictionary()
		{
			InitializeComponent();
		}

		public TestSharedResourceDictionary(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
				Application.Current = new MockApplication
				{
					Resources = new ResourceDictionary
					{
#pragma warning disable 618
						MergedWith = typeof(MyRD)
#pragma warning restore 618
					}
				};
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void MergedResourcesAreFound(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual(Color.Pink, layout.label.TextColor);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void NoConflictsBetweenSharedRDs(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual(Color.Pink, layout.label.TextColor);
				Assert.AreEqual(Color.Purple, layout.label2.TextColor);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ImplicitStyleCanBeSharedFromSharedRD(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual(Color.Red, layout.implicitLabel.TextColor);
			}

			class MyRD : ResourceDictionary
			{
				public MyRD()
				{
					Add("foo", "Foo");
					Add("bar", "Bar");
				}
			}

			[TestCase(false)]
			[TestCase(true)]
			public void MergedRDAtAppLevel(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual("Foo", layout.label3.Text);
			}

		}
	}
}