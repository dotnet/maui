namespace Microsoft.Maui.Controls.Design
{

	/// <summary>
	/// Provides design-time type conversion for Easing values.
	/// </summary>
	public class EasingDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EasingDesignTypeConverter"/> class.
		/// </summary>
		public EasingDesignTypeConverter()
		{ }

		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
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
