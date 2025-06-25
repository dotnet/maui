using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

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
		}		public class Tests
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
			}			public void TearDown()
			{
				Application.ClearCurrent();
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void MergedResourcesAreFound(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.label.TextColor);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void NoConflictsBetweenSharedRDs(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.label.TextColor);
				Assert.Equal(Colors.Purple, layout.label2.TextColor);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ImplicitStyleCanBeSharedFromSharedRD(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
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
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void MergedRDAtAppLevel(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.Equal("Foo", layout.label3.Text);
			}

		}
	}
}