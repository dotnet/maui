namespace Microsoft.Maui.UiEvidence;

sealed class CommandLineOptions
{
	readonly Dictionary<string, string?> _values;

	CommandLineOptions(Dictionary<string, string?> values)
	{
		_values = values;
	}

	public static CommandLineOptions Parse(IEnumerable<string> arguments)
	{
		var items = arguments.ToArray();
		var values = new Dictionary<string, string?>(StringComparer.Ordinal);
		for (var index = 0; index < items.Length; index++)
		{
			var argument = items[index];
			if (!argument.StartsWith("--", StringComparison.Ordinal) || argument.Length == 2)
				throw new ArgumentException($"Invalid option '{argument}'.");

			var name = argument[2..];
			var value = index + 1 < items.Length &&
				!items[index + 1].StartsWith("--", StringComparison.Ordinal)
					? items[++index]
					: "true";

			if (!values.TryAdd(name, value))
				throw new ArgumentException($"Option '--{name}' was supplied more than once.");
		}

		return new CommandLineOptions(values);
	}

	public string Required(string name) =>
		_values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
			? value
			: throw new ArgumentException($"Option '--{name}' is required.");

	public string Optional(string name, string defaultValue) =>
		_values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
			? value
			: defaultValue;

	public int OptionalInt(string name, int defaultValue)
	{
		var value = Optional(name, defaultValue.ToString());
		return int.TryParse(value, out var parsed)
			? parsed
			: throw new ArgumentException($"Option '--{name}' must be an integer.");
	}

	public bool Flag(string name) =>
		_values.TryGetValue(name, out var value) &&
		(bool.TryParse(value, out var parsed) ? parsed : value is "1" or "yes");
}
