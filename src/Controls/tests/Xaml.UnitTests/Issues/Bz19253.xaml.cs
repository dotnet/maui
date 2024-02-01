using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz19253 : ContentPage
	{
		public Bz19253()
		{
			InitializeComponent();
		}

		public Bz19253(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void RelativeSourceWithAncestorTypeShouldWork(bool useCompiledXaml)
			{
				var layout = new Bz19253(useCompiledXaml);

				var border = layout.border;
				var gradientStops = ((LinearGradientBrush)layout.border.Background).GradientStops;
				Assert.True(gradientStops[0].Color == border.BackgroundColor);
				Assert.True(gradientStops[1].Color == ((SolidColorBrush)border.Stroke).Color);
			}
		}
	}
}