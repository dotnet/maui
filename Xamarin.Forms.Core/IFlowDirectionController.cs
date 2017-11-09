namespace Xamarin.Forms
{
	internal interface IFlowDirectionController
	{
		EffectiveFlowDirection EffectiveFlowDirection { get; set; }

		double Width { get; }

		void NotifyFlowDirectionChanged();
	}
}