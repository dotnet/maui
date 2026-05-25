namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for items layout values.
	/// </summary>
	public class ItemsLayoutDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ItemsLayoutDesignTypeConverter"/> class.
		/// </summary>
		public ItemsLayoutDesignTypeConverter()
		{
		}

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] { "VerticalList", "HorizontalList", "VerticalGrid", "HorizontalGrid" };
	}
}