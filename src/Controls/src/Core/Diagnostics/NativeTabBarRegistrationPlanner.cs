namespace Microsoft.Maui.Controls.Diagnostics
{
	internal static class NativeTabBarRegistrationPlanner
	{
		public static bool TryPlan(
			int logicalItemCount,
			int viewControllerCount,
			int tabBarItemCount,
			int realizedControlCount,
			bool lastItemIsMore,
			out int slotCount,
			out bool hasMore)
		{
			hasMore = viewControllerCount > tabBarItemCount && lastItemIsMore;
			slotCount = tabBarItemCount;

			if (logicalItemCount < 0 ||
				viewControllerCount < 0 ||
				tabBarItemCount <= 0 ||
				viewControllerCount < tabBarItemCount ||
				(viewControllerCount > tabBarItemCount && !lastItemIsMore))
			{
				return false;
			}

			var logicalSlotCount = hasMore ? tabBarItemCount - 1 : tabBarItemCount;
			return logicalSlotCount <= logicalItemCount &&
				realizedControlCount == tabBarItemCount;
		}
	}
}
