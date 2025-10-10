# QueryPropertyGenerator

The `QueryPropertyGenerator` is an incremental source generator that automatically generates `IQueryAttributable` interface implementations for classes decorated with the `[QueryProperty]` attribute.

## Purpose

The `[QueryProperty]` attribute has traditionally been used to receive navigation data in .NET MAUI Shell applications. However, this attribute is not trim-safe and causes issues with full trimming and NativeAOT. The recommended approach is to implement the `IQueryAttributable` interface manually.

This source generator bridges the gap by automatically generating the `IQueryAttributable` implementation for any class using `[QueryProperty]`, making the attribute trim-safe while maintaining the same developer experience.

## How It Works

When you decorate a partial class with `[QueryProperty]` attributes:

```csharp
[QueryProperty(nameof(Name), "name")]
[QueryProperty(nameof(Age), "age")]
public partial class PersonDetailsPage : ContentPage
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

The generator automatically creates a partial class implementation that:

1. Implements the `IQueryAttributable` interface
2. Implements the `ApplyQueryAttributes` method
3. Maps query parameters to properties based on the `[QueryProperty]` attributes
4. Handles URL decoding for string properties
5. Performs type conversion for non-string properties
6. Tracks and clears properties from previous navigation

## Generated Code Example

For the class above, the generator produces:

```csharp
partial class PersonDetailsPage : Microsoft.Maui.Controls.IQueryAttributable
{
    void Microsoft.Maui.Controls.IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query == null)
            return;

        // Track which properties were set in previous navigation
        var previousKeys = _queryPropertyKeys ?? new HashSet<string>();
        _queryPropertyKeys = new HashSet<string>();

        if (query.TryGetValue("name", out var nameValue))
        {
            _queryPropertyKeys.Add("name");
            if (nameValue != null)
                Name = global::System.Net.WebUtility.UrlDecode(nameValue.ToString());
            else
                Name = null;
        }
        else if (previousKeys.Contains("name"))
        {
            Name = default;
        }

        if (query.TryGetValue("age", out var ageValue))
        {
            _queryPropertyKeys.Add("age");
            try
            {
                if (ageValue != null)
                {
                    var convertedValue = Convert.ChangeType(ageValue, typeof(int));
                    Age = (int)convertedValue;
                }
            }
            catch
            {
                // Ignore conversion errors
            }
        }
        else if (previousKeys.Contains("age"))
        {
            // Property is not nullable, skipping clear
        }
    }

    private HashSet<string>? _queryPropertyKeys;
}
```

## Features

### String Property Handling
- Automatically applies URL decoding using `WebUtility.UrlDecode`
- Handles null values appropriately

### Non-String Property Handling
- Uses `Convert.ChangeType` for type conversion
- Wraps conversion in try-catch to handle invalid values gracefully
- Supports all types that implement `IConvertible`

### Property Clearing
- Tracks properties set in previous navigation
- Clears nullable properties when they're not present in the current navigation
- Skips clearing non-nullable properties to avoid default value assignment

### Requirements
- The class must be declared as `partial`
- The class must have a property matching the name specified in `[QueryProperty]`
- The property must have a public setter

## Trim Safety

This generator makes `[QueryProperty]` attributes trim-safe because:
1. All property access is statically generated at compile time
2. No reflection is used to discover or set properties
3. The generated code is fully analyzable by the trimmer

## Compatibility

The generated implementation is fully compatible with:
- Full trimming mode
- NativeAOT
- All supported .NET MAUI platforms

## Testing

The generator includes comprehensive unit tests covering:
- Single and multiple query properties
- String properties with URL decoding
- Non-string properties (int, double, bool, etc.)
- Property clearing behavior
- Nullable and non-nullable types
- Documentation examples
- Compilation validation

See `QueryPropertyGeneratorTests.cs` for the complete test suite.
