// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

// The MAUI xmlns and the global xmlns both bring these namespaces in scope, exactly like an app declaring
// its own [XmlnsDefinition] would. Unqualified extension properties are resolved through that map.
[assembly: XmlnsDefinition(XamlParser.MauiUri, "Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions")]
[assembly: XmlnsDefinition(XamlParser.MauiUri, "Controls.Xaml.UnitTests.ExternalAssembly.ScopedExtensions",
	AssemblyName = "Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly")]
[assembly: XmlnsDefinition(XamlParser.MauiGlobalUri, "Microsoft.Maui.Controls.Xaml.UnitTests.GlobalScopedExtensions")]

namespace Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions;

public static class LabelScopedExtensions
{
	static readonly Dictionary<Label, string> _tags = new();

	extension(Label label)
	{
		public string ScopedTag
		{
			get => _tags.TryGetValue(label, out var tag) ? tag : string.Empty;
			set => _tags[label] = value;
		}

		// Label.Text exists as an instance property and has to win over this one
		public string Text
		{
			get => "extension";
			set => label.AutomationId = "extension wins: " + value;
		}
	}
}

// two containers declaring the same name for receivers of different specificity
public static class ViewScopedExtensions
{
	static readonly Dictionary<View, string> _values = new();

	extension(View view)
	{
		public string MostSpecific
		{
			get => _values.TryGetValue(view, out var value) ? value : string.Empty;
			set => _values[view] = "view:" + value;
		}
	}
}

public static class LabelSpecificExtensions
{
	static readonly Dictionary<Label, string> _values = new();

	extension(Label label)
	{
		public string MostSpecific
		{
			get => _values.TryGetValue(label, out var value) ? value : string.Empty;
			set => _values[label] = "label:" + value;
		}
	}
}

// two containers declaring the same name for unrelated receivers, both applicable to a Label
public static class ScopedAmbiguousAExtensions
{
	extension(IView view)
	{
		public string ScopedAmbiguous
		{
			get => string.Empty;
			set => _ = value;
		}
	}
}

public static class ScopedAmbiguousBExtensions
{
	extension(BindableObject bindable)
	{
		public string ScopedAmbiguous
		{
			get => string.Empty;
			set => _ = value;
		}
	}
}
