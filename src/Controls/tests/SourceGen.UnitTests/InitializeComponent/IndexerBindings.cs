using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class IndexerBindings : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void BindingToCustomIndexerWithIndexerNameAttribute()
	{
		// Test that indexers with [IndexerName("CustomName")] attribute generate correct handlers
		// The PropertyChanged events use "CustomName[index]" format, not "Item[index]"
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Model[3]}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public IndexedModel Model { get; set; } = new IndexedModel();
}

public class IndexedModel
{
	private string[] _values = new string[5];

	[IndexerName("Indexer")]
	public string this[int index]
	{
		get => _values[index];
		set => _values[index] = value;
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the generated code uses "Indexer" (from IndexerName attribute), not "Item"
		Assert.Contains("\"Indexer\"", generated, StringComparison.Ordinal);
		Assert.Contains("\"Indexer[3]\"", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("\"Item\"", generated, StringComparison.Ordinal);
		Assert.DoesNotContain("\"Item[3]\"", generated, StringComparison.Ordinal);

		// Verify the getter and setter use the indexer correctly
		// The indexer access is wrapped in conditional access (Model?[3]) because Model is a reference type
		// that could be null at runtime
		Assert.Contains("source.Model?[3]", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToDefaultIndexer()
	{
		// Test that indexers without [IndexerName] attribute use the default "Item" name
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Model[2]}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public IndexedModel Model { get; set; } = new IndexedModel();
}

public class IndexedModel
{
	private string[] _values = new string[5];

	public string this[int index]
	{
		get => _values[index];
		set => _values[index] = value;
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the generated code uses "Item" (default indexer name)
		Assert.Contains("\"Item\"", generated, StringComparison.Ordinal);
		Assert.Contains("\"Item[2]\"", generated, StringComparison.Ordinal);

		// Verify the getter and setter use the indexer correctly
		// The indexer access is wrapped in conditional access because Model is a reference type
		Assert.Contains("source.Model?[2]", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToListIndexer()
	{
		// Test binding to List<T> indexer - should use the typed indexer returning T, not object
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Items[0]}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public List<string> Items { get; set; } = new List<string> { "Item1", "Item2" };
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the generated TypedBinding uses string, not object (from typed List<string> indexer)
		Assert.Contains("TypedBinding<global::Test.TestViewModel, string>", generated, StringComparison.Ordinal);

		// Verify the getter uses the indexer (with conditional access since Items is a reference type)
		Assert.Contains("source.Items?[0]", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToDictionaryIndexer()
	{
		// Test binding to Dictionary<TKey, TValue> indexer with string key
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Data[key1]}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public Dictionary<string, int> Data { get; set; } = new Dictionary<string, int> { ["key1"] = 42 };
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the generated TypedBinding uses int (the value type of Dictionary<string, int>)
		Assert.Contains("TypedBinding<global::Test.TestViewModel, int>", generated, StringComparison.Ordinal);

		// Verify the getter uses the indexer with string key (with conditional access since Data is a reference type)
		Assert.Contains("source.Data?[\"key1\"]", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToArrayElement()
	{
		// Test binding to array element - arrays don't have named indexers
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Names[1]}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public string[] Names { get; set; } = new[] { "Alice", "Bob", "Charlie" };
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the generated TypedBinding uses string (element type of string[])
		Assert.Contains("TypedBinding<global::Test.TestViewModel, string>", generated, StringComparison.Ordinal);

		// Verify the getter uses array indexer (with conditional access since Names is a reference type)
		Assert.Contains("source.Names?[1]", generated, StringComparison.Ordinal);

		// For arrays, the handler should use empty string for indexer name (no property to listen to on the array itself)
		// The array itself can't notify about element changes - only the containing property can
		Assert.Contains("new(static source => source, \"Names\")", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToNestedIndexer()
	{
		// Test binding through multiple levels with indexer
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Model.Items[0].Name}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public Container Model { get; set; } = new Container();
}

public class Container
{
	public List<Item> Items { get; set; } = new List<Item> { new Item { Name = "First" } };
}

public class Item
{
	public string Name { get; set; } = "";
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the generated TypedBinding uses string
		Assert.Contains("TypedBinding<global::Test.TestViewModel, string>", generated, StringComparison.Ordinal);

		// Verify the getter navigates through the path correctly
		// All intermediate reference type accesses use conditional access for null safety
		Assert.Contains("source.Model?.Items?[0]?.Name", generated, StringComparison.Ordinal);

		// Verify handlers for each part of the path
		Assert.Contains("\"Model\"", generated, StringComparison.Ordinal);
		Assert.Contains("\"Items\"", generated, StringComparison.Ordinal);
		Assert.Contains("\"Item\"", generated, StringComparison.Ordinal);
		Assert.Contains("\"Item[0]\"", generated, StringComparison.Ordinal);
		Assert.Contains("\"Name\"", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToIndexerWithTwoWayMode()
	{
		// Test that two-way binding to indexer generates correct setter
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Entry Text="{Binding Values[0], Mode=TwoWay}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public List<string> Values { get; set; } = new List<string> { "Initial" };
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify setter is generated for two-way binding with null check pattern matching
		// The setter uses pattern matching to safely access the indexer on a reference type
		Assert.Contains("p0[0] = value", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToNullableIndexer()
	{
		// Test binding to indexer on nullable property - should use conditional access
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Model[0]}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public List<string>? Model { get; set; } = null;
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the getter uses conditional access for nullable Model
		Assert.Contains("source.Model?[0]", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToStringIndexer()
	{
		// Test binding with string indexer key
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Label Text="{Binding Settings[theme]}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public SettingsCollection Settings { get; set; } = new SettingsCollection();
}

public class SettingsCollection
{
	public string this[string key]
	{
		get => key == "theme" ? "dark" : "unknown";
		set { }
	}
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify the getter uses string indexer (with conditional access since Settings is a reference type)
		Assert.Contains("source.Settings?[\"theme\"]", generated, StringComparison.Ordinal);

		// Verify handlers include the indexer with string key
		Assert.Contains("\"Item[theme]\"", generated, StringComparison.Ordinal);
	}

	[Fact]
	public void BindingToReadOnlyIndexer()
	{
		// Test binding to indexer without setter - should not generate setter for TwoWay
		var xaml =
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage
	xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:test="clr-namespace:Test"
	x:Class="Test.TestPage"
	x:DataType="test:TestViewModel">
	<Entry Text="{Binding Values[0], Mode=TwoWay}"/>
</ContentPage>
""";

		var code =
"""
#nullable enable
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}

public class TestViewModel
{
	public ReadOnlyIndexer Values { get; set; } = new ReadOnlyIndexer();
}

public class ReadOnlyIndexer
{
	public string this[int index] => index.ToString();
}
""";

		var (result, generated) = RunGenerator(xaml, code);

		// Verify no errors
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error));
		Assert.NotNull(generated);

		// Verify setter throws because indexer is read-only
		Assert.Contains("throw new global::System.InvalidOperationException", generated, StringComparison.Ordinal);
	}
}
