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
		static IFlashlight? currentImplementation;

		public static IFlashlight Current =>
			currentImplementation ??= new FlashlightImplementation();

		internal static void SetCurrent(IFlashlight? implementation) =>
			currentImplementation = implementation;
	}
}
