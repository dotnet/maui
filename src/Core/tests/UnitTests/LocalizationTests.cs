using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Xml;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class LocalizationTests : IClassFixture<CultureFixture>, IClassFixture<GlobalLogSetup>
	{
		string _mauiRoot = Path.Combine("..", "..", "..", "..", "..");
		static string _globalFilePath = Path.Combine("localizationTestsOutput", "GlobalLog.txt");

		// Since one word phrases are sometimes not translated, we can filter them out if needed
		bool _ignoreOneWordPhrases = true;

		/// <summary>
		/// This test checks that all the .lcl files' <str> elements have a corresponding
		/// <tgt> element with their translations. Since there may be times that we have untranslated
		/// elements, instead of failing the test when something is missing, the strings without
		/// translations will be stored in a txt file for viewing: artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/ResxLocalizationMissingTargets.txt
		/// </summary>
		[Fact(Skip = "Skipped because doesn t work on helix")]
		public void ResxLocalizationStringsAreTranslated()
		{
			string lclFilePath = Path.Combine(_mauiRoot, "loc");
			string outputFilePath = Path.Combine("localizationTestsOutput", "ResxLocalizationMissingTargets.txt");

			// Ensure the directory exists
			var directoryPath = Path.GetDirectoryName(outputFilePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			var lclFiles = Directory.GetFiles(lclFilePath, "*.lcl", SearchOption.AllDirectories);

			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.WriteLine("This file is created by the ResxLocalizationStringsAreTranslated test.");
				writer.WriteLine("This test checks that all the strings in our .lcl files have a corresponding 'Target' tag.\n");

				foreach (var file in lclFiles)
				{
					var sb = new StringBuilder();
					var localizedKeys = LoadKeysFromLcl(file);

					foreach (var key in localizedKeys)
					{
						if (key.Value.Target is null)
						{
							var filePath = file.Replace(_mauiRoot, "", StringComparison.Ordinal);
							sb.AppendLine($"    Missing Target:");
							sb.AppendLine($"        Key: {key.Key}");
							sb.AppendLine($"        Source: {key.Value.Source}");
						}
					}

					if (sb.Length > 0)
					{
						writer.WriteLine($"File: {file}");
						writer.WriteLine(sb.ToString());
						WriteToGlobalLog($"File: {file}");
						WriteToGlobalLog(sb.ToString());
					}
				}
			}
		}

		/// <summary>
		/// This test checks that the strings inside the resx files are actually translated inside the assemblies
		/// when the locale is changed. It compares the actual value inside the assembly to the culture specific .lcl file.
		/// Strings that are not translated will be stored in a txt file for viewing:
		/// artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/ResxLocalizationTranslationsNotDisplayed_<locale>.txt
		/// NOTE: Link any new resx files as EmbeddedResource in the Core.UnitTests.csproj file.
		/// </summary>
		[Theory(Skip = "Skipped because doesn t work on helix")]
		[InlineData("cs", "cs-CZ")]
		[InlineData("de", "de-DE")]
		[InlineData("es", "es-ES")]
		[InlineData("fr", "fr-FR")]
		[InlineData("it", "it-IT")]
		[InlineData("ja", "ja-JP")]
		[InlineData("ko", "ko-KR")]
		[InlineData("pl", "pl-PL")]
		[InlineData("pt-BR", "pt-BR")]
		[InlineData("ru", "ru-RU")]
		[InlineData("tr", "tr-TR")]
		[InlineData("zh-Hans", "zh-Hans")]
		[InlineData("zh-Hant", "zh-Hant")]
		public void ResxLocalizationStringsAreDisplayedProperly(string cultureAbbreviation, string culture)
		{
			string lclFilePath = Path.Combine(_mauiRoot, "loc", cultureAbbreviation);

			var lclFiles = Directory.GetFiles(lclFilePath, "*.resx.lcl", SearchOption.AllDirectories);

			CultureInfo.CurrentCulture = new CultureInfo(culture);
			CultureInfo.CurrentUICulture = new CultureInfo(culture);

			var count = 0;

			var outputFilePath = Path.Combine("localizationTestsOutput", "ResxLocalizationTranslationsNotDisplayed_" + cultureAbbreviation + ".txt");

			// Ensure the directory exists
			var directoryPath = Path.GetDirectoryName(outputFilePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.WriteLine("This file is created by the ResxLocalizationStringsAreDisplayedProperly test.");
				writer.WriteLine("This test checks that the strings inside the resx files are actually translated inside the assemblies when the locale is changed. It compares the actual value inside the assembly to the culture specific .lcl file.");
				writer.WriteLine("Following lines in this file mean that those strings showing up as the 'actual' are what we see from the ResourceManager when using a different locale and we would expect them to match 'expected' string from the lcl file.\n");

				foreach (var file in lclFiles)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
					string resourceName = fileNameWithoutExtension.Split('.')[0];
					var filePath = file.Replace(_mauiRoot, "", StringComparison.Ordinal);
					var localizedKeys = LoadKeysFromLcl(file);
					var sb = new StringBuilder();

					// Check each key in the localization
					foreach (var key in localizedKeys)
					{
						if (key.Value.Target is not null)
						{
							// Get the actual translation from the assembly
							var keyName = key.Key[1..];
							string actualTranslation = GetLocalizedString(resourceName, keyName);

							// Compare with the expected translation in the .lcl file
							// if (key.Value.Target != actualTranslation)
							if (!IsDifferenceOnly5D(key.Value.Target, actualTranslation) && FilterOneWordPhrases(key.Value.Target))
							{
								sb.AppendLine($"    Key: {keyName}");
								sb.AppendLine($"        Expected: {key.Value.Target}");
								sb.AppendLine($"        Actual: {actualTranslation}");
							}
							else
							{
								count++;
							}
						}
					}

					if (sb.Length > 0)
					{
						writer.WriteLine($"File: {filePath}");
						writer.WriteLine(sb.ToString());
						WriteToGlobalLog($"File: {filePath}");
						WriteToGlobalLog(sb.ToString());
					}
				}

				writer.WriteLine($"Number of correctly displayed translated strings: {count}");
			}
		}

		/// <summary>
		/// This test compares the localized json translations with the standard json files and makes sure they are not equal.
		/// This test will not be able to compare if both strings are still english but just different, so it is not a perfect test.
		/// Strings that are not translated will be stored in a txt file for viewing:
		/// artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/JsonLocalizationTranslationsIncorrect_<locale>.txt
		/// </summary>
		[Theory]
		[InlineData("cs")]
		[InlineData("de")]
		[InlineData("es")]
		[InlineData("fr")]
		[InlineData("it")]
		[InlineData("ja")]
		[InlineData("ko")]
		[InlineData("pl")]
		[InlineData("pt-BR")]
		[InlineData("ru")]
		[InlineData("tr")]
		[InlineData("zh-Hans")]
		[InlineData("zh-Hant")]
		public void JsonLocalizationStringsAreTranslated(string culture)
		{
			var jsonfiles = GetFilesExcludingArtifacts(_mauiRoot, "*templatestrings.json");

			var outputFilePath = Path.Combine("localizationTestsOutput", "JsonLocalizationTranslationsIncorrect_" + culture + ".txt");

			// Ensure the directory exists
			var directoryPath = Path.GetDirectoryName(outputFilePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.WriteLine("This file is created by the JsonLocalizationStringsAreTranslated test.");
				writer.WriteLine("This test compares the localized json translations with the standard json files and makes sure they are not equal.");
				writer.WriteLine("This test will not be able to compare if both strings are still english but just different, so it is not a perfect test.\n");

				foreach (var file in jsonfiles)
				{
					string directory = Path.GetDirectoryName(file);
					string localizedJson = Path.Combine(directory, $"templatestrings.{culture}.json");

					if (!File.Exists(localizedJson))
					{
						writer.WriteLine($"*** File does not exist!!: {localizedJson}");
						WriteToGlobalLog($"*** File does not exist!!: {localizedJson}");
						return;
					}

					var originalJson = File.ReadAllText(file);
					var localizedJsonContent = File.ReadAllText(localizedJson);

					using (JsonDocument originalDoc = JsonDocument.Parse(originalJson))
					using (JsonDocument localizedDoc = JsonDocument.Parse(localizedJsonContent))
					{
						var path = string.Empty;
						var original = originalDoc.RootElement;
						var localized = localizedDoc.RootElement;

						var sb = new StringBuilder();

						foreach (JsonProperty property in original.EnumerateObject())
						{
							if (localized.TryGetProperty(property.Name, out JsonElement localizedProperty))
							{
								if (localizedProperty.GetRawText() == property.Value.GetRawText() && property.Name != "author" && FilterOneWordPhrases(localizedProperty.GetRawText()))
								{
									sb.AppendLine($"    String not translated:");
									sb.AppendLine($"        Name: {property.Name}");
									sb.AppendLine($"        Value: {localizedProperty}");
								}
							}
							else
							{
								sb.AppendLine($"    Missing property in localized file: Name: {property.Name}");
							}
						}

						if (sb.Length > 0)
						{
							writer.WriteLine($"File: {GetRelativePathFromSrc(localizedJson)}");
							writer.WriteLine(sb.ToString());
							WriteToGlobalLog($"File: {GetRelativePathFromSrc(localizedJson)}");
							WriteToGlobalLog(sb.ToString());
						}
					}
				}
			}
		}

		/// <summary>
		/// This test looks at the keys in the standard json files and makes sure that those keys are in the corresponding lcl file.
		/// Keys that are not included will be stored in a txt file for viewing:
		/// artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/LclFilesMissingJsonKeys_<locale>.txt
		/// </summary>
		[Theory(Skip = "Skipped because doesn t work on helix")]
		[InlineData("cs")]
		[InlineData("de")]
		[InlineData("es")]
		[InlineData("fr")]
		[InlineData("it")]
		[InlineData("ja")]
		[InlineData("ko")]
		[InlineData("pl")]
		[InlineData("pt-BR")]
		[InlineData("ru")]
		[InlineData("tr")]
		[InlineData("zh-Hans")]
		[InlineData("zh-Hant")]
		public void LclFilesMissingJsonKeys(string culture)
		{
			var jsonfiles = GetFilesExcludingArtifacts(_mauiRoot, "*templatestrings.json");

			string lclFilePath = Path.Combine(_mauiRoot, "loc", culture);
			var lclFiles = GetFilesExcludingArtifacts(lclFilePath, "*.json.lcl");

			var outputFilePath = Path.Combine("localizationTestsOutput", "LclFilesMissingJsonKeys_" + culture + ".txt");

			// Ensure the directory exists
			var directoryPath = Path.GetDirectoryName(outputFilePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.WriteLine("This file is created by the LclFilesMissingJsonKeys test.");
				writer.WriteLine("This test looks at the keys in the standard json files and makes sure that those keys are in the corresponding lcl file.\n");

				foreach (var file in jsonfiles)
				{
					var sb = new StringBuilder();

					string directory = Path.GetDirectoryName(file);

					while (directory is not null)
					{
						if (Path.GetFileName(directory) == ".template.config")
						{
							directory = Directory.GetParent(directory)?.FullName;
							break;
						}

						directory = Directory.GetParent(directory)?.FullName;
					}

					if (directory is null)
					{
						throw new Exception("Could not find the directory above '.template.config'");
					}

					directory = Path.GetFileName(directory);

					var correspondingLclFile = string.Empty;

					foreach (var lcl in lclFiles)
					{
						if (lcl.Contains(directory, StringComparison.Ordinal))
						{
							correspondingLclFile = lcl;
							break;
						}
					}

					if (correspondingLclFile == string.Empty)
					{
						writer.WriteLine($"*** No corresponding lcl file for: {directory}\n");
						WriteToGlobalLog($"*** No corresponding lcl file for: {directory}\n");
						continue;
					}

					var originalJson = File.ReadAllText(file);

					using (JsonDocument originalDoc = JsonDocument.Parse(originalJson))
					{
						foreach (JsonProperty property in originalDoc.RootElement.EnumerateObject())
						{
							var lclKeys = LoadKeysFromLcl(correspondingLclFile);
							var isFound = false;

							foreach (var lclKey in lclKeys)
							{
								if (lclKey.Key == ";" + property.Name)
								{
									isFound = true;
									break;
								}
							}

							if (!isFound)
							{
								sb.AppendLine($"    Key not found in lcl file: {property.Name}");
							}
						}
					}

					if (sb.Length > 0)
					{
						writer.WriteLine($"Json File: {GetRelativePathFromSrc(file)}");
						writer.WriteLine($"Lcl File: {correspondingLclFile}");
						writer.WriteLine(sb.ToString());
						WriteToGlobalLog($"Json File: {GetRelativePathFromSrc(file)}");
						WriteToGlobalLog($"Lcl File: {correspondingLclFile}");
						WriteToGlobalLog(sb.ToString());
					}
				}
			}
		}

		public Dictionary<string, (string Source, string Target)> LoadKeysFromLcl(string filePath)
		{
			var translations = new Dictionary<string, (string Source, string Target)>();
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(filePath);

			var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
			namespaceManager.AddNamespace("lcx", "http://schemas.microsoft.com/locstudio/2006/6/lcx");

			var itemNodes = xmlDoc.SelectNodes("//lcx:Item", namespaceManager);

			foreach (XmlNode itemNode in itemNodes)
			{
				var itemId = itemNode.Attributes["ItemId"]?.Value;

				var sourceValue = itemNode.SelectSingleNode("lcx:Str/lcx:Val", namespaceManager)?.InnerText.Trim();
				var targetValue = itemNode.SelectSingleNode("lcx:Str/lcx:Tgt/lcx:Val", namespaceManager)?.InnerText.Trim();

				if (!string.IsNullOrEmpty(itemId) && sourceValue is not null)
				{
					translations[itemId] = (sourceValue, targetValue);
				}
			}

			return translations;
		}

		string GetLocalizedString(string resourceString, string key)
		{
			var resourceManager = new ResourceManager("Core.UnitTests.Resources." + resourceString, typeof(LocalizationTests).Assembly);
			return resourceManager.GetString(key, CultureInfo.CurrentUICulture);
		}

		static string GetRelativePathFromSrc(string path)
		{
			var srcDirectory = "src";
			var index = path.IndexOf(srcDirectory, StringComparison.Ordinal);

			if (index >= 0)
			{
				return path.Substring(index);
			}

			return path;
		}

		private static void WriteToGlobalLog(string message)
		{
			// Ensure the directory exists
			var directoryPath = Path.GetDirectoryName(_globalFilePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (var writer = new StreamWriter(_globalFilePath, append: true))
			{
				writer.WriteLine(message);
			}
		}

		bool IsDifferenceOnly5D(string expected, string actual)
		{
			var expectedWithout5D = string.Join("", expected.Split(new[] { "5D;" }, StringSplitOptions.None));
			var actualWithout5D = string.Join("", actual.Split(new[] { "5D;" }, StringSplitOptions.None));

			return expectedWithout5D == actualWithout5D;
		}

		bool FilterOneWordPhrases(string phrase)
		{
			var words = phrase.Split(' ');

			if (words.Length == 1 && _ignoreOneWordPhrases)
			{
				return false;
			}

			return true;
		}

		string[] GetFilesExcludingArtifacts(string root, string searchPattern) =>
			Directory.GetFiles(root, searchPattern, SearchOption.AllDirectories)
				.Where(file => !file.Contains("/artifacts/", StringComparison.Ordinal))
				.ToArray();
	}

	public class CultureFixture : IDisposable
	{
		CultureInfo _originalCulture;
		CultureInfo _originalUICulture;

		public CultureFixture()
		{
			// Save the current culture and UI culture
			_originalCulture = CultureInfo.CurrentCulture;
			_originalUICulture = CultureInfo.CurrentUICulture;
		}

		public void Dispose()
		{
			// Restore the original culture and UI culture
			CultureInfo.CurrentCulture = _originalCulture;
			CultureInfo.CurrentUICulture = _originalUICulture;
		}
	}

	public class GlobalLogSetup : IDisposable
	{
		string _globalFilePath = Path.Combine("localizationTestsOutput", "GlobalLog.txt");

		public GlobalLogSetup()
		{
			// Delete the global log file if it exists
			if (File.Exists(_globalFilePath))
			{
				File.Delete(_globalFilePath);
			}
		}

		public void Dispose()
		{
		}
	}
}
