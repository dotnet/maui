using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Resources;
using Xunit;


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
		/// translations will be stored in a txt file for viewing: artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/LocalizationMissingTargets.txt
		/// </summary>
		[Fact]
		public void LocalizationStringsAreTranslated()
		{
			string lclFilePath = Path.Combine(_mauiRoot, "loc");
			string outputFilePath = Path.Combine("localizationTestsOutput", "LocalizationMissingTargets.txt");

			// Ensure the directory exists
			var directoryPath = Path.GetDirectoryName(outputFilePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			var lclFiles = Directory.GetFiles(lclFilePath, "*.lcl", SearchOption.AllDirectories);

			using (var writer = new StreamWriter(outputFilePath))
			{
				foreach (var file in lclFiles)
				{
					var localizedKeys = LoadKeysFromLcl(file);

					foreach (var key in localizedKeys)
					{
						if (key.Value.Target is null)
						{
							var filePath = file.Replace(_mauiRoot, "", StringComparison.Ordinal);
							writer.WriteLine($"File: {filePath}, Key: {key.Key}, Source: {key.Value.Source}");
						}
					}
				}
			}
		}

		/// <summary>
		/// This test checks that the strings inside the resx files are actually translated inside the assemblies
		/// when the locale is changed. It compares the actual value inside the assembly to the culture specific .lcl file.
		/// Strings that are not translated will be stored in a txt file for viewing:
		/// artifacts/bin/Core.UnitTests/Debug/net9.0/localizationTestsOutput/LocalizationTranslationsNotDisplayed_<locale>.txt
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
		public void LocalizationStringsAreDisplayedProperly(string cultureAbbreviation, string culture)
		{
			string lclFilePath = Path.Combine(_mauiRoot, "loc", cultureAbbreviation);

			var lclFiles = Directory.GetFiles(lclFilePath, "*.resx.lcl", SearchOption.AllDirectories);

			CultureInfo.CurrentCulture = new CultureInfo(culture);
			CultureInfo.CurrentUICulture = new CultureInfo(culture);

			var count = 0;

			var outputFilePath = Path.Combine("localizationTestsOutput", "LocalizationTranslationsNotDisplayed_" + cultureAbbreviation + ".txt");

			// Ensure the directory exists
			var directoryPath = Path.GetDirectoryName(outputFilePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			using (var writer = new StreamWriter(outputFilePath))
			{
				foreach (var file in lclFiles)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
					string resourceName = fileNameWithoutExtension.Split('.')[0];
					var filePath = file.Replace(_mauiRoot, "", StringComparison.Ordinal);
					var localizedKeys = LoadKeysFromLcl(file);

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
								writer.WriteLine($"File: {filePath}, Key: {keyName}, Expected: {key.Value.Target}, Actual: {actualTranslation}");
							}
							else
							{
								count++;
							}
						}
					}
				}

				writer.WriteLine($"Number of correctly displayed translated strings: {count}");
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
