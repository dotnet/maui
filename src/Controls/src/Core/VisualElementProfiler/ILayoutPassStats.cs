namespace Microsoft.Maui.Controls;

internal interface ILayoutPassStats
{
	ILayoutPassTypeStats this[LayoutPassType type] { get; }
}