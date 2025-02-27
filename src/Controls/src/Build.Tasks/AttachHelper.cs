using System;
using System.Diagnostics;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	static class AttachHelper
	{
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		private static extern int MessageBox(int hWnd, String text, String caption, uint type);

		public static bool ShouldAttach { get; set; }// = true;
		public static void AttachToProcessIfEnabled()
		{
			if (ShouldAttach)
			{
				using Process currentProcess = Process.GetCurrentProcess();
				string title = $"{currentProcess.ProcessName} - Attach";
				MessageBox(0, $"Attach debugger now! PID = {currentProcess.Id}", title, 0);
				ShouldAttach = false;
			}
		}
	}
}
