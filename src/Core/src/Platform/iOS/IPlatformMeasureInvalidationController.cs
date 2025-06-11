namespace Microsoft.Maui.Platform;

internal interface IPlatformMeasureInvalidationController
{
	void InvalidateAncestorsMeasuresWhenMovedToWindow();
	bool InvalidateMeasure(bool isPropagating = false);
}