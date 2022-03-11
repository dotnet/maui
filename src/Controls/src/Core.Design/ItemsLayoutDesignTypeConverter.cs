namespace Microsoft.Maui.Controls.Design
{
	public class ItemsLayoutDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public ItemsLayoutDesignTypeConverter()
		{
		}

		protected override string[] KnownValues
			=> new [] { "VerticalList", "HorizontalList", "VerticalGrid", "HorizontalGrid" };
	}
}