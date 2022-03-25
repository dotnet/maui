namespace Microsoft.Maui
{
	public static class IViewExtensions
	{
		public static FlowDirection GetEffectiveFlowDirection(this IView view) 
		{
			if (view.FlowDirection != FlowDirection.MatchParent)
			{
				return view.FlowDirection;
			}

			// If the FlowDirection is MatchParent, then ask the Parent, if available
			if (view.Parent is IView parentView)
			{
				return parentView.GetEffectiveFlowDirection();
			}

			// If there's no parent, try asking the App; failing that, fall back to LTR
			return view.Handler?.MauiContext?.GetFlowDirection() ?? FlowDirection.LeftToRight;
		}
	}
}
