using System;
using System.Diagnostics;

namespace Xamarin.Forms.UITest.TestCloud
{
	internal static class TestCloudUtils
	{
		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}

		public static Tuple<DeviceSet.Platform, string> ParseArgs(string input)
		{
			string[] deviceArgs = input.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			if (deviceArgs.Length != 2)
			{
				Console.WriteLine("!!! Invalid upload paramters, should be <Platform> <Category> : You entered : " + input);
				return null;
			}

			DeviceSet.Platform platform = deviceArgs[0] == "Android" ? DeviceSet.Platform.Android : DeviceSet.Platform.IOs;
			string category = deviceArgs[1];

			return Tuple.Create(platform, category);
		}

		public static int UploadApp(string command)
		{
			Tuple<string, string> execArgsPair = IsRunningOnMono() ? MonoExecArgs(command) : WindowsExecArgs(command);

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = execArgsPair.Item1,
					Arguments = execArgsPair.Item2,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};

			try
			{
				process.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return 1;
			}

			while (!process.StandardOutput.EndOfStream)
			{
				string line = process.StandardOutput.ReadLine();
				Console.WriteLine(line);
			}
			while (!process.StandardError.EndOfStream)
			{
				string line = process.StandardError.ReadLine();
				Console.WriteLine(line);
			}
			process.WaitForExit();
			return process.ExitCode;
		}

		static Tuple<string, string> MonoExecArgs(string command)
		{
			return Tuple.Create("mono", command);
		}

		static Tuple<string, string> WindowsExecArgs(string command)
		{
			string[] commandArray = command.Split(' ');
			string executable = commandArray[0];
			string uploadCommand = string.Join(" ", commandArray, 1, commandArray.Length - 1);
			return Tuple.Create(executable, uploadCommand);
		}
	}
}