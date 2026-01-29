# XAML C# Expressions

Guidelines for using C# expressions directly in XAML property values. This feature is **SourceGen-only** (projects with XamlSourceGen enabled).

## Quick Reference

| Task | Syntax | Example |
|------|--------|---------|
| Property binding | `{PropertyName}` | `Text="{Name}"` |
| Nested property | `{Parent.Child}` | `Text="{User.DisplayName}"` |
| Method call | `{Method()}` | `Text="{GetDisplayText()}"` |
| Expression | `{A + B}` | `Text="{Price * Quantity}"` |
| Negation | `{!Property}` | `IsVisible="{!IsHidden}"` |
| Ternary | `{cond ? a : b}` | `Text="{IsActive ? 'Yes' : 'No'}"` |
| Null-coalescing | `{a ?? b}` | `Text="{Value ?? 'Default'}"` |
| String interpolation | `{$'text {expr}'}` | `Text="{$'Hello {Name}!'}"` |
| Lambda event | `{(s, e) => ...}` | `Clicked="{(s, e) => Count++}"` |
| Async event | `{async (s, e) => ...}` | `Clicked="{async (s, e) => await SaveAsync()}"` |

## Syntax Rules

### String Quoting

XAML uses double quotes for attributes, so C# strings use **single quotes**:

```xml
<!-- Single quotes become double quotes in generated C# -->
<Label Text="{Value ?? 'Default'}" />
<!-- Generated: label.Text = Value ?? "Default"; -->

<!-- Escape single quotes with backslash -->
<Label Text="{Value ?? 'it\'s working'}" />
<!-- Generated: label.Text = Value ?? "it's working"; -->

<!-- Single character stays as char literal -->
<Label Text="{GetChar('x')}" />
<!-- Generated: label.Text = GetChar('x'); -->
```

### XML Escaping in Attributes

Since XAML is XML, certain C# operators require escaping in attribute values:

| C# Operator | XAML Syntax | Notes |
|-------------|-------------|-------|
| `&&` | `&amp;&amp;` | Logical AND |
| `<` | `&lt;` | Less than |
| `>` | `&gt;` | Greater than (recommended) |
| `\|\|` | `\|\|` | Logical OR - no escaping needed |

```xml
<!-- Boolean AND requires escaping -->
<Label IsVisible="{IsLoaded &amp;&amp; HasData}" />

<!-- Boolean OR works as-is -->
<Label IsVisible="{IsEmpty || HasError}" />

<!-- Comparisons -->
<Label IsVisible="{Count &gt; 0 &amp;&amp; Count &lt; 100}" />
```

### CDATA for Complex Expressions

Use **element content with CDATA** to avoid XML escaping entirely:

```xml
<!-- Attribute with escaping (harder to read) -->
<Label IsVisible="{Count &gt; 0 &amp;&amp; Count &lt; 100}" />

<!-- CDATA with natural C# (easier to read) -->
<Label>
    <Label.IsVisible><![CDATA[{Count > 0 && Count < 100}]]></Label.IsVisible>
</Label>
```

**CDATA also allows double-quoted strings** (no single-quote transformation):

```xml
<!-- CDATA with double quotes -->
<Label>
    <Label.Text><![CDATA[{Value ?? "Default Text"}]]></Label.Text>
</Label>

<!-- CDATA with string interpolation -->
<Label>
    <Label.Text><![CDATA[{$"Hello {Name}!"}]]></Label.Text>
</Label>
```

### When to Use Attribute vs CDATA

| Scenario | Use | Example |
|----------|-----|---------|
| Simple binding | Attribute | `Text="{Name}"` |
| Single `&&` | Attribute | `IsVisible="{A &amp;&amp; B}"` |
| Boolean OR | Attribute | `IsVisible="{A \|\| B}"` |
| Multiple `&&`/comparisons | **CDATA** | `<![CDATA[{A && B && C > 0}]]>` |
| Double-quoted strings | **CDATA** | `<![CDATA[{Value ?? "Default"}]]>` |

**Rule of thumb**: If the escaped version is hard to read, use CDATA.
```

### Explicit vs Implicit Syntax

```xml
<!-- Implicit (preferred) - detected automatically -->
<Label Text="{Name}" />
<Label Text="{GetText()}" />

<!-- Explicit with = prefix (only when needed for disambiguation) -->
<Label Text="{= SomeAmbiguousName}" />
```

## Member Resolution (x:DataType)

When `x:DataType` is set, the compiler automatically determines whether identifiers refer to `this` (the page/view) or the `BindingContext`:

### Automatic Detection

```xml
<ContentPage x:DataType="local:MyViewModel">
    <!-- 'Name' only exists on MyViewModel → generates TypedBinding -->
    <Label Text="{Name}" />
    
    <!-- 'Title' only exists on this page → generates SetValue -->
    <Label Text="{Title}" />
</ContentPage>
```

### Explicit Prefixes (when needed)

| Prefix | Meaning | Use When |
|--------|---------|----------|
| `{this.Foo}` | Force local (page/view) | Property exists on both types |
| `{.Foo}` | Force binding (BindingContext) | Property exists on both types |
| `{BindingContext.Foo}` | Force binding (verbose) | Clarity in complex scenarios |

```xml
<!-- Both page and ViewModel have 'Title' property -->
<Label Text="{this.Title}" />      <!-- Uses page's Title -->
<Label Text="{.Title}" />          <!-- Binds to ViewModel's Title -->
```

### Diagnostics

| Code | Meaning | Fix |
|------|---------|-----|
| `MAUIX2007` | Bare identifier `{Foo}` could be markup extension OR property | Use `{= Foo}` for expression or `{local:Foo}` for markup |
| `MAUIX2008` | Property exists on both `this` and `x:DataType` | Use `this.Prop` or `.Prop` prefix |
| `MAUIX2009` | Property not found on either type | Check spelling, add property, or check x:DataType |
| `MAUIX2010` | Expression cannot generate setter for two-way binding | Use simple property chain or set Mode=OneWay |

**Note on MAUIX2007**: When a bare identifier like `{Binding}` matches both a markup extension name (e.g., `BindingExtension`) AND a property on your ViewModel, the compiler defaults to treating it as the markup extension (for backward compatibility) and emits this warning. To disambiguate:
- Use `{= Binding}` to treat it as a C# expression accessing the `Binding` property
- Use the markup extension with explicit prefix like `{x:Binding}` if you meant the extension

## TypedBinding Generation

Expressions referencing `x:DataType` properties generate `TypedBinding` with INPC handlers.

**Simple property chains** get a setter for two-way binding:

```xml
<Entry Text="{User.Name}" />
```

Generates:
```csharp
new TypedBinding<ViewModel, string>(
    getter: __source => (__source.User.Name, true),
    setter: (__source, __value) => __source.User.Name = __value,
    handlers: ...);
```

**Complex expressions** (operators, method calls) get `null` setter:

```xml
<Label Text="{User.DisplayName}" />
```

Generates:
```csharp
label.SetBinding(Label.TextProperty,
    new TypedBinding<ViewModel, string>(
        getter: __source => (__source.User.DisplayName, true),
        setter: null,
        handlers: new[] {
            new Tuple<Func<ViewModel, object>, string>(__source => __source, "User"),
            new Tuple<Func<ViewModel, object>, string>(__source => __source.User, "DisplayName"),
        }));
```

### Multi-Root Expressions

Multiple properties from BindingContext create handlers for each:

```xml
<Label Text="{(Price * Quantity).ToString()}" />
```

Generates handlers for both `Price` and `Quantity`.

### Mixed Local + Binding

When expression uses both `this` and `x:DataType` properties:

```xml
<Label Text="{(Price * TaxRate).ToString('F2')}" />
<!-- Price from ViewModel, TaxRate from this -->
```

- Local values (`this.TaxRate`) are **captured once** at initialization
- Only BindingContext properties get INPC handlers
- Use explicit `this.` prefix if auto-detection fails

## Lambda Events

### Basic Syntax

```xml
<!-- Inline action -->
<Button Clicked="{(s, e) => Count++}" />

<!-- Method call -->
<Button Clicked="{(s, e) => Save()}" />

<!-- With sender -->
<Button Clicked="{(sender, e) => Log(sender)}" />

<!-- Event args -->
<Entry TextChanged="{(s, e) => OnTextChanged(e.NewTextValue)}" />
```

### Async Events

```xml
<!-- Basic async -->
<Button Clicked="{async (s, e) => await LoadDataAsync()}" />

<!-- With error handling -->
<Button Clicked="{async (s, e) => { try { await SaveAsync(); } catch { ShowError(); } }}" />
```

**Note:** Lambda events must include `(sender, args)` parameters. Parameterless `() => ...` is not supported.

## String Interpolation

```xml
<!-- Simple -->
<Label Text="{$'Hello {Name}!'}" />

<!-- Multiple expressions -->
<Label Text="{$'{Quantity}x at {Price:C2}'}" />

<!-- With method calls -->
<Label Text="{$'Result: {GetText()}'}" />
```

## Markup Extension Disambiguation

Known markup extensions are NOT treated as expressions:

```xml
<!-- These use standard markup extension processing -->
<Label Text="{Binding Name}" />
<Label Text="{StaticResource MyKey}" />
<Label Text="{DynamicResource ThemeColor}" />
<Label Text="{x:Static Member=local:Constants.Value}" />
<Label Text="{OnPlatform Default=A, iOS=B}" />
<Label Text="{OnIdiom Default=X, Phone=Y}" />
```

## Common Patterns

### Replacing MultiBinding + Converters

**Before:**
```xml
<Label.IsVisible>
    <MultiBinding Converter="{StaticResource allTrueConverter}">
        <Binding Path="IsLoaded"/>
        <Binding Path="ShowContent"/>
    </MultiBinding>
</Label.IsVisible>
```

**After:**
```xml
<Label IsVisible="{IsLoaded && ShowContent}" />
```

### Replacing BooleanInvertConverter

**Before:**
```xml
<Label IsVisible="{Binding IsHidden, Converter={StaticResource invertBool}}" />
```

**After:**
```xml
<Label IsVisible="{!IsHidden}" />
```

### Replacing Code-Behind Event Handlers

**Before:**
```xml
<Button Clicked="OnButtonClicked" />
```
```csharp
void OnButtonClicked(object s, EventArgs e) => Count++;
```

**After:**
```xml
<Button Clicked="{(s, e) => Count++}" />
```

### Computed Display Values

**Before:**
```xml
<Label Text="{Binding FullName}" />
```
```csharp
public string FullName => $"{FirstName} {LastName}";
```

**After:**
```xml
<Label Text="{$'{FirstName} {LastName}'}" />
```

## When NOT to Use Expressions

| Scenario | Use Instead |
|----------|-------------|
| Two-way binding | `{Binding Prop, Mode=TwoWay}` |
| RelativeSource binding | `{Binding Prop, Source={RelativeSource ...}}` |
| Complex formatting | Converter or ViewModel property |
| Reusable logic | ViewModel method or property |
| Multi-statement logic | Code-behind method |

## Static Method Calls

Static methods work but require proper qualification:

| Syntax | Works? | Why |
|--------|--------|-----|
| `{string.Format(...)}` | ✅ | `string` is C# keyword |
| `{System.Math.Max(A, B)}` | ✅ | Fully qualified |
| `{Math.Max(A, B)}` | ✅ | Works if project has `global using System;` |

## File Extensions

This feature requires **XamlSourceGen** to be enabled in the project. It works with regular `.xaml` files when the project has SourceGen enabled.

| Project Configuration | Expressions Work? |
|----------------------|-------------------|
| XamlSourceGen enabled | ✅ Yes |
| XamlC only | ❌ No |
| Runtime XAML loading | ❌ No |

## Specification

Full specification: `docs/specs/XamlCSharpExpressions.md`
