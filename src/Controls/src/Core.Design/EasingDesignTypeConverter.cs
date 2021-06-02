namespace Microsoft.Maui.Controls.Design
{

	public class EasingDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public EasingDesignTypeConverter()
		{ }

		protected override string[] KnownValues
			=> new string[]
			{
				"Linear",
				"SinOut",
				"SinIn",
				"SinInOut",
				"CubicIn",
				"CubicOut",
				"CubicInOut",
				"BounceOut",
				"BounceIn",
				"SpringIn",
				"SpringOut"
			};
	}
}
