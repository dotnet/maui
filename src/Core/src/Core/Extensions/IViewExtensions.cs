namespace Microsoft.Maui
{
	public static class IViewExtensions
	{
		// TODO ezhart Do the public api stuff to get rid of this
		public static FlowDirection GetEffectiveFlowDirection(this IView view)
		{
			return FlowDirection.LeftToRight;
		}
	}
}
