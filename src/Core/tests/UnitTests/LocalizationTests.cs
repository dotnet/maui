using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
		static string _globalFilePath = Path.Combine(AppContext.BaseDirectory, "localizationTestsOutput", "GlobalLog.txt");

		// Since one word phrases are sometimes not translated, we can filter them out if needed
		bool _ignoreOneWordPhrases = true;

		/// <summary>
		/// This test checks that all the .lcl files' <str> elements have a corresponding
		/// <tgt> element with their translations. Since there may be times that we have untranslated
		/// elements, instead of failing the test when something is missing, the strings without
		/// translations will be stored in a txt file for viewing: artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/ResxLocalizationMissingTargets.txt
		/// LCL files are packaged with the tests so no repository checkout is required on Helix.
		/// </summary>
		[Fact]
		public void ResxLocalizationStringsAreTranslated()
		{
			string lclFilePath = GetLocalizationRoot();
			string outputFilePath = Path.Combine(AppContext.BaseDirectory, "localizationTestsOutput", "ResxLocalizationMissingTargets.txt");

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
		[Theory]
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
			string localizationRoot = GetLocalizationRoot();
			string lclFilePath = Path.Combine(localizationRoot, cultureAbbreviation);

			var lclFiles = Directory.GetFiles(lclFilePath, "*.resx.lcl", SearchOption.AllDirectories);
			Assert.NotEmpty(lclFiles);

			CultureInfo.CurrentCulture = new CultureInfo(culture);
			CultureInfo.CurrentUICulture = new CultureInfo(culture);

			var count = 0;

			var outputFilePath = Path.Combine(AppContext.BaseDirectory, "localizationTestsOutput", "ResxLocalizationTranslationsNotDisplayed_" + cultureAbbreviation + ".txt");

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
					var filePath = Path.GetRelativePath(localizationRoot, file);
					var localizedKeys = LoadKeysFromLcl(file);
					var sb = new StringBuilder();
					var translatedKeys = localizedKeys.Where(key => key.Value.Target is not null).ToArray();
					Assert.NotEmpty(translatedKeys);
					var resourceManager = new ResourceManager("Core.UnitTests.Resources." + resourceName, typeof(LocalizationTests).Assembly);
					Assert.NotNull(resourceManager.GetResourceSet(new CultureInfo(cultureAbbreviation), createIfNotExists: true, tryParents: false));

					// Check each key in the localization
					foreach (var key in translatedKeys)
					{
						// Get the actual translation from the assembly
						var keyName = key.Key[1..];
						string actualTranslation = resourceManager.GetString(keyName, CultureInfo.CurrentUICulture);
						Assert.NotNull(actualTranslation);

						// Compare with the expected translation in the .lcl file
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
		/// Template localization files are copied to the test output so the test also runs from an isolated Helix payload.
		/// The packaged baseline count must match the source count captured in the test assembly at build time.
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
			var templateLocalizationRoot = Path.Combine(AppContext.BaseDirectory, "TemplateLocalization");
			var jsonfiles = GetTemplateLocalizationBaselines();

			var outputFilePath = Path.Combine(AppContext.BaseDirectory, "localizationTestsOutput", "JsonLocalizationTranslationsIncorrect_" + culture + ".txt");

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

					Assert.True(File.Exists(localizedJson), $"Localized template strings file does not exist: {localizedJson}");

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
							var relativePath = Path.GetRelativePath(templateLocalizationRoot, localizedJson);
							writer.WriteLine($"File: {relativePath}");
							writer.WriteLine(sb.ToString());
							WriteToGlobalLog($"File: {relativePath}");
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
		public void LclFilesMissingJsonKeys(string culture)
		{
			var templateLocalizationRoot = Path.Combine(AppContext.BaseDirectory, "TemplateLocalization");
			var jsonfiles = GetTemplateLocalizationBaselines();
			string lclFilePath = Path.Combine(GetLocalizationRoot(), culture, "src", "Templates", "src", "templates");

			var outputFilePath = Path.Combine(AppContext.BaseDirectory, "localizationTestsOutput", "LclFilesMissingJsonKeys_" + culture + ".txt");

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

					var relativePath = Path.GetRelativePath(templateLocalizationRoot, file);
					var correspondingLclFile = Path.Combine(lclFilePath, relativePath + ".lcl");
					Assert.True(File.Exists(correspondingLclFile), $"No corresponding LCL file for '{relativePath}' in '{culture}': {correspondingLclFile}");

					var originalJson = File.ReadAllText(file);
					var lclKeys = LoadKeysFromLcl(correspondingLclFile);

					using (JsonDocument originalDoc = JsonDocument.Parse(originalJson))
					{
						foreach (JsonProperty property in originalDoc.RootElement.EnumerateObject())
						{
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
						writer.WriteLine($"Json File: {relativePath}");
						writer.WriteLine($"Lcl File: {correspondingLclFile}");
						writer.WriteLine(sb.ToString());
						WriteToGlobalLog($"Json File: {relativePath}");
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

			Assert.NotEmpty(translations);
			return translations;
		}

		static string[] GetTemplateLocalizationBaselines()
		{
			var root = Path.Combine(AppContext.BaseDirectory, "TemplateLocalization");
			Assert.True(Directory.Exists(root), $"Template localization files were not copied to '{root}'.");
			var files = Directory.GetFiles(root, "templatestrings.json", SearchOption.AllDirectories);
			Assert.NotEmpty(files);
			var metadata = Assert.Single(
				typeof(LocalizationTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>(),
				attribute => attribute.Key == "TemplateLocalizationBaselineCount");
			Assert.Equal(int.Parse(metadata.Value, CultureInfo.InvariantCulture), files.Length);
			return files;
		}

		static string GetLocalizationRoot()
		{
			var root = Path.Combine(AppContext.BaseDirectory, "Localization");
			Assert.True(Directory.Exists(root), $"LCL localization files were not copied to '{root}'.");
			var files = Directory.GetFiles(root, "*.lcl", SearchOption.AllDirectories);
			Assert.NotEmpty(files);
			var metadata = Assert.Single(
				typeof(LocalizationTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>(),
				attribute => attribute.Key == "LocalizationFileCount");
			Assert.Equal(int.Parse(metadata.Value, CultureInfo.InvariantCulture), files.Length);

			return root;
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
		string _globalFilePath = Path.Combine(AppContext.BaseDirectory, "localizationTestsOutput", "GlobalLog.txt");

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
