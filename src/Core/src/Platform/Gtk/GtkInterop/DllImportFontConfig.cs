using System.Runtime.InteropServices;

// ReSharper disable CA2211

// ReSharper disable InconsistentNaming

namespace Microsoft.Maui.GtkInterop
{

	public static class DllImportFontConfig
	{

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool d_FcConfigAppFontAddFile(System.IntPtr config, string fontPath);

		public static d_FcConfigAppFontAddFile FcConfigAppFontAddFile = FuncLoader.LoadFunction<d_FcConfigAppFontAddFile>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Fontconfig), "FcConfigAppFontAddFile"));

	}

}