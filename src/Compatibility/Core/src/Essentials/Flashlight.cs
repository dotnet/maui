#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Flashlight']/Docs" />
	public static partial class Flashlight
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="//Member[@MemberName='TurnOnAsync']/Docs" />
		public static Task TurnOnAsync() =>
			Current.TurnOnAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="//Member[@MemberName='TurnOffAsync']/Docs" />
		public static Task TurnOffAsync() =>
			Current.TurnOffAsync();

		static IFlashlight Current => Devices.Flashlight.Current;
	}
}
