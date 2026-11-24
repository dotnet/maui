// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.GlobalScopedExtensions;

// only reachable through the global xmlns
public static class GlobalLabelExtensions
{
	static readonly Dictionary<Label, string> _tags = new();

	extension(Label label)
	{
		public string GlobalTag
		{
			get => _tags.TryGetValue(label, out var tag) ? tag : string.Empty;
			set => _tags[label] = value;
		}
	}
}
