namespace Microsoft.Maui
{
	internal static class VirtualListViewExtensions
	{
		public static object DataFor(this IVirtualListViewAdapter vlva, PositionKind kind, int sectionIndex, int itemIndex)
			=> kind switch
			{
				PositionKind.Item => vlva.Item(sectionIndex, itemIndex),
				PositionKind.SectionHeader => vlva.Section(sectionIndex),
				PositionKind.SectionFooter => vlva.Section(sectionIndex),
				_ => default
			};
	}
}
