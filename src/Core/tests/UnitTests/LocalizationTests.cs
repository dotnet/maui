using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Resources;
using Xunit;
using System.Text.Json;
using System.Text;


namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class LocalizationTests : IClassFixture<CultureFixture>
	{
		string _mauiRoot = Path.Combine("..", "..", "..", "..", "..");

		/// <summary>
		/// This test checks that all the .lcl files' <str> elements have a corresponding
		/// <tgt> element with their translations. Since there may be times that we have untranslated
		/// elements, instead of failing the test when something is missing, the strings without
		/// translations will be stored in a txt file for viewing: artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/ResxLocalizationMissingTargets.txt
		/// </summary>
		[Fact]
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
							if (key.Value.Target != actualTranslation)
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
			var jsonfiles = Directory.GetFiles(_mauiRoot, "*templatestrings.json", SearchOption.AllDirectories);

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
							string propertyPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
							if (localized.TryGetProperty(property.Name, out JsonElement localizedProperty))
							{
								if (localizedProperty.GetRawText() == property.Value.GetRawText())
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
							writer.WriteLine($"File: {localizedJson}");
							writer.WriteLine(sb.ToString());
						}
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

		private string GetLocalizedString(string resourceString, string key)
		{
			var resourceManager = new ResourceManager("Core.UnitTests.Resources." + resourceString, typeof(LocalizationTests).Assembly);
			return resourceManager.GetString(key, CultureInfo.CurrentUICulture);
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
}
