// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.OutOfScopeExtensions;

// no XmlnsDefinition maps this namespace: it can only be reached with an explicit clr-namespace prefix
public static class OutOfScopeLabelExtensions
{
	static readonly Dictionary<Label, string> _tags = new();

	extension(Label label)
	{
		public string OutOfScopeTag
		{
			get => _tags.TryGetValue(label, out var tag) ? tag : string.Empty;
			set => _tags[label] = value;
		}
	}
}
