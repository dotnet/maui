using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Controls.Xaml.UnitTests.ExternalAssembly.ScopedExtensions;

/// <summary>
/// Brought in scope from another assembly through an [XmlnsDefinition] carrying an AssemblyName.
/// </summary>
public static class ExternalScopedExtensions
{
	static readonly Dictionary<Label, string> _tags = new();

	extension(Label label)
	{
		public string ExternalScopedTag
		{
			get => _tags.TryGetValue(label, out var tag) ? tag : string.Empty;
			set => _tags[label] = value;
		}
	}
}
