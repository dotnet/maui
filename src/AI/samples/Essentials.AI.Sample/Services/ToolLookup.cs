namespace Maui.Controls.Sample.Services;

public class ToolLookup
{
    public required string Id { get; init; }

    public IDictionary<string, object?>? Arguments { get; set; }

    public object? Result { get; set; }
}
