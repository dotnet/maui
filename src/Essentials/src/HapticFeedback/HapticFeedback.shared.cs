using System;
using System.ComponentModel;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Devices
{
	public interface IHapticFeedback
	{
		bool IsSupported { get; }
		
		void Perform(HapticFeedbackType type);
	}
}
namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="Type[@FullName='Microsoft.Maui.Essentials.HapticFeedback']/Docs" />
	public static partial class HapticFeedback
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="//Member[@MemberName='Perform']/Docs" />
		public static void Perform(HapticFeedbackType type = HapticFeedbackType.Click)
		{
			if (!Current.IsSupported)
				throw new FeatureNotSupportedException();
			Current.Perform(type);
		}

#nullable enable
		static IHapticFeedback? currentImplementation;

		public static IHapticFeedback Current =>
			currentImplementation ??= new HapticFeedbackImplementation();

		internal static void SetCurrent(IHapticFeedback? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
