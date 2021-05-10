namespace Microsoft.Maui.Controls.Design
{
	public class NamedSizeDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public NamedSizeDesignTypeConverter()
		{
		}

		protected override string[] KnownValues
			=> new[] { "Default", "Micro", "Small", "Medium", "Large", "Body", "Header", "Title", "Subtitle", "Caption" };
	}
}