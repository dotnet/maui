using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TestSharedResourceDictionary : ContentPage
{
	public TestSharedResourceDictionary() => InitializeComponent();

	public class Tests : IDisposable
	{

		public void Dispose()
		{
			Application.Current = null;
		}
		public Tests()
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary
				{
					new MyRD()
				}
			};
		}

		[Theory]
		[Values]
		public void MergedResourcesAreFound(XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
		}

		[Theory]
		[Values]
		public void NoConflictsBetweenSharedRDs(XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
			Assert.Equal(Colors.Purple, layout.label2.TextColor);
		}

		[Theory]
		[Values]
		public void ImplicitStyleCanBeSharedFromSharedRD(XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
			Assert.Equal(Colors.Red, layout.implicitLabel.TextColor);
		}

		class MyRD : ResourceDictionary
		{
			public MyRD()
			{
				Add("foo", "Foo");
				Add("bar", "Bar");
			}
		}

		[Theory]
		[Values]
		public void MergedRDAtAppLevel(XamlInflator inflator)
		{
			var layout = new TestSharedResourceDictionary(inflator);
			Assert.Equal("Foo", layout.label3.Text);
		}

	}
}