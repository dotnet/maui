// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using System.Collections.Generic;
using Controls.Xaml.UnitTests.ExternalAssembly;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// C# extension members for Label. In XAML those are addressed with the qualified syntax
// <Label local:LabelExtensions.MyTag="value" />, exactly like an attached property.
public static class LabelExtensions
{
	static readonly Dictionary<Label, string> _myTags = new();
	static readonly Dictionary<Label, int> _myPriorities = new();
	static readonly Dictionary<Label, Color> _myAccents = new();

	extension(Label label)
	{
		public string MyTag
		{
			get => _myTags.TryGetValue(label, out var tag) ? tag : string.Empty;
			set => _myTags[label] = value;
		}

		public int MyPriority
		{
			get => _myPriorities.TryGetValue(label, out var priority) ? priority : 0;
			set => _myPriorities[label] = value;
		}

		public Color? MyAccent
		{
			get => _myAccents.TryGetValue(label, out var accent) ? accent : null;
			set => _myAccents[label] = value!;
		}

		// no setter: can be read from C#, but cannot be assigned from XAML
		public string MyReadOnly => label.MyTag + "!";
	}
}

// extension container targeting a base type; applicable to any View
public static class NoteExtensions
{
	static readonly Dictionary<View, string> _myNotes = new();

	extension(View view)
	{
		public string MyNote
		{
			get => _myNotes.TryGetValue(view, out var note) ? note : string.Empty;
			set => _myNotes[view] = value;
		}
	}
}

// two applicable receivers for the same name; the most derived one wins
public static class OverloadedExtensions
{
	static readonly Dictionary<Element, string> _values = new();

	extension(View view)
	{
		public string MyLabel
		{
			get => _values.TryGetValue(view, out var value) ? value : string.Empty;
			set => _values[view] = "view:" + value;
		}
	}

	extension(Label label)
	{
		public string MyLabel
		{
			get => _values.TryGetValue(label, out var value) ? value : string.Empty;
			set => _values[label] = "label:" + value;
		}
	}
}

// generic extension blocks lower to generic accessor methods, which XAML cannot instantiate
public static class GenericExtensions
{
	extension<T>(IList<T> list)
	{
		public bool IsEmpty { get => list.Count == 0; set { } }
	}
}

// a non-generic IMarkupExtension: its value is only known as an object at compile time
[AcceptEmptyServiceProvider]
public class ObjectValueExtension : IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider) => "from markup";
}

public partial class ExtensionProperties : ContentPage
{
	public ExtensionProperties() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyIsSetFromXaml(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal("Hello from extension property", page.label0.MyTag);
			Assert.Equal("Regular text", page.label0.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void SeveralExtensionPropertiesAreSetFromXaml(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal("Tag value", page.label1.MyTag);
			Assert.Equal(42, page.label1.MyPriority);
			Assert.Equal("Another label", page.label1.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyValueGoesThroughTypeConverter(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal(Colors.Red, page.label2.MyAccent);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyOnBaseTypeAppliesToDerivedElement(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal("note on a view", page.label3.MyNote);
		}

		[Theory]
		[XamlInflatorData]
		internal void MostDerivedReceiverWins(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal("label:from xaml", page.label4.MyLabel);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyFromAnotherAssemblyIsSetFromXaml(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal("external", page.label5.ExternalTag);
		}

		[Theory]
		[XamlInflatorData]
		internal void SetterAloneDefinesTheExtensionPropertyShape(XamlInflator inflator)
		{
			// SplitAccessor is declared with an int getter on Label and a string setter on View. Only the
			// setter is used, so the value is a string and the receiver is upcast to View
			var page = new ExtensionProperties(inflator);

			Assert.Equal("view:from xaml", page.label7.AutomationId);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyAcceptsAValueKnownAsObject(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal("from markup", page.label8.MyTag);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyIsSetFromPropertyElementSyntax(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			Assert.Equal("from property element", page.label6.MyTag);
		}

		[Fact]
		public void ExtensionPropertiesStillWorkFromCode()
		{
			var label = new Label();

			label.MyTag = "Test value";
			label.MyPriority = 123;

			Assert.Equal("Test value", label.MyTag);
			Assert.Equal(123, label.MyPriority);
			Assert.Equal("Test value!", label.MyReadOnly);
		}

		// The runtime inflator is the only one that can be fed arbitrary xaml from a test, so the
		// negative cases below are asserted there. XamlC and SourceGen apply the exact same rules
		// (see ExtensionPropertyConventions); ExtensionPropertiesUnqualified covers them at build time.
		static string PageWith(string attribute) =>
			$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
						xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
						xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"
						xmlns:ext="clr-namespace:Controls.Xaml.UnitTests.ExternalAssembly;assembly=Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly">
				<Label {attribute} />
			</ContentPage>
			""";

		[Fact]
		public void UnqualifiedExtensionPropertyNameIsNotSupported()
		{
			// there is no `using` equivalent in xaml, so an unqualified name is never resolved against
			// extension containers, whichever assemblies happen to be loaded
			var xaml = PageWith("MyTag=\"nope\"");
			Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
		}

		[Fact]
		public void AmbiguousExtensionPropertyThrows()
		{
			var xaml = PageWith("ext:AmbiguousExtensions.MyAmbiguous=\"nope\"");
			var e = Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
			Assert.Contains("MyAmbiguous", e.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void GenericExtensionPropertyThrows()
		{
			var xaml = PageWith("local:GenericExtensions.IsEmpty=\"true\"");
			var e = Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
			Assert.Contains("IsEmpty", e.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void ReadOnlyExtensionPropertyThrows()
		{
			var xaml = PageWith("local:LabelExtensions.MyReadOnly=\"nope\"");
			var e = Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
			Assert.Contains("MyReadOnly", e.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void InaccessibleExtensionContainerThrows()
		{
			// InternalLabelExtensions is internal to the external assembly, even though the accessor methods
			// the compiler emits for it are public. The runtime rejects it while resolving the container type;
			// XamlC and SourceGen resolve non-public types and reject them in ResolveExtensionProperty instead.
			var xaml = PageWith("ext:InternalLabelExtensions.InternalTag=\"nope\"");
			var e = Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
			Assert.Contains("InternalLabelExtensions", e.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void QualifiedNameNeverFallsBackToTheTargetsOwnProperty()
		{
			// ExternalLabelExtensions declares no extension property named Text, and Label.Text must not be
			// assigned just because the qualifier was dropped
			var xaml = """
				<Label xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
						xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
						xmlns:ext="clr-namespace:Controls.Xaml.UnitTests.ExternalAssembly;assembly=Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly"
						ext:ExternalLabelExtensions.Text="wrong" />
				""";
			var label = new Label();

			Assert.Throws<XamlParseException>(() => label.LoadFromXaml(xaml));
			Assert.Null(label.Text);
		}

		[Fact]
		public void LookalikeStaticClassIsNotAnExtensionContainer()
		{
			// the members of LookalikeExtensions have the shape of lowered accessors, but the class carries no
			// extension declaration
			var xaml = PageWith("ext:LookalikeExtensions.Lookalike=\"nope\"");

			Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
		}

		[Fact]
		public void UnknownExtensionPropertyOnAContainerThrows()
		{
			var xaml = PageWith("local:LabelExtensions.NotAThing=\"nope\"");
			Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
		}

		[Fact]
		public void MismatchingReceiverThrows()
		{
			// MyTag is declared for Label, not for Button
			var xaml = """
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
							xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
							xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests">
					<Button local:LabelExtensions.MyTag="nope" />
				</ContentPage>
				""";
			Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
		}
	}
}
