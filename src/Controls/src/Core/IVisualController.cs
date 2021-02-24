namespace Microsoft.Maui.Controls
{
	internal interface IVisualController
	{
		IVisual EffectiveVisual { get; set; }
		IVisual Visual { get; }
	}
}