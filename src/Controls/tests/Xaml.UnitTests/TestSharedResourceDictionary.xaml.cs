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
		}
		public public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp]
			[Xunit.Fact]
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

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown]
			[Xunit.Fact]
			public void TearDown()
			{
				Application.ClearCurrent();
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void MergedResourcesAreFound(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.label.TextColor);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void NoConflictsBetweenSharedRDs(bool useCompiledXaml)
			{
				var layout = new TestSharedResourceDictionary(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.label.TextColor);
				Assert.Equal(Colors.Purple, layout.label2.TextColor);
			}

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