using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BottomSheetProperties : ContentPage
{
	public BottomSheetProperties() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BottomSheetAttachedPropertiesAreSetFromXaml(XamlInflator inflator)
		{
			var page = new BottomSheetProperties(inflator) { BindingContext = new { Selected = BottomSheetDetent.Medium } };

			Assert.True(BottomSheet.GetIsEnabled(page));
			Assert.Equal(new[] { BottomSheetDetent.Fraction(0.3), BottomSheetDetent.Medium, BottomSheetDetent.Large }, BottomSheet.GetDetents(page));
			Assert.Equal(BottomSheetDetent.Medium, BottomSheet.GetSelectedDetent(page));
			Assert.False(BottomSheet.GetHasDragHandle(page));
			Assert.False(BottomSheet.GetIsDismissible(page));
			Assert.Equal(24, BottomSheet.GetCornerRadius(page));

			Assert.Equal(new[] { BottomSheetDetent.Height(250), BottomSheetDetent.FitContent }, BottomSheet.GetDetents(page.fitPage));
			Assert.Same(BottomSheetDetent.FitContent, BottomSheet.GetSelectedDetent(page.fitPage));
		}
	}
}
