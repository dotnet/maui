using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;
using static Microsoft.Maui.IntegrationTests.TemplateTests;

namespace Microsoft.Maui.IntegrationTests
{
	public class WarningsPerFile
	{
		public string File { get; set; } = string.Empty;
		public List<WarningsPerCode> WarningsPerCode { get; set; } = new List<WarningsPerCode>();
	}

	public class WarningsPerCode
	{
		public string Code { get; set; } = string.Empty;
		public List<string> Messages { get; set; } = new List<string>();
	}

	public static class BuildWarningsUtilities
	{
		public static List<WarningsPerFile> ExpectedNativeAOTWarnings
		{
			get => expectedNativeAOTWarnings;
		}

		// We rely on the fact that expected file paths are stored as relative to the repo root (e.g., src/Core/...).
		// While the actual file paths are always full paths and can have different repo roots (e.g., building locally or on CI).
		private static bool CompareWarningsFilePaths(this string actual, string expected) => actual.Contains(expected, StringComparison.Ordinal);

		private static string NormalizeFilePath(string file) => file.Replace("\\\\", "/", StringComparison.Ordinal).Replace('\\', '/');

		public static List<WarningsPerFile> ReadNativeAOTWarningsFromBinLog(string binLogFilePath)
		{
			var actualWarnings = new List<WarningsPerFile>();
			foreach (var record in new BinLogReader().ReadRecords(binLogFilePath))
			{
				if (record.Args is BuildWarningEventArgs warning && !string.IsNullOrEmpty(warning.Message))
				{
					// We normalize all warnings file paths for easier comparison
					actualWarnings.AddActualWarning(NormalizeFilePath(warning.File), warning.Code, warning.Message);
				}
			}
			return actualWarnings;
		}

		private static void AddActualWarning(this List<WarningsPerFile> warnings, string file, string code, string message)
		{
			var warningsPerFile = warnings.FirstOrDefault(w => w.File == file);
			if (warningsPerFile is null)
			{
				var newEntry = new WarningsPerFile
				{
					File = file,
					WarningsPerCode = new List<WarningsPerCode>
						{
							new WarningsPerCode
							{
								Code = code,
								Messages = new List<string> { message }
							}
						}
				};
				warnings.Add(newEntry);
			}
			else
			{
				var warningsPerCode = warningsPerFile.WarningsPerCode.FirstOrDefault(w => w.Code == code);
				if (warningsPerCode is null)
				{
					var newEntry = new WarningsPerCode
					{
						Code = code,
						Messages = new List<string> { message }
					};
					warningsPerFile.WarningsPerCode.Add(newEntry);
				}
				else
				{
					warningsPerCode.Messages.Add(message);
				}
			}
		}

		public static void AssertWarnings(this List<WarningsPerFile> actualWarnings, List<WarningsPerFile> expectedWarnings)
		{
			foreach (var expectedWarningsPerFile in expectedWarnings)
			{
				var actualWarningsPerFile = actualWarnings.FirstOrDefault(actualWarning => actualWarning.File.CompareWarningsFilePaths(expectedWarningsPerFile.File));
				Assert.NotNull(actualWarningsPerFile,
					$"Expected warnings file path '{expectedWarningsPerFile.File}' was not found.");

				foreach (var expectedWarningsPerCode in expectedWarningsPerFile.WarningsPerCode)
				{
					var actualWarningsPerCode = actualWarningsPerFile!.WarningsPerCode.FirstOrDefault(x => x.Code == expectedWarningsPerCode.Code);
					Assert.NotNull(actualWarningsPerCode,
						$"Expected warning code '{expectedWarningsPerCode.Code}' was not found for the expected warnings file path '{expectedWarningsPerFile.File}'");

					foreach (var expectedWarningsMessage in expectedWarningsPerCode.Messages)
					{
						Assert.True(actualWarningsPerCode!.Messages.Remove(expectedWarningsMessage),
							$"Expected warning message '{expectedWarningsMessage}' was not found for the expected warnings file path '{expectedWarningsPerFile.File}' and warning code '{expectedWarningsPerCode.Code}'");
					}

					Assert.AreEqual(0, actualWarningsPerCode!.Messages.Count,
						$"Unexpected warning messages detected for the expected warnings file path '{expectedWarningsPerFile.File}' and warning code '{expectedWarningsPerCode.Code}'! Unexpected warning messages are: {string.Join("\n\t\t", actualWarningsPerCode.Messages)}");

					actualWarningsPerFile.WarningsPerCode.Remove(actualWarningsPerCode);
				}

				Assert.AreEqual(0, actualWarningsPerFile!.WarningsPerCode.Count,
					$"Unexpected warning codes detected for the expected warnings file path '{expectedWarningsPerFile.File}'! Unexpected warning codes are: {string.Join("\n\t\t", actualWarningsPerFile.WarningsPerCode.Select(c => c.Code).ToList())}");

				actualWarnings.Remove(actualWarningsPerFile!);
			}

			Assert.AreEqual(0, actualWarnings.Count,
				$"Unexpected warning files detected! Unexpected warning file paths are: {string.Join("\n\t\t", actualWarnings.Select(f => f.File).ToList())}");
		}

		#region Expected warning messages

		// IMPORTANT: Always store expected File information as a relative path to the repo ROOT
		private static readonly List<WarningsPerFile> expectedNativeAOTWarnings = new();

		#region Utility methods for generating the list of expected warnings

		// Use this method to regenerate warnings found in a .binlog file at 'binLogFilePath'.
		// Based on the results read from the .binlog file the method will output to the console
		// a definition and initialization of a private member "expectedNativeAOTWarnings" which can
		// further be used to update the definition of the member in this file.
		// Specifying 'repoRoot' will make storing warnings file paths relative to the repository path for easier comparison.
		public static void GenerateNewExpectedWarningsFromBinLog(string binLogFilePath, string? repoRoot = null)
		{
			var warnings = ReadNativeAOTWarningsFromBinLog(binLogFilePath);
			warnings.PrintNewExpectedWarnings(repoRoot: repoRoot);
		}

		private static void PrintNewWarningsPerCode(WarningsPerCode wpc, int indentSpaces = 0)
		{
			var indent = string.Empty.PadLeft(indentSpaces);
			Console.WriteLine(indent + "new WarningsPerCode");
			Console.WriteLine(indent + "{");
			Console.WriteLine(indent + "    Code = \"" + wpc.Code + "\",");
			Console.WriteLine(indent + "    Messages = new List<string>");
			Console.WriteLine(indent + "    {");
			foreach (var message in wpc.Messages)
			{
				Console.WriteLine(indent + "        \"" + message + "\",");
			}
			Console.WriteLine(indent + "    }");
			Console.WriteLine(indent + "},");
		}

		private static void PrintNewWarningsPerFile(WarningsPerFile wpf, int indentSpaces = 0, string? repoRoot = null)
		{
			var indent = string.Empty.PadLeft(indentSpaces);
			var file = wpf.File;
			if (!string.IsNullOrEmpty(repoRoot) && wpf.File.StartsWith(repoRoot))
				file = wpf.File.Substring(repoRoot.Length);

			Console.WriteLine(indent + "new WarningsPerFile");
			Console.WriteLine(indent + "{");
			Console.WriteLine(indent + "    File = \"" + file + "\",");
			Console.WriteLine(indent + "    WarningsPerCode = new List<WarningsPerCode>");
			Console.WriteLine(indent + "    {");
			foreach (var warningPerCode in wpf.WarningsPerCode)
			{
				PrintNewWarningsPerCode(warningPerCode, indentSpaces + 8);
			}
			Console.WriteLine(indent + "    }");
			Console.WriteLine(indent + "},");
		}

		private static void PrintNewExpectedWarnings(this List<WarningsPerFile> warnings, int indentSpaces = 0, string? repoRoot = null)
		{
			var indent = string.Empty.PadLeft(indentSpaces);
			Console.WriteLine(indent + $"private static readonly List<WarningsPerFile> expectedNativeAOTWarnings = new()");
			Console.WriteLine(indent + "{");
			foreach (var warning in warnings)
			{
				PrintNewWarningsPerFile(warning, indentSpaces + 4, repoRoot);
			}
			Console.WriteLine(indent + "};");
		}
		#endregion

		#endregion
	};
}
