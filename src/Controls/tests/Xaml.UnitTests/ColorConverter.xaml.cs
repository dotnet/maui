using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class ColorConverter : ContentPage
	{


		public ColorConverter()
		{
			InitializeComponent();
		}

		public ColorConverter(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void StringsAreValidAsColor(bool useCompiledXaml)
			{
				var page = new ColorConverter(useCompiledXaml);
				page.BindingContext = new ColorConverterVM();

				var expected = Color.FromArgb("#fc87ad");
				Assert.Equal(expected, page.Button0.BackgroundColor);
			}
		}
	}

	public class ColorConverterVM
	{
		public string ButtonBackground => "#fc87ad";
	}
}