using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Issue3076Button : Button
	{
		public static readonly BindableProperty VerticalContentAlignmentProperty =
			BindableProperty.Create("VerticalContentAlignemnt", typeof(TextAlignment), typeof(Issue3076Button), TextAlignment.Center);

		public TextAlignment VerticalContentAlignment
		{
			get { return (TextAlignment)GetValue(VerticalContentAlignmentProperty); }
			set { SetValue(VerticalContentAlignmentProperty, value); }
		}
	}

	public partial class Issue3076 : ContentPage
	{
		public Issue3076()
		{
			InitializeComponent();
		}

		public Issue3076(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void CanUseBindableObjectDefinedInThisAssembly(bool useCompiledXaml)
			{
				var layout = new Issue3076(useCompiledXaml);

				Assert.IsType<Issue3076Button>(layout.local);
				Assert.Equal(TextAlignment.Start, layout.local.VerticalContentAlignment);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void CanUseBindableObjectDefinedInOtherAssembly(bool useCompiledXaml)
			{
				var layout = new Issue3076(useCompiledXaml);

				Assert.IsType<Microsoft.Maui.Controls.ControlGallery.Issue3076Button>(layout.controls);
				Assert.Equal(TextAlignment.Start, layout.controls.HorizontalContentAlignment);
			}
		}
	}
}