using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Mono.Options;
using Xamarin.Forms.Core.UITests;
using Xamarin.Forms.Loader;

namespace Xamarin.Forms.UITest.TestCloud
{
	// TODO: Provide way to construct url for results easy access

	static class Uploader
	{
		static LoaderActions loaderActions;

		static int Main(string[] args)
		{
			loaderActions = new LoaderActions();

			var categories = new List<string>();
			string series = null;
			var platform = DeviceSet.Platform.None;
			DeviceSet deviceSet = null;
			var validate = false;
			string outputFile = null;
			var account = "";
			var user = "";

			OptionSet optionSet = null;
			optionSet = new OptionSet
			{
				{
					"p|platform=", "specify the test platform, iOS or Android",
					s => platform = (DeviceSet.Platform)Enum.Parse(typeof (DeviceSet.Platform), s)
				},
				{ "d|deviceset=", "the device set to use for the test run", s => deviceSet = StringToDeviceSet(s) },
				{ "c|category=", "add a category to the test run", s => categories.Add(s) },
				{ "s|series=", "specify the series when uploaded to Test Cloud", s => series = s },
				{ "l|list", "list categories available in test suite", ListCategories },
				{ "sets", "list available device sets", ListDeviceSets },
				{ "i|interactive", "start uploader in interactive mode", InteractiveMode },
				{ "h|help", "show this message and exit", s => ShowHelp(optionSet) },
				{ "v|validate", "validate all tests or a specified category", s => validate = true },
				{ "o|output=", "output destination for NUnit XML", s => outputFile = s },
				{ "a|account=", "Test Cloud key", s => account = s },
				{ "u|user=", "Test Cloud user", s => user = s }
			};

			List<string> extra;
			try
			{
				extra = optionSet.Parse(args);
			}
			catch (OptionException ex)
			{
				Console.Write("Uploader:");
				Console.WriteLine(ex.Message);
				Console.WriteLine("Try --help for more informaiton");
			}

			if (args.Length == 0)
				ShowHelp(optionSet);

			if (validate)
			{
				var category = categories.FirstOrDefault();
				return loaderActions.ValidateCategory(category) ? 0 : 1;
			}

			if (platform == DeviceSet.Platform.None)
			{
				Console.WriteLine("Platform must be specified");
				return 1;
			}

			if (deviceSet != null && !deviceSet.DeviceSetPlatform.Contains(platform))
			{
				Console.WriteLine("DeviceSet platform does not match specified platform");
				return 1;
			}

			if (deviceSet == null)
			{
				if (platform == DeviceSet.Platform.Android)
					deviceSet = DeviceSets.AndroidFastParallel;
				else
					deviceSet = DeviceSets.IOsFastParallel;
			}

			var execString = BuildExecutionString(platform, deviceSet, categories, series, account, user, outputFile);

			Console.WriteLine(execString);

			var processStatus = TestCloudUtils.UploadApp(execString);

			Console.WriteLine("test-cloud.exe status: " + processStatus);
			return processStatus;
		}

		static string BuildExecutionString(DeviceSet.Platform platform, DeviceSet deviceSet, IEnumerable<string> categories,
			string series, string account, string user, string outputFile = null)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(ConsolePath);
			stringBuilder.Append(" submit ");

			switch (platform)
			{
				case DeviceSet.Platform.Android:
					stringBuilder.Append(ApkPath);
					break;
				case DeviceSet.Platform.IOs:
					stringBuilder.Append(IpaPath);
					break;
				case DeviceSet.Platform.IOsClassic:
					stringBuilder.Append(IpaClassicPath);
					break;
			}

			stringBuilder.Append(" ");
			stringBuilder.Append(account);
			stringBuilder.Append(" --user ");
			stringBuilder.Append(user);
			stringBuilder.Append(" --devices ");
			stringBuilder.Append(deviceSet.Id);

			foreach (var category in categories)
			{
				stringBuilder.Append(" --include ");
				stringBuilder.Append(category);
			}

			if (!string.IsNullOrEmpty(series))
			{
				stringBuilder.Append(" --series ");
				stringBuilder.Append(series);
			}

			stringBuilder.Append(" --locale \"en_US\"");

			switch (platform)
			{
				case DeviceSet.Platform.Android:
					stringBuilder.Append(" --app-name \"AndroidControlGallery\"");
					break;
				case DeviceSet.Platform.IOs:
				case DeviceSet.Platform.IOsClassic:
					stringBuilder.Append(" --app-name \"XamControl\"");
					break;
			}

			stringBuilder.Append(" --assembly-dir ");

			if (platform == DeviceSet.Platform.Android)
				stringBuilder.Append(AndroidTestingDirectory);
			else
				stringBuilder.Append(iOSTestingDirectory);

			stringBuilder.Append(" --fixture-chunk");

			if (!string.IsNullOrEmpty(outputFile))
				stringBuilder.Append($" --nunit-xml {outputFile}");

			return stringBuilder.ToString();
		}

		static void InteractiveMode(string s)
		{
			Console.WriteLine("Interactive testcloud uploader. Type --help for help");
			Console.WriteLine(
				"Usage: >>> -d <deviceset> -c <category> -a <key> -u <user> [-c <category> -c <category> -c <category>]");

			while (true)
			{
				Console.Write(">>> ");
				var command = Console.ReadLine();
				var commandList = command.Split(' ');

				var platform = DeviceSet.Platform.None;
				var deviceSet = "";
				var categories = new List<string>();
				var series = "";
				var account = "";
				var user = "";

				OptionSet options = null;
				options = new OptionSet
				{
					{ "q|quit", "quit", Exit },
					{ "h|help", "show this message and exit", str => ShowInteractiveHelp(options) },
					{ "c|category=", "specify the category to run in Test cloud", str => categories.Add(str) },
					{ "d|deviceset=", "specify the device set to upload", str => deviceSet = str },
					{ "lc|listcategories", "Lists categories in uitests", ListCategories },
					{ "ld|listdevicesets", "Lists defined devices sets", ListDeviceSets },
					{ "a|account=", "Test Cloud key", str => account = str },
					{ "u|user=", "Test Cloud user", str => user = str }
				};

				List<string> extra;
				try
				{
					extra = options.Parse(commandList);
				}
				catch (OptionException ex)
				{
					Console.Write("Uploader:");
					Console.WriteLine(ex.Message);
					Console.WriteLine("Try --help for more informaiton");
				}

				if (command.Length == 0)
					ShowHelp(options);

				// by default take the first category as the series name
				if (categories.Count >= 1)
					series = categories.First();

				if (commandList.Length >= 4)
				{
					var validQuery = true;

					if (!IsValidDeviceSet(deviceSet))
					{
						Console.WriteLine("Invalid DeviceSet: {0}", deviceSet);
						validQuery = false;
					}

					if (!CategoriesValid(categories))
					{
						Console.Write("Invalid Category(s):");
						foreach (var c in categories)
							Console.Write(" {0} ", c);
						Console.Write("\n");
						validQuery = false;
					}

					if (validQuery)
					{
						var devSet = StringToDeviceSet(deviceSet);
						var execString = BuildExecutionString(devSet.DeviceSetPlatform.First(), devSet, categories, series, account, user);
						Console.WriteLine(execString);
						TestCloudUtils.UploadApp(execString);
					}
				}
			}
		}

		static void ShowInteractiveHelp(OptionSet options)
		{
			Console.WriteLine("Usage: [OPTIONS]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			options.WriteOptionDescriptions(Console.Out);
		}

		static void Exit(string s)
		{
			Environment.Exit(0);
		}

		static void ShowHelp(OptionSet options)
		{
			Console.WriteLine("Usage: Uploader [OPTIONS]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			options.WriteOptionDescriptions(Console.Out);

			Environment.Exit(0);
		}

		static DeviceSet StringToDeviceSet(string s)
		{
			try
			{
				var device = (DeviceSet)typeof (DeviceSets).GetField(s).GetValue(null);
				return device;
			}
			catch (Exception ex)
			{
				Console.WriteLine("DeviceSet not found");
				return null;
			}
		}

		static void ListDeviceSets(string s)
		{
			var deviceSetsType = typeof (DeviceSets);

			var fields = deviceSetsType.GetFields();

			foreach (var field in fields)
				Console.WriteLine(field.Name);
		}

		static void ListCategories(string s)
		{
			loaderActions.ListCategories();
		}

		static bool CategoriesValid(List<string> categories)
		{
			var areValid = true;

			if (categories.Count < 1)
				return false;

			foreach (var category in categories)
			{
				if (!loaderActions.ValidateCategory(category))
					return false;
			}

			return areValid;
		}

		static bool IsValidDeviceSet(string deviceSet)
		{
			var deviceSetsType = typeof (DeviceSets);
			var isValid = deviceSetsType.GetFields().Any(ds => ds.Name == deviceSet);
			return isValid;
		}

		public static string ConsolePath
		{
			get
			{
				string[] consolePathElements = { "..", "..", "..", "packages", "Xamarin.UITest.1.3.7", "tools", "test-cloud.exe" };
				return Path.Combine(consolePathElements);
			}
		}

		public static string IpaPath
		{
			get
			{
				string[] ipaPathElements =
				{
					"..",
					"..",
					"..",
					"Xamarin.Forms.ControlGallery.iOS",
					"bin",
					"iPhone",
					"Debug",
					"XamarinFormsControlGalleryiOS-1.0.ipa"
				};
				return Path.Combine(ipaPathElements);
			}
		}

		public static string IpaClassicPath
		{
			get
			{
				string[] ipaPathElements =
				{
					"..",
					"..",
					"..",
					"Xamarin.Forms.ControlGallery.iOS",
					"classic_bin",
					"iPhone",
					"Debug",
					"XamarinFormsControlGalleryiOS-1.0.ipa"
				};
				return Path.Combine(ipaPathElements);
			}
		}

		public static string ApkPath
		{
			get
			{
				string[] apkPathElements =
				{
					"..",
					"..",
					"..",
					"Xamarin.Forms.ControlGallery.Android",
					"bin",
					"Debug",
					"AndroidControlGallery.AndroidControlGallery-Signed.apk"
				};
				return Path.Combine(apkPathElements);
			}
		}

		public static string iOSTestingDirectory
		{
			get
			{
				string[] testDiriOSPathElements = { "..", "..", "..", "Xamarin.Forms.Core.iOS.UITests", "bin", "Debug" };
				return Path.Combine(testDiriOSPathElements);
			}
		}

		public static string AndroidTestingDirectory
		{
			get
			{
				string[] testDirAndroidPathElements = { "..", "..", "..", "Xamarin.Forms.Core.Android.UITests", "bin", "Debug" };
				return Path.Combine(testDirAndroidPathElements);
			}
		}
	}
}
