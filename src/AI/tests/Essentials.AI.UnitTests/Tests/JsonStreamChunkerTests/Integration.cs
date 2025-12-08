using System.Text.Json;
using Maui.Controls.Sample.Services;
using Microsoft.Maui.Essentials.AI.UnitTests.TestHelpers;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonStreamChunkerTests
{
	/// <summary>
	/// Integration tests using real JSONL test data files.
	/// </summary>
	public class IntegrationTests
	{
		[Theory]
		[MemberData(nameof(DataStreamsHelper.JsonlItineraries), MemberType = typeof(DataStreamsHelper))]
		public void Process_FromJsonlFile_ProducesValidJsonMatchingFinalLine(string fileName, string filePath)
		{
			_ = fileName; // We are not using the parameter here

			// Arrange
			var chunker = new JsonStreamChunker();
			var lines = File.ReadAllLines(filePath);

			// Act
			var chunks = new List<string>();
			foreach (var line in lines)
				chunks.Add(chunker.Process(line));
			chunks.Add(chunker.Flush());

			// For debugging - write chunks to .txt file
			var txtPath = Path.ChangeExtension(filePath, ".txt");
			File.WriteAllLines(txtPath, chunks);

			var concatenated = string.Concat(chunks);
			var finalLine = lines[^1];

			// Assert
			// 1. Concatenated chunks should be valid JSON
			JsonDocument concatenatedDoc;
			try
			{
				concatenatedDoc = JsonDocument.Parse(concatenated);
			}
			catch (JsonException ex)
			{
				Assert.Fail($"Concatenated chunks are not valid JSON: {ex.Message}\n\nConcatenated:\n{concatenated}");
				return;
			}

			// 2. Should match the final line when parsed
			var finalDoc = JsonDocument.Parse(finalLine);

			// Deep equality check
			var areEqual = JsonElementsAreEqual(finalDoc.RootElement, concatenatedDoc.RootElement);
			Assert.True(areEqual, 
				$"Concatenated JSON does not match final line.\n\nExpected:\n{finalLine}\n\nActual:\n{concatenated}");
		}
	}
}
