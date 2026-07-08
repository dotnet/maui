using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static class IndexPathHelpers
	{
		public static NSIndexPath[] GenerateIndexPathRange(int section, int startIndex, int count)
		{
			var result = new NSIndexPath[count];

			for (int n = 0; n < count; n++)
			{
				result[n] = NSIndexPath.Create(section, startIndex + n);
			}

			return result;
		}

		public static NSIndexPath[] GenerateLoopedIndexPathRange(int section, int sectionCount, int iterations, int startIndex, int count)
		{
			var result = new NSIndexPath[iterations * count];
			var step = sectionCount / iterations;

			for (int r = 0; r < iterations; r++)
			{
				for (int n = 0; n < count; n++)
				{
					var index = startIndex + (r * step) + n;
					result[(r * count) + n] = NSIndexPath.Create(section, index);
				}
			}

			return result;
		}

		public static bool IsIndexPathValid(this IItemsViewSource source, NSIndexPath indexPath)
		{
			if (indexPath.Section >= source.GroupCount)
			{
				return false;
			}

			if (indexPath.Item >= source.ItemCountInGroup(indexPath.Section))
			{
				return false;
			}

			return true;
		}
	}
}
