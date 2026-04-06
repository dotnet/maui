using System.Text.Json;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Helper methods shared across JsonStreamChunker test classes.
/// </summary>
public partial class JsonStreamChunkerTests
{
	/// <summary>
	/// Checks if a string is valid JSON.
	/// </summary>
	public static bool IsValidJson(string json)
	{
		try
		{
			JsonDocument.Parse(json);
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Performs deep equality comparison of two JsonElements.
	/// Property order is ignored for objects.
	/// </summary>
	public static bool JsonElementsAreEqual(JsonElement expected, JsonElement actual)
	{
		if (expected.ValueKind != actual.ValueKind)
			return false;

		switch (expected.ValueKind)
		{
			case JsonValueKind.Object:
				var expectedProps = expected.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
				var actualProps = actual.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

				if (expectedProps.Count != actualProps.Count)
					return false;

				foreach (var prop in expectedProps)
				{
					if (!actualProps.TryGetValue(prop.Key, out var actualValue))
						return false;
					if (!JsonElementsAreEqual(prop.Value, actualValue))
						return false;
				}
				return true;

			case JsonValueKind.Array:
				var expectedItems = expected.EnumerateArray().ToList();
				var actualItems = actual.EnumerateArray().ToList();

				if (expectedItems.Count != actualItems.Count)
					return false;

				for (int i = 0; i < expectedItems.Count; i++)
				{
					if (!JsonElementsAreEqual(expectedItems[i], actualItems[i]))
						return false;
				}
				return true;

			case JsonValueKind.String:
				return expected.GetString() == actual.GetString();

			case JsonValueKind.Number:
				return expected.GetRawText() == actual.GetRawText();

			case JsonValueKind.True:
			case JsonValueKind.False:
			case JsonValueKind.Null:
				return true;

			default:
				return false;
		}
	}
}
