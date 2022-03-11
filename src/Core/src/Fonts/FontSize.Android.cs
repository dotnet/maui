using System;
using System.Collections.Generic;
using System.Text;
using Android.Util;

namespace Microsoft.Maui
{
	public readonly struct FontSize
	{
		public FontSize(float value, ComplexUnitType unit)
		{
			Value = value;
			Unit = unit;
		}

		public float Value { get; }
		public ComplexUnitType Unit { get; }
	}
}
