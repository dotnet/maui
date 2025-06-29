using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz56852
	{
		public Bz56852()
		{
			InitializeComponent();
		}

		public Bz56852(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Tests
		{
			public void TearDown()
			{
				Application.Current = null;
			}

			[InlineData(true)]]
			[InlineData(false)]]
			public void DynamicResourceApplyingOrder(bool useCompiledXaml)
			{
				var layout = new Bz56852(useCompiledXaml);
				Assert.Equal(50, layout.label.FontSize);
			}
		}
	}
}
