namespace Microsoft.Maui.Controls.Design
{
	public class VisualDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public VisualDesignTypeConverter()
		{
		}

		protected override string[] KnownValues
			=> new string[] { "Default", "Material" };
	}
}