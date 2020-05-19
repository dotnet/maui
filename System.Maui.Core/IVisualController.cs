namespace System.Maui
{
	internal interface IVisualController
	{
		IVisual EffectiveVisual { get; set; }
		IVisual Visual { get; }
	}
}