using System;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IVibration
	{
		bool IsSupported { get; }

		void Vibrate();

		void Vibrate(double duration);

		void Vibrate(TimeSpan duration);

		void Cancel();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Vibration']/Docs" />
	public static partial class Vibration
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][1]/Docs" />
		public static void Vibrate()
			=> Current.Vibrate(TimeSpan.FromMilliseconds(500));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][2]/Docs" />
		public static void Vibrate(double duration)
			=> Current.Vibrate(TimeSpan.FromMilliseconds(duration));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Vibrate'][3]/Docs" />
		public static void Vibrate(TimeSpan duration)
		{
			if (!Current.IsSupported)
				throw new FeatureNotSupportedException();

			if (duration.TotalMilliseconds < 0)
				duration = TimeSpan.Zero;
			else if (duration.TotalSeconds > 5)
				duration = TimeSpan.FromSeconds(5);

			Current.Vibrate(duration);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="//Member[@MemberName='Cancel']/Docs" />
		public static void Cancel()
		{
			if (!Current.IsSupported)
				throw new FeatureNotSupportedException();

			Current.Cancel();
		}

#nullable enable
		static IVibration? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IVibration Current =>
			currentImplementation ??= new VibrationImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IVibration? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
