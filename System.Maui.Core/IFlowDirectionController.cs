namespace System.Maui
{
	internal interface IFlowDirectionController
	{
		EffectiveFlowDirection EffectiveFlowDirection { get; set; }

		double Width { get; }

		bool ApplyEffectiveFlowDirectionToChildContainer { get; }
	}
}