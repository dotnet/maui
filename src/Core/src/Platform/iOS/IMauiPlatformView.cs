namespace Microsoft.Maui.Platform;

internal interface IMauiPlatformView
{
	void InvalidateAncestorsMeasuresWhenMovedToWindow();
	void InvalidateMeasure(bool isPropagating = false);
}