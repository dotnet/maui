using System;
using System.Runtime.InteropServices;
using ElmSharp;
using EEntry = ElmSharp.Entry;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	internal static class EntryExtensions
	{
		internal static InputPanelReturnKeyType ToInputPanelReturnKeyType(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return InputPanelReturnKeyType.Go;
				case ReturnType.Next:
					return InputPanelReturnKeyType.Next;
				case ReturnType.Send:
					return InputPanelReturnKeyType.Send;
				case ReturnType.Search:
					return InputPanelReturnKeyType.Search;
				case ReturnType.Done:
					return InputPanelReturnKeyType.Done;
				case ReturnType.Default:
					return InputPanelReturnKeyType.Default;
				default:
					throw new System.NotImplementedException($"ReturnType {returnType} not supported");
			}
		}
		public static void GetSelectRegion(this EEntry entry, out int start, out int end)
		{
			elm_entry_select_region_get(entry.RealHandle, out start, out end);
		}

		[DllImport("libelementary.so.1")]
		static extern void elm_entry_select_region_get(IntPtr obj, out int start, out int end);
	}
}
