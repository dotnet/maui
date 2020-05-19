namespace Xamarin.Forms
{
	internal interface IVisualController
	{
		IVisual EffectiveVisual { get; set; }
		IVisual Visual { get; }
	}
}