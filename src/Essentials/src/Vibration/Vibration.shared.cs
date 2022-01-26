using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Vibration']/Docs" />
	public static partial class Vibration
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][0]/Docs" />
		public static void Vibrate()
			=> Vibrate(TimeSpan.FromMilliseconds(500));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][1]/Docs" />
		public static void Vibrate(double duration)
			=> Vibrate(TimeSpan.FromMilliseconds(duration));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][2]/Docs" />
		public static void Vibrate(TimeSpan duration)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (duration.TotalMilliseconds < 0)
				duration = TimeSpan.Zero;
			else if (duration.TotalSeconds > 5)
				duration = TimeSpan.FromSeconds(5);

			PlatformVibrate(duration);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Cancel']/Docs" />
		public static void Cancel()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			PlatformCancel();
		}
	}
}
