namespace CollectionViewPerformanceMaui.Enums
{
	public enum Template
	{
		Card, // Baseline

        CardWithShadow,
        CardWithElevation, // Android only
        CardWithCornerRadius,
		CardWithBindableLayout,
		CardWithTapGesture,
		CardWithGrid,

		CardWithTheLot, // Worst case scenario

		CardWithComplexContent
    }
}
