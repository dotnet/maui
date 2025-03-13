namespace Microsoft.Maui.Platform;

internal interface IPlatformMeasureInvalidationController
{
	void InvalidateAncestorsMeasuresWhenMovedToWindow();
	void InvalidateMeasure(bool isPropagating = false);
}