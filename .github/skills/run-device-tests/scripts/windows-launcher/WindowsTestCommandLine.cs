using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Maui.TestUtils.WindowsTesting;

internal static class WindowsTestCommandLine
{
	public static string Format(IEnumerable<string> arguments) => string.Join(" ", arguments.Select(Quote));

	public static string Quote(string argument)
	{
		ArgumentNullException.ThrowIfNull(argument);
		if (argument.Contains('\0', StringComparison.Ordinal))
			throw new ArgumentException("A Windows argument cannot contain NUL.", nameof(argument));

		var result = new StringBuilder("\"");
		var backslashes = 0;
		foreach (var character in argument)
		{
			if (character == '\\')
			{
				backslashes++;
				continue;
			}
			result.Append('\\', character == '"' ? backslashes * 2 + 1 : backslashes);
			result.Append(character);
			backslashes = 0;
		}
		result.Append('\\', backslashes * 2);
		return result.Append('"').ToString();
	}

	public static string[] ParseActivation(string arguments)
	{
		if (!OperatingSystem.IsWindows())
			throw new PlatformNotSupportedException("CommandLineToArgvW requires Windows.");

		// argv[0] has different quoting rules; activation only supplies arguments.
		var pointer = CommandLineToArgvW("maui-tests.exe " + arguments, out var count);
		if (pointer == IntPtr.Zero)
			throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
		try
		{
			var result = new string[Math.Max(0, count - 1)];
			for (var index = 1; index < count; index++)
				result[index - 1] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pointer, index * IntPtr.Size))!;
			return result;
		}
		finally
		{
			LocalFree(pointer);
		}
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	static extern IntPtr CommandLineToArgvW(string commandLine, out int argumentCount);

	[DllImport("kernel32.dll")]
	static extern IntPtr LocalFree(IntPtr memory);
}
