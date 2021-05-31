namespace Microsoft.Maui.Controls.Design
{
	public class LayoutOptionsDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public LayoutOptionsDesignTypeConverter()
		{
		}

		protected override string[] KnownValues
			=> new string[] { "Start", "Center", "End", "Fill", "StartAndExpand", "CenterAndExpand", "EndAndExpand", "FillAndExpand" };
	}
}