namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for Visual values.
	/// </summary>
	public class VisualDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VisualDesignTypeConverter"/> class.
		/// </summary>
		public VisualDesignTypeConverter()
		{
		}

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new string[] { "Default" /*, "Material" */ };
	}
}