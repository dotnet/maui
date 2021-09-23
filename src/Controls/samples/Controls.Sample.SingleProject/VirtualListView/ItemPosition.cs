namespace Microsoft.Maui
{
	public struct ItemPosition
	{
		public ItemPosition(int sectionIndex = 0, int itemIndex = 0)
		{
			SectionIndex = sectionIndex;
			ItemIndex = itemIndex;
		}

		public int SectionIndex { get; }
		public int ItemIndex { get; }
	}
}