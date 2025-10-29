using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz55347 : ContentPage
	{
		public Bz55347()
		{
		}

		public Bz55347(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown]
			[Xunit.Fact]
			public void TearDown()
			{
				Application.Current = null;
			}

			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void PaddingThicknessResource(bool useCompiledXaml)
			{
				Application.Current = new MockApplication
				{
					Resources = new ResourceDictionary {
						{"Padding", new Thickness(8)}
					}
				};
				var layout = new Bz55347(useCompiledXaml);
			}
		}
	}
}