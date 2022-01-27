using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Flashlight']/Docs" />
	public static partial class Flashlight
	{
		static Task PlatformTurnOnAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformTurnOffAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
