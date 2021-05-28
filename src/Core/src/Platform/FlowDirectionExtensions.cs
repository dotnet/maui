namespace Microsoft.Maui
{
	public static class FlowDirectionExtensions
	{
		public static bool IsRightToLeft(this FlowDirection flowDirection)
		{
			return (flowDirection & FlowDirection.RightToLeft) == FlowDirection.RightToLeft;
		}

		public static bool IsLeftToRight(this FlowDirection flowDirection)
		{
			return (flowDirection & FlowDirection.RightToLeft) != FlowDirection.RightToLeft;
		}
	}
}