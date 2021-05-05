using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
						new MyRD()
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
				Assert.AreEqual(Colors.Pink, layout.label.TextColor);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void NoConflictsBetweenSharedRDs(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual(Colors.Pink, layout.label.TextColor);
				Assert.AreEqual(Colors.Purple, layout.label2.TextColor);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ImplicitStyleCanBeSharedFromSharedRD(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual(Colors.Red, layout.implicitLabel.TextColor);
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