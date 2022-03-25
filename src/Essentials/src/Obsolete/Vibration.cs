#nullable enable
using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Vibration']/Docs" />
	public static class Vibration
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][1]/Docs" />
		public static void Vibrate() =>
			Current.Vibrate();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][2]/Docs" />
		public static void Vibrate(double duration) =>
			Current.Vibrate(duration);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][3]/Docs" />
		public static void Vibrate(TimeSpan duration) =>
			Current.Vibrate(duration);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Cancel']/Docs" />
		public static void Cancel() =>
			Current.Cancel();

		static IVibration Current => Devices.Vibration.Default;
	}
}
