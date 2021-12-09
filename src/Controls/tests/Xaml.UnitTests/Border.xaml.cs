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
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void InitializeStrokeShape(bool useCompiledXaml)
			{
				var layout = new Border(useCompiledXaml);
				Assert.NotNull(layout.border0.StrokeShape);
				Assert.NotNull(layout.border1.StrokeShape);
				Assert.NotNull(layout.border2.StrokeShape);
			}
		}
	}
}