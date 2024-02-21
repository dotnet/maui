#nullable enable
using System;
using System.IO;
using System.Text;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public class TestLogger : TextWriter
	{
		public TestLogger()
		{
		}

		public override void Write(char value)
		{
			Console.Write(value);
			System.Diagnostics.Debug.Write(value);
		}

		public override void WriteLine(string? value)
		{
			Console.WriteLine(value);
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override Encoding Encoding => Encoding.Default;
	}
}