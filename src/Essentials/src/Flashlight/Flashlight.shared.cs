#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices
{
	public interface IFlashlight
	{
		Task TurnOnAsync();

		Task TurnOffAsync();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Flashlight']/Docs" />
	public static partial class Flashlight
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="//Member[@MemberName='TurnOnAsync']/Docs" />
		public static Task TurnOnAsync() =>
			Default.TurnOnAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="//Member[@MemberName='TurnOffAsync']/Docs" />
		public static Task TurnOffAsync() =>
			Default.TurnOffAsync();

		static IFlashlight? defaultImplementation;

		public static IFlashlight Default =>
			defaultImplementation ??= new FlashlightImplementation();

		internal static void SetDefault(IFlashlight? implementation) =>
			defaultImplementation = implementation;
	}
}
