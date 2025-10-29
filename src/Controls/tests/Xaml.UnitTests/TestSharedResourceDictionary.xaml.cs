using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TestSharedResourceDictionary : ContentPage
{
	public TestSharedResourceDictionary() => InitializeComponent();

	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary
				{
					new MyRD()
				}
			};
		}

		[TearDown] public void TearDown() => Application.ClearCurrent();

		[Test]
		public void MergedResourcesAreFound([Values] XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
			Assert.AreEqual(Colors.Pink, layout.label.TextColor);
		}

		[Test]
		public void NoConflictsBetweenSharedRDs([Values] XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
			Assert.AreEqual(Colors.Pink, layout.label.TextColor);
			Assert.AreEqual(Colors.Purple, layout.label2.TextColor);
		}

		[Test]
		public void ImplicitStyleCanBeSharedFromSharedRD([Values] XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
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

		[Test]
		public void MergedRDAtAppLevel([Values] XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
			Assert.AreEqual("Foo", layout.label3.Text);
		}

	}
}