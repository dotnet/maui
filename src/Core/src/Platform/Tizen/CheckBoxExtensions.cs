using ElmSharp;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this Check nativeCheck, ICheckBox check)
		{
			nativeCheck.IsChecked = check.IsChecked;
		}
	}
}