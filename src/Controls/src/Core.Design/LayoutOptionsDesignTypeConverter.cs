namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for LayoutOptions values.
	/// </summary>
	public class LayoutOptionsDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutOptionsDesignTypeConverter"/> class.
		/// </summary>
		public LayoutOptionsDesignTypeConverter()
		{
		}

		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new string[] { "Start", "Center", "End", "Fill", "StartAndExpand", "CenterAndExpand", "EndAndExpand", "FillAndExpand" };
	}
}