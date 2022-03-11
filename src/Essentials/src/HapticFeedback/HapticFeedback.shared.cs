using System;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IHapticFeedback
	{
		bool IsSupported { get; }
		
		void Perform(HapticFeedbackType type);
	}

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
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IHapticFeedback Current =>
			currentImplementation ??= new HapticFeedbackImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IHapticFeedback? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
