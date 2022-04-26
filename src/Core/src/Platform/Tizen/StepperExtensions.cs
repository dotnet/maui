using ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class StepperExtensions
	{
		public static void UpdateMinimum(this Spinner nativeStepper, IStepper stepper)
		{
			nativeStepper.Minimum = stepper.Minimum;
		}

		public static void UpdateMaximum(this Spinner nativeStepper, IStepper stepper)
		{
			nativeStepper.Maximum = stepper.Maximum;
		}

		public static void UpdateIncrement(this Spinner nativeStepper, IStepper stepper)
		{
			var increment = stepper.Interval;

			if (increment > 0)
			{
				nativeStepper.LabelFormat = string.Format("%.{0}f", GetRequiredPrecision(increment));
				nativeStepper.Step = stepper.Interval;
			}
		}

		public static void UpdateValue(this Spinner nativeStepper, IStepper stepper)
		{
			if (nativeStepper.Value != stepper.Value)
				nativeStepper.Value = stepper.Value;
		}

		static int GetRequiredPrecision(double step)
		{
			// Determines how many decimal places are there in current Stepper's value.
			// The 15 pound characters below correspond to the maximum precision of Double type.
			var decimalValue = decimal.Parse(step.ToString("0.###############"));

			// GetBits() method returns an array of four 32-bit integer values.
			// The third (0-indexing) element of an array contains the following information:
			//     bits 00-15: unused, required to be 0
			//     bits 16-23: an exponent between 0 and 28 indicating the power of 10 to divide the integer number passed as a parameter.
			//                 Conversely this is the number of decimal digits in the number as well.
			//     bits 24-30: unused, required to be 0
			//     bit     31: indicates the sign. 0 means positive number, 1 is for negative numbers.
			//
			// The precision information needs to be extracted from bits 16-23 of third element of an array
			// returned by GetBits() call. Right-shifting by 16 bits followed by zeroing anything else results
			// in a nice conversion of this data to integer variable.
			var precision = (decimal.GetBits(decimalValue)[3] >> 16) & 0x000000FF;

			return precision;
		}
	}
}