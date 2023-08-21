// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Design
{

	public class EasingDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public EasingDesignTypeConverter()
		{ }

		protected override bool ExclusiveToKnownValues => true;

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
