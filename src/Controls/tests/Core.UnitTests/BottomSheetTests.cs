using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BottomSheetTests : BaseTestFixture
	{
		[Theory]
		[InlineData(0)]
		[InlineData(-0.1)]
		[InlineData(1.01)]
		[InlineData(double.NaN)]
		[InlineData(1e-9)]
		public void FractionOutOfRangeThrows(double fraction) =>
			Assert.Throws<ArgumentOutOfRangeException>(() => BottomSheetDetent.Fraction(fraction));

		[Theory]
		[InlineData(0)]
		[InlineData(-10)]
		[InlineData(double.NaN)]
		[InlineData(double.PositiveInfinity)]
		[InlineData(0.0004)]
		public void InvalidHeightThrows(double height) =>
			Assert.Throws<ArgumentOutOfRangeException>(() => BottomSheetDetent.Height(height));

		[Fact]
		public void DetentsHaveValueEquality()
		{
			Assert.Equal(BottomSheetDetent.Fraction(0.4), BottomSheetDetent.Fraction(0.4));
			Assert.True(BottomSheetDetent.Height(300) == BottomSheetDetent.Height(300));
			Assert.Equal(BottomSheetDetent.Height(300).GetHashCode(), BottomSheetDetent.Height(300).GetHashCode());

			// Same value, different meaning.
			Assert.NotEqual(BottomSheetDetent.Fraction(0.5), BottomSheetDetent.Medium);
			Assert.NotEqual(BottomSheetDetent.Fraction(1), BottomSheetDetent.Large);
			Assert.True(BottomSheetDetent.Medium != null);
		}

		[Theory]
		[InlineData("Medium")]
		[InlineData("Large")]
		[InlineData("FitContent")]
		[InlineData("40%")]
		[InlineData("33.5%")]
		[InlineData("300")]
		[InlineData("120.5")]
		public void ConverterRoundTrips(string value)
		{
			var converter = new BottomSheetDetentTypeConverter();
			var detent = (BottomSheetDetent)converter.ConvertFromInvariantString(value);

			Assert.Equal(value, converter.ConvertToInvariantString(detent));
			Assert.Equal(detent, converter.ConvertFromInvariantString(detent.ToString()));
		}

		[Fact]
		public void ConverterParsesAllForms()
		{
			var converter = new BottomSheetDetentTypeConverter();

			Assert.Same(BottomSheetDetent.Medium, converter.ConvertFromInvariantString(" medium "));
			Assert.Same(BottomSheetDetent.Large, converter.ConvertFromInvariantString("LARGE"));
			Assert.Same(BottomSheetDetent.FitContent, converter.ConvertFromInvariantString("fitcontent"));
			Assert.Equal(BottomSheetDetent.Fraction(0.25), converter.ConvertFromInvariantString("25 %"));
			Assert.Equal(BottomSheetDetent.Fraction(1), converter.ConvertFromInvariantString("100%"));
			Assert.Equal(BottomSheetDetent.Height(250), converter.ConvertFromInvariantString("250"));
		}

		[Fact]
		public void ConverterIgnoresCulture()
		{
			var previous = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = new CultureInfo("nl-NL");
				var converter = new BottomSheetDetentTypeConverter();
				Assert.Equal(BottomSheetDetent.Fraction(0.125), converter.ConvertFromInvariantString("12.5%"));
				Assert.Equal("12.5%", BottomSheetDetent.Fraction(0.125).ToString());
			}
			finally
			{
				CultureInfo.CurrentCulture = previous;
			}
		}

		[Theory]
		[InlineData("0.25")]
		[InlineData("0%")]
		[InlineData("150%")]
		[InlineData("Small")]
		[InlineData("")]
		[InlineData("%")]
		[InlineData("NaN")]
		[InlineData("NaN%")]
		public void ConverterRejectsInvalidValues(string value) =>
			Assert.Throws<FormatException>(() => new BottomSheetDetentTypeConverter().ConvertFromInvariantString(value));

		[Fact]
		public void CollectionConverterParsesList()
		{
			var converter = new BottomSheetDetentCollectionTypeConverter();
			var detents = (IReadOnlyList<BottomSheetDetent>)converter.ConvertFromInvariantString("30%, Medium,Large");

			Assert.Equal(new[] { BottomSheetDetent.Fraction(0.3), BottomSheetDetent.Medium, BottomSheetDetent.Large }, detents);
			Assert.Equal("30%, Medium, Large", converter.ConvertToInvariantString(detents));
		}

		[Theory]
		[InlineData("")]
		[InlineData(" , ")]
		[InlineData("Medium, Tiny")]
		public void CollectionConverterRejectsInvalidValues(string value) =>
			Assert.Throws<FormatException>(() => new BottomSheetDetentCollectionTypeConverter().ConvertFromInvariantString(value));

		[Fact]
		public void AttachedPropertyDefaults()
		{
			var page = new ContentPage();

			Assert.False(BottomSheet.GetIsEnabled(page));
			Assert.Equal(new[] { BottomSheetDetent.Large }, BottomSheet.GetDetents(page));
			Assert.Null(BottomSheet.GetSelectedDetent(page));
			Assert.True(BottomSheet.GetHasDragHandle(page));
			Assert.True(BottomSheet.GetIsDismissible(page));
			Assert.Equal(-1, BottomSheet.GetCornerRadius(page));
			Assert.Equal(BindingMode.TwoWay, BottomSheet.SelectedDetentProperty.DefaultBindingMode);
		}

		[Fact]
		public void DetentsAcceptOneToThreeEntries()
		{
			var page = new ContentPage();

			BottomSheet.SetDetents(page, new[] { BottomSheetDetent.Fraction(0.25), BottomSheetDetent.Medium, BottomSheetDetent.Large });
			Assert.Equal(3, BottomSheet.GetDetents(page).Count);

			Assert.Throws<ArgumentException>(() => BottomSheet.SetDetents(page, Array.Empty<BottomSheetDetent>()));
			Assert.Throws<ArgumentException>(() => BottomSheet.SetDetents(page, null));
			Assert.Throws<ArgumentException>(() => BottomSheet.SetDetents(page, new BottomSheetDetent[] { BottomSheetDetent.Medium, null }));
			Assert.Throws<ArgumentException>(() => BottomSheet.SetDetents(page, new[]
			{
				BottomSheetDetent.Height(100), BottomSheetDetent.Fraction(0.25), BottomSheetDetent.Medium, BottomSheetDetent.Large
			}));

			Assert.Equal(3, BottomSheet.GetDetents(page).Count);

			// Like other bindable properties, an invalid value from XAML or a binding is ignored.
			page.SetValue(BottomSheet.DetentsProperty, Array.Empty<BottomSheetDetent>());
			Assert.Equal(3, BottomSheet.GetDetents(page).Count);
		}

		[Theory]
		[InlineData(-1, true)]
		[InlineData(0, true)]
		[InlineData(24, true)]
		[InlineData(-2, false)]
		[InlineData(double.PositiveInfinity, false)]
		[InlineData(double.NaN, false)]
		public void CornerRadiusValidation(double radius, bool valid)
		{
			var page = new ContentPage();

			if (valid)
			{
				BottomSheet.SetCornerRadius(page, radius);
				Assert.Equal(radius, BottomSheet.GetCornerRadius(page));
			}
			else
			{
				Assert.Throws<ArgumentException>(() => BottomSheet.SetCornerRadius(page, radius));
			}
		}

		[Fact]
		public void ResolverOrdersByHeight()
		{
			var stops = BottomSheetDetentResolver.Resolve(
				new[] { BottomSheetDetent.Large, BottomSheetDetent.Height(100), BottomSheetDetent.Medium },
				available: 800,
				measureContent: null,
				tolerance: 1);

			Assert.Equal(new double[] { 100, 400, 800 }, stops.ConvertAll(s => s.Height));
			Assert.Equal(BottomSheetDetent.Height(100), stops[0].Detents[0]);
			Assert.Equal(BottomSheetDetent.Medium, stops[1].Detents[0]);
			Assert.Equal(BottomSheetDetent.Large, stops[2].Detents[0]);
		}

		[Fact]
		public void ResolverClampsToAvailableHeightAndMergesCoincidingDetents()
		{
			// Content taller than the screen: FitContent and Large are the same resting position.
			var stops = BottomSheetDetentResolver.Resolve(
				new[] { BottomSheetDetent.FitContent, BottomSheetDetent.Height(2000), BottomSheetDetent.Large },
				available: 800,
				measureContent: () => 1200,
				tolerance: 1);

			var stop = Assert.Single(stops);
			Assert.Equal(800, stop.Height);
			Assert.Equal(new[] { BottomSheetDetent.FitContent, BottomSheetDetent.Height(2000), BottomSheetDetent.Large }, stop.Detents);

			// The selected detent is reported while it rests there; otherwise the first declared one.
			Assert.Equal(BottomSheetDetent.Large, stop.ReportedDetent(BottomSheetDetent.Large));
			Assert.Equal(BottomSheetDetent.FitContent, stop.ReportedDetent(null));
			Assert.Equal(BottomSheetDetent.FitContent, stop.ReportedDetent(BottomSheetDetent.Medium));
		}

		[Fact]
		public void ResolverMeasuresContentOnce()
		{
			int measured = 0;
			var stops = BottomSheetDetentResolver.Resolve(
				new[] { BottomSheetDetent.FitContent, BottomSheetDetent.FitContent, BottomSheetDetent.Large },
				available: 800,
				measureContent: () => { measured++; return 250; },
				tolerance: 1);

			Assert.Equal(1, measured);
			Assert.Equal(new double[] { 250, 800 }, stops.ConvertAll(s => s.Height));
		}

		[Fact]
		public void ResolverUsesAvailableHeightForFitContentWithoutMeasurement()
		{
			var stops = BottomSheetDetentResolver.Resolve(new[] { BottomSheetDetent.FitContent }, 600, null, 1);
			Assert.Equal(600, Assert.Single(stops).Height);
		}

		[Fact]
		public void IndexOfFindsTheStopOfTheSelectedDetent()
		{
			var stops = BottomSheetDetentResolver.Resolve(
				new[] { BottomSheetDetent.Fraction(0.25), BottomSheetDetent.Medium, BottomSheetDetent.Large }, 800, null, 1);

			Assert.Equal(0, BottomSheetDetentResolver.IndexOf(stops, null));
			Assert.Equal(1, BottomSheetDetentResolver.IndexOf(stops, BottomSheetDetent.Medium));
			Assert.Equal(2, BottomSheetDetentResolver.IndexOf(stops, BottomSheetDetent.Large));
			Assert.Equal(0, BottomSheetDetentResolver.IndexOf(stops, BottomSheetDetent.Fraction(0.25)));

			// A detent the sheet doesn't have isn't found, so the sheet stays where it is.
			Assert.Equal(-1, BottomSheetDetentResolver.IndexOf(stops, BottomSheetDetent.Height(123)));
		}

		[Fact]
		public void ResolverMergesDetentsWithinTolerance()
		{
			var stops = BottomSheetDetentResolver.Resolve(
				new[] { BottomSheetDetent.Height(400.9), BottomSheetDetent.Medium, BottomSheetDetent.Height(402) },
				available: 800,
				measureContent: null,
				tolerance: 1);

			Assert.Equal(2, stops.Count);
			Assert.Equal(new[] { BottomSheetDetent.Medium, BottomSheetDetent.Height(400.9) }, stops[0].Detents);
			Assert.Equal(BottomSheetDetent.Height(402), Assert.Single(stops[1].Detents));
		}

		[Fact]
		public void FitContentOfEmptyContentStillHasAHeight()
		{
			Assert.Equal(1, BottomSheetDetentResolver.ResolveHeight(BottomSheetDetent.FitContent, 800, () => 0));
			Assert.Equal(800, BottomSheetDetentResolver.ResolveHeight(BottomSheetDetent.FitContent, 800, () => double.PositiveInfinity));
			Assert.Equal(800, BottomSheetDetentResolver.ResolveHeight(BottomSheetDetent.FitContent, 800, () => double.NaN));
		}

		[Fact]
		public void FractionsAndHeightsFromXamlEqualTheSameValuesInCode()
		{
			var converter = new BottomSheetDetentTypeConverter();

			Assert.Equal(BottomSheetDetent.Fraction(0.333), converter.ConvertFromInvariantString("33.3%"));
			Assert.Equal(BottomSheetDetent.Fraction(0.123), converter.ConvertFromInvariantString("12.3%"));
			Assert.Equal(BottomSheetDetent.Height(250.4), converter.ConvertFromInvariantString("250.4"));
		}

		[Fact]
		public void DetentsAreCopiedWhenSet()
		{
			var page = new ContentPage();
			var detents = new List<BottomSheetDetent> { BottomSheetDetent.Medium, BottomSheetDetent.Large };

			BottomSheet.SetDetents(page, detents);
			detents.Clear();

			Assert.Equal(new[] { BottomSheetDetent.Medium, BottomSheetDetent.Large }, BottomSheet.GetDetents(page));
		}

		[Fact]
		public void ResolverReturnsNothingUsableWithoutSpace()
		{
			Assert.Equal(0, BottomSheetDetentResolver.ResolveHeight(BottomSheetDetent.Large, 0, null));
			Assert.Equal(0, BottomSheetDetentResolver.ResolveHeight(BottomSheetDetent.Medium, double.NaN, null));
		}
	}
}
