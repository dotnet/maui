namespace Microsoft.Maui.Controls.Diagnostics
{
	internal static class NativeBottomNavigationSelection
	{
		public static int GetMenuItemId(
			int selectedIndex,
			int itemCount,
			int maxVisibleItems,
			int moreItemId)
		{
			if (selectedIndex < 0 ||
				selectedIndex >= itemCount ||
				maxVisibleItems <= 0)
			{
				return -1;
			}

			return itemCount > maxVisibleItems &&
				selectedIndex >= maxVisibleItems - 1
					? moreItemId
					: selectedIndex;
		}
	}
}
