# XAML x:Code Directive

## Overview

The `x:Code` directive allows embedding inline C# member declarations directly in XAML files. The XAML source generator extracts these code blocks and emits them as part of a partial class, making them available alongside the code-behind.

### Motivation

Currently, any C# logic associated with a XAML page must live in a separate code-behind file. For simple cases — a single event handler, a helper method, a field — switching between XAML and code-behind adds friction. `x:Code` lets developers keep tightly-coupled logic next to the markup that uses it:

- Small event handlers can live next to the control they serve
- Helper methods used by a single page don't need a separate file
- Prototyping is faster when everything is in one file

### Example

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.MainPage">

    <x:Code><![CDATA[
        using System.Diagnostics;

        void OnButtonClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Button was clicked!");
            clickCount++;
        }

        int clickCount;
    ]]></x:Code>

    <Button Clicked="OnButtonClicked" Text="Click me" />
</ContentPage>
```

## Syntax

### Basic Form

`x:Code` is an element in the XAML `x:` namespace. Its text content is C# code:

```xml
<x:Code><![CDATA[
    void MyMethod() { }
]]></x:Code>
```

**CDATA is recommended** to avoid XML escaping issues with `<`, `>`, `&`, and other characters common in C#. Plain text content is also accepted for simple declarations that don't use these characters.

### Placement Rules

- `x:Code` **must be a direct child of the root element**. It cannot appear inside a `StackLayout`, `Grid`, or any other non-root element.
- The root element **must have `x:Class`** defined — `x:Code` generates a partial class and needs a target type.
- Multiple `x:Code` blocks are allowed. They are concatenated in document order.

```xml
<!-- ✅ Valid: direct child of root -->
<ContentPage x:Class="MyApp.MainPage" ...>
    <x:Code><![CDATA[ int _count; ]]></x:Code>
    <Label Text="Hello" />
    <x:Code><![CDATA[ void Increment() => _count++; ]]></x:Code>
</ContentPage>

<!-- ❌ Invalid: nested inside StackLayout -->
<ContentPage x:Class="MyApp.MainPage" ...>
    <StackLayout>
        <x:Code><![CDATA[ ... ]]></x:Code>  <!-- MAUIX2015 -->
    </StackLayout>
</ContentPage>

<!-- ❌ Invalid: no x:Class -->
<ResourceDictionary ...>
    <x:Code><![CDATA[ ... ]]></x:Code>  <!-- MAUIX2016 -->
</ResourceDictionary>
```

### What Can Go Inside x:Code

`x:Code` accepts any C# that is valid inside a class body **or** at the file top level:

| Supported | Example |
|-----------|---------|
| Methods | `void OnClicked(object s, EventArgs e) { }` |
| Fields | `int _count;` |
| Properties | `public string Name { get; set; }` |
| Events | `public event EventHandler MyEvent;` |
| Nested types | `record Item(string Name, int Qty);` |
| Using directives | `using System.Net.Http;` |

## Using Directives

`using` directives inside `x:Code` are automatically promoted to the top of the generated file, outside the namespace and class declarations. This lets you reference additional namespaces from your inline code without modifying the code-behind.

```xml
<x:Code><![CDATA[
    using System.Net.Http;
    using System.Threading.Tasks;
    using static System.Math;
    using Compat = System.ComponentModel;

    async Task<string> FetchAsync()
    {
        using var client = new HttpClient();
        return await client.GetStringAsync("https://example.com");
    }

    double Clamp(double value) => Max(0, Min(1, value));
]]></x:Code>
```

**Generated output** (simplified):

```csharp
using System.Net.Http;
using System.Threading.Tasks;
using static System.Math;
using Compat = System.ComponentModel;

namespace MyApp
{
    partial class MainPage
    {
        async Task<string> FetchAsync()
        {
            using var client = new HttpClient();
            return await client.GetStringAsync("https://example.com");
        }

        double Clamp(double value) => Max(0, Min(1, value));
    }
}
```

### Rules

- **Regular usings** (`using System.Net.Http;`), **static usings** (`using static System.Math;`), and **aliases** (`using Alias = System.Type;`) are all promoted.
- **Using statements** (`using var x = ...`, `using (var x = ...) { }`) are left inside the class body — they are runtime constructs, not directives.
- **Duplicates are deduplicated.** If multiple `x:Code` blocks declare the same `using`, it appears once in the output.
- Using directives from `x:Code` are **independent** of usings in the code-behind file. Each generated file has its own set.

## Code Generation

### Pipeline Position

`x:Code` is processed as a third source generator pipeline, running between CodeBehind (CB) and InitializeComponent (IC):

```
XAML → CB pipeline → x:Code pipeline → IC pipeline
```

This ordering ensures:
1. The code-behind partial class exists before x:Code is emitted
2. Types and members declared in x:Code are visible to InitializeComponent (e.g., event handlers referenced in XAML attributes)

### Output

For each XAML file containing `x:Code`, the generator emits a source file with the hint name `{path}_{FileName}.xaml.xcode.cs` containing:

1. Auto-generated header comment
2. Promoted `using` directives (if any)
3. A `namespace` block matching the `x:Class` namespace
4. A `partial class` matching the `x:Class` type name
5. The member code from all `x:Code` blocks, concatenated in document order

### IC Visitor Behavior

All InitializeComponent visitors (`CreateValuesVisitor`, `SetPropertiesVisitor`, `SetNamescopesAndRegisterNames`, etc.) skip `x:Code` elements entirely. The `x:Code` element is not treated as a XAML visual element — it produces no runtime object.

## Diagnostics

| Code | Severity | Condition |
|------|----------|-----------|
| MAUIX2012 | Error | `EnablePreviewFeatures` is not set (shared with XEXPR) |
| MAUIX2015 | Error | `x:Code` is not a direct child of the root element |
| MAUIX2016 | Error | `x:Code` used without `x:Class` on the root element |

Standard C# compiler errors apply to the content of `x:Code` blocks (e.g., syntax errors, type resolution failures). These appear as normal build errors referencing the generated `.xcode.cs` file.

## Constraints

- **SourceGen only** — `x:Code` is not supported by Runtime inflation or XamlC. Attempting to use it with those inflators throws `NotSupportedException`.
- **Requires `EnablePreviewFeatures`** — same gate as XAML C# Expressions (XEXPR).
- **Root children only** — `x:Code` must be an immediate child of the root element.
- **No access to x:Name fields** — `x:Code` is emitted in a separate partial class file. Fields generated by `x:Name` are in the InitializeComponent file. Both are partial, so members are accessible at compile time, but initialization order means `x:Name` fields are only populated after `InitializeComponent()` runs.

## Relationship to XAML C# Expressions (XEXPR)

`x:Code` and XEXPR are complementary features:

| | XEXPR | x:Code |
|-|-------|--------|
| **Scope** | Inline expressions in attribute values | Member declarations in the class body |
| **Syntax** | `{expression}` in attributes | `<x:Code>` element with C# code |
| **Produces** | Bindings, event wiring, computed values | Methods, fields, properties, nested types |
| **Use case** | Bind `{Price * Quantity}` or `{(s,e) => Save()}` | Define `void Save() { ... }` |

They share the same prerequisites (`EnablePreviewFeatures`, SourceGen) and can be used together:

```xml
<ContentPage x:Class="MyApp.MainPage" ...>
    <x:Code><![CDATA[
        int Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1);
    ]]></x:Code>

    <Label Text="{$'5! = {Factorial(5)}'}" />
</ContentPage>
```

## WPF Parity

The `x:Code` directive originates from the [XAML 2006 specification](https://learn.microsoft.com/en-us/dotnet/desktop/xaml-services/xcode-intrinsic-xaml-type) and was supported in WPF. The .NET MAUI implementation follows the same core semantics with these differences:

| Aspect | WPF | .NET MAUI |
|--------|-----|-----------|
| Processing | Runtime compilation | Source generator (compile-time) |
| Inflator | Runtime only | SourceGen only |
| Using directives | Not supported | ✅ Promoted to file top |
| Preview gate | None | Requires `EnablePreviewFeatures` |
| CDATA requirement | Required | Recommended (plain text also works) |
