namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for LinearItemsLayout values.
	/// </summary>
	public class LinearItemsLayoutDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinearItemsLayoutDesignTypeConverter"/> class.
		/// </summary>
		public LinearItemsLayoutDesignTypeConverter()
		{
		}

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new string[] { "Vertical", "Horizontal" };
	}
}