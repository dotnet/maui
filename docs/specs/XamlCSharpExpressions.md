# XAML C# Expressions

## Overview

XAML C# Expressions allow writing C# expressions directly in XAML attribute values. The source generator parses these expressions and generates appropriate bindings or event handlers.

### Motivation

Currently, XAML property values can be:
1. **Literal strings** - `Text="Hello World"`
2. **Markup extensions** - `Text="{Binding Name}"`, `Text="{StaticResource MyString}"`
3. **Type converters** - `Color="Red"` (converted via `ColorTypeConverter`)

However, there's no way to directly invoke C# code (methods, properties, expressions) from XAML without creating a binding or code-behind event handler. This leads to:
- Boilerplate for simple computed values
- Unnecessary runtime overhead for static computations
- Reduced readability when simple expressions are wrapped in bindings

### Example

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:DataType="local:MainBindingContext">

  <StackLayout>
    <Label Text="{$'{FirstName} {LastName}'}" />
    <Label Text="{Items.Count}" />
    <Button Clicked="{(s, e) => Count++}" />
  </StackLayout>

</ContentPage>
```

## Expression Resolution

When the source generator encounters a `{...}` value, it applies the following rules.

### Simple Expressions

For simple expressions (single identifier or property path), resolution follows this order:

**1. Markup Extension Match**

If the expression matches a markup extension (e.g., `{Binding ...}`, `{StaticResource ...}`, `{x:Static ...}`), use the markup extension.

**Ambiguity (MAUIX2007):** If a bare identifier like `{Foo}` matches both a markup extension (`FooExtension`) and a resolvable C# property, the markup extension takes precedence and a warning is emitted.

**2. Binding Expression**

If the target property is bindable and the expression can be matched as a BindingContext property path, the generator creates a `Binding`:

```xml
<Label Text="{Username}" />
<Label Text="{User.DisplayName}" />
```

This is equivalent to `{Binding Username}` but with compile-time type checking. The path is resolved against the `x:DataType`.

**Conflict (MAUIX2008):** If the identifier exists on both the page and BindingContext, an error is emitted. See [Disambiguation](#disambiguation) for resolution options.

**3. Local Access**

If the expression cannot be converted to a binding, the generator checks for instance properties or methods on the page/view type:

```xml
<Label Text="{Title}" />
<Label Text="{GetFormattedDate()}" />
```

The value is captured once at initialization time.

**4. Static Invocation**

If no instance member matches, the generator checks for static methods and properties. Global usings are respected:

```xml
<Label Text="{DateTime.Now}" />
<Label Text="{Math.Max(A, B)}" />
```

### Lambda for Events

If the target property is an event and the expression is a lambda, the generator creates a delegate:

```xml
<Button Clicked="{(s, e) => Count++}" />
<Button Clicked="{(s, e) => SaveData()}" />
```

The generator creates a method with the appropriate signature and wires it to the event.

**Note:** Async lambdas (`{async (s, e) => ...}`) are not supported. Use a regular method for async event handling.

### Compound Expressions

Expressions can combine multiple sources. Each part is resolved using the simple expression rules 2-4 above (binding, local, static) — markup extensions are not matched.

**String Interpolation:**

```xml
<Label Text="{$'{FirstName} {LastName}'}" />
<Label Text="{$'{Quantity}x {ProductName}'}" />
<Label Text="{$'Total: {Price:C2}'}" />
```

Each interpolation hole is analyzed and bound appropriately.

**Operators and Mixed Sources:**

```xml
<!-- Arithmetic: binding + method -->
<Label Text="{Price * GetTaxRate()}" />

<!-- Boolean: two bindings -->
<Label IsVisible="{IsAdmin || IsPrivileged}" />

<!-- Comparison: binding + local -->
<Label IsVisible="{Count > MinimumCount}" />

<!-- Mixed: string interpolation, binding, static -->
<Label Text="{$'{Name} - {DateTime.Now:d}'}" />
```

The generator analyzes the expression and creates appropriate bindings or captured values for each part.

## Expression Syntax

Expressions are standard C#. Any valid C# expression can be used, subject to XML escaping rules.

### Operator Aliases

To avoid XML escaping, you can use word-based operator aliases:

| Alias | C# Equivalent | Example |
|-------|---------------|---------|
| `AND` | `&&` | `{.IsLoaded AND .HasData}` |
| `OR` | `\|\|` | `{.IsEmpty OR .HasError}` |
| `LT` | `<` | `{.Count LT 100}` |
| `GT` | `>` | `{.Count GT 0}` |
| `LTE` | `<=` | `{.Count LTE 100}` |
| `GTE` | `>=` | `{.Count GTE 0}` |

```xml
<!-- Readable without escaping -->
<Label IsVisible="{.IsLoaded AND .HasData}" />
<Label IsVisible="{.Count GT 0 AND .Count LT 100}" />
```

Aliases are case-insensitive (`AND`, `and`, `And` all work) and require surrounding spaces.

### XML Escaping

If you prefer standard C# syntax, escape these characters in attributes:

| C# | XAML Attribute |
|----|----------------|
| `&&` | `&amp;&amp;` |
| `<` | `&lt;` |
| `>` | `&gt;` |
| `\|\|` | `\|\|` (no escaping needed) |

```xml
<Label IsVisible="{Count &gt; 0 &amp;&amp; Count &lt; 100}" />
```

### String Quoting

Since XAML attributes use double quotes, C# strings use single quotes:

```xml
<Label Text="{Name ?? 'Unknown'}" />
<Label Text="{IsVip ? 'Gold' : 'Standard'}" />
```

Single quotes are converted to double quotes in the generated C#. Escape single quotes with backslash: `'it\'s'` becomes `"it's"`.

**Char literals:** For methods expecting `char`, the generator inspects the method signature and preserves char literals:

```xml
<Label Text="{GetChar('x')}" />  <!-- GetChar(char) → keeps 'x' as char -->
<Label Text="{GetText('x')}" />  <!-- GetText(string) → converts to "x" -->
```

### CDATA Alternative

For complex expressions, use element syntax with CDATA to avoid escaping entirely:

```xml
<Label>
    <Label.IsVisible><![CDATA[{Count > 0 && Count < 100}]]></Label.IsVisible>
</Label>

<Label>
    <Label.Text><![CDATA[{Value ?? "Default"}]]></Label.Text>
</Label>
```

**Use CDATA when:**
- Expression has multiple `&&` or comparison operators
- You need double-quoted strings
- You need explicit char literals without semantic analysis
- The escaped version is hard to read

### Type References

Types in expressions are resolved using C# rules: global usings, project usings, and fully qualified names. For types that are not in scope via C# usings, you can use **XAML xmlns prefixes** to reference them:

```xml
<ContentPage xmlns:ios="clr-namespace:Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;assembly=..."
             xmlns:local="clr-namespace:MyApp.Helpers">

  <!-- xmlns prefix to reference a type not in global usings -->
  <Label Text="{ios:Page.GetUseSafeArea(this)}" />

  <!-- Same as writing the fully qualified name -->
  <Label Text="{Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this)}" />

  <!-- Types in global usings don't need a prefix -->
  <Label Text="{Math.Max(A, B)}" />
</ContentPage>
```

The `prefix:Type` syntax is expanded to the fully qualified CLR type name before the expression is compiled. This reuses the same xmlns-to-namespace mappings already declared on the XAML document.

**Note:** This is distinct from `{prefix:Name}` at the start of an expression, which is always interpreted as a markup extension (see [Disambiguation](#disambiguation)).

## Disambiguation

When a `{...}` value could be interpreted as either a markup extension or an expression, disambiguation rules apply.

### Explicit Expression: `{= ...}`

Prefix with `=` to force expression parsing:

```xml
<Label Text="{= Binding}" />  <!-- Uses the 'Binding' property, not BindingExtension -->
```

### Page Member: `{this.Foo}`

Use `this.` to explicitly reference a member on the page/view type:

```xml
<Label Text="{this.Title}" />
```

### BindingContext Member: `{.Foo}`

Use `.` prefix to explicitly reference a member on the BindingContext:

```xml
<Label Text="{.Title}" />
```

This is useful when both page and BindingContext have a property with the same name.

### Member Conflict (MAUIX2008)

When `{Foo}` exists on both the page and BindingContext:
- **Error**: MAUIX2008 is emitted
- **Resolution**: Use `{this.Foo}` for page or `{.Foo}` for BindingContext

### Explicit Markup Extension: `{prefix:Name}`

Use an xmlns prefix to explicitly invoke a markup extension:

```xml
<Label Text="{local:MyMarkup}" />
```

### Ambiguity Warning (MAUIX2007)

When `{Foo}` matches both a markup extension (`FooExtension`) and a property (`Foo`):
- **Behavior**: Defaults to markup extension (backward compatible)
- **Warning**: MAUIX2007 is emitted
- **Resolution**: Use `{= Foo}` for expression or `{local:Foo}` for explicit markup

This is extremely unlikely in practice—it requires a property name that exactly matches a markup extension name without its `Extension` suffix.

## Two-Way Binding

Simple property paths support two-way binding automatically:

| Expression | Two-Way? |
|------------|----------|
| `{Name}` | ✅ |
| `{User.Name}` | ✅ |
| `{Price * Qty}` | ❌ |
| `{Name.ToUpper()}` | ❌ |

Complex expressions (operators, method calls) cannot generate a setter and are one-way only.

## Diagnostics

| Code | Severity | Description |
|------|----------|-------------|
| MAUIX2007 | Warning | Bare identifier matches both markup extension and property — defaults to markup |
| MAUIX2008 | Error | Member exists on both page and BindingContext — use `this.` or `.` to disambiguate |
| MAUIX2009 | Error | Member not found on page or BindingContext |
| MAUIX2010 | Info | Complex expression on two-way property — binding will be one-way |
| MAUIX2013 | Error | Async lambda event handlers are not supported |

## Limitations

- **SourceGen only** — not available in XamlC or Runtime inflation
- **Single expressions** — no multi-statement blocks or control flow
- **Event lambdas require parameters** — `{(s, e) => ...}` not `{() => ...}`
- **No async lambdas** — use regular methods for async event handling
- **Static types need qualification** — use full path, global usings, or xmlns prefixes (see [Type References](#type-references))

## Attached Bindable Properties

Attached bindable properties (ABPs) can be accessed in expressions using target-qualified `target.(Type.Property)` parenthesized syntax, following the convention established by [x:Bind](https://learn.microsoft.com/en-us/windows/apps/develop/platform/xaml/x-bind-markup-extension#attached-properties).

### Syntax

```
target.(Type.Property)
```

The parentheses distinguish an attached property access from a regular `Type.Property` static member access. `target` must currently be a named element or `this`. `Type` is resolved via C# usings or XAML xmlns prefixes (see [Type References](#type-references)).

### Reading Attached Properties

Because attached properties are set on `BindableObject`s (views), the primary use case is reading from a **named element**. `this` refers to the top-level element (the page/view root), not the element the attribute is on, so `this.(Grid.Row)` reads the row of the *page*, not the current element.

**From a named element** (most common) — read an ABP from another element by `x:Name`:

```xml
<Grid>
  <Button x:Name="myButton" Grid.Row="2" />
  <Label Text="{myButton.(Grid.Row)}" />
</Grid>
```

**From the page root** — `this` is the top-level element in the XAML file:

```xml
<!-- Reads Grid.Row of the page itself, NOT of the Label -->
<Label Text="{this.(Grid.Row)}" />
```

**With xmlns prefix** — for types not in global usings (see [Type References](#type-references)):

```xml
<Label Text="{this.(ios:Page.UseSafeArea)}" />
```

**From BindingContext** — **not yet implemented**. The standalone `(Type.Property)` form is reserved for possible future BindingContext attached-property binding support and is currently rejected by the source generator:

```xml
<!-- Not yet implemented -->
<Label Text="{(Grid.Row)}" />
```

### In Compound Expressions

ABP access can appear anywhere a value is expected:

```xml
<!-- Arithmetic -->
<Label Text="{myButton.(Grid.Row) * 100}" />

<!-- String interpolation -->
<Label Text="{$'Row {myButton.(Grid.Row)}, Col {myButton.(Grid.Column)}'}" />

<!-- Ternary -->
<Label Background="{myButton.(Grid.Row) == 0 ? Colors.Gray : Colors.White}" />

<!-- Method argument -->
<Label Text="{FormatPosition(myButton.(Grid.Row), myButton.(Grid.Column))}" />
```

### Two-Way Binding

**Not yet implemented for attached bindable properties.** Target-qualified ABP expressions are lowered to static getter calls and are one-way initialization/binding inputs only. Supporting generated setters for ABPs needs a separate design decision.

| Expression | Two-Way? |
|------------|----------|
| `{myBtn.(Grid.Row)}` | ❌ |
| `{this.(Grid.Row)}` | ❌ |
| `{myBtn.(Grid.Row) * 2}` | ❌ (compound) |

### Change Notification

**Not yet implemented for attached bindable properties.** The current rewrite does not add `PropertyChanged` subscriptions for ABP changes. Expressions are evaluated through the same expression pipeline as other generated values, but ABP-specific change tracking is future work.

### Classification

The `target.(Type.Property)` pattern is classified as a C# expression when it appears in an expression context (for example `{this.(Grid.Row)}` or `{myButton.(Grid.Row)}`). The source generator rewrites it to the corresponding static getter call (`Type.GetProperty(target)`) before generated C# is emitted.

**Disambiguation from casts and grouping:**

| Expression | Interpretation |
|------------|---------------|
| `{(int)Value}` | Cast — `int` is a keyword, not `Identifier.Identifier` |
| `{(Value)}` | Grouping — single identifier, no dot |
| `{(Grid.Row)}` | Reserved for future BindingContext ABP support — not yet implemented |
| `{(a + b)}` | Grouping — contains operator, not `Identifier.Identifier` |

The current rewrite is textual and target-qualified: `target.(X.Y)` becomes `X.GetY(target)`. XAML xmlns prefixes are resolved before this rewrite, so `this.(ios:Page.UseSafeArea)` becomes a fully qualified static getter call.

### Diagnostics

There are no dedicated ABP-specific diagnostics yet. Invalid ABP expressions currently surface through existing source-generator diagnostics or generated C# compilation errors.

## Future Considerations

- **Parameterless event handlers** — `{() => ...}` and `{args => ...}`
- **RelativeSource bindings** — `{RelativeSource Self.Width}`
- **Explicit two-way** — `{= Name, Mode=TwoWay}` or `{Name, set: value => Name = value}`
- **Attached bindable property setters and change notification** — two-way ABP binding and ABP-specific `PropertyChanged` tracking
- **BindingContext attached properties** — standalone `{(Type.Property)}` support when the BindingContext is a `BindableObject`

## Syntax Cheat Sheet

| Want to... | Write |
|------------|-------|
| Bind to property | `{Name}` |
| Bind to nested property | `{User.Email}` |
| Call local method | `{GetDisplayText()}` |
| Call static method | `{Math.Max(A, B)}` |
| Compute value | `{A * B}` |
| Negate bool | `{!Flag}` |
| Combine bools | `{A &amp;&amp; B}` or `{A \|\| B}` |
| Ternary | `{IsVip ? 'Gold' : 'Standard'}` |
| Null-coalesce | `{Value ?? 'Default'}` |
| Format string | `{$'{Value:F2}'}` |
| Handle event | `{(s, e) => Action()}` |
| Force expression | `{= Foo}` |
| Force page member | `{this.Foo}` |
| Force BindingContext | `{.Foo}` |
| Read attached from self | `{this.(Grid.Row)}` |
| Read attached from element | `{myBtn.(Grid.Row)}` |
| Read prefixed attached property | `{this.(ios:Page.UseSafeArea)}` |
