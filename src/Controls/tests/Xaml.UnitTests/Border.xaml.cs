using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Border : ContentPage
	{
		public Border() => InitializeComponent();

		public Border(bool useCompiledXaml)
		{
			// This stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void InitializeStrokeShape(bool useCompiledXaml)
			{
				var layout = new Border(useCompiledXaml);
				Assert.NotNull(layout.Border0.StrokeShape);
				Assert.NotNull(layout.Border1.StrokeShape);
				Assert.NotNull(layout.Border2.StrokeShape);
			}
		}
	}
}