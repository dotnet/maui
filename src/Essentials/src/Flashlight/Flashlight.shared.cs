#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
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
			Current.TurnOnAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="//Member[@MemberName='TurnOffAsync']/Docs" />
		public static Task TurnOffAsync() =>
			Current.TurnOffAsync();

		static IFlashlight? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IFlashlight Current =>
			currentImplementation ??= new FlashlightImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IFlashlight? implementation) =>
			currentImplementation = implementation;
	}
}
