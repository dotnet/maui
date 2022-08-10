using System;

namespace Microsoft.Maui
{
	public static class IViewExtensions
	{
		[Obsolete($"This method is obsolete as of 7.0. Please use {nameof(IView.EffectiveFlowDirection)} instead.")]
		public static FlowDirection GetEffectiveFlowDirection(this IView view)
		{
			return view.EffectiveFlowDirection;
		}
	}
}
