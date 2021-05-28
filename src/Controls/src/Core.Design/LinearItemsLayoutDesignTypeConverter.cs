namespace Microsoft.Maui.Controls.Design
{
	public class LinearItemsLayoutDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public LinearItemsLayoutDesignTypeConverter()
		{
		}

		protected override string[] KnownValues
			=> new string[] { "Vertical", "Horizontal" };
	}
}