#nullable enable
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	[ContentProperty(nameof(Name))]
	class EmbeddedResourceExtension : IMarkupExtension
	{
		public string? Name { get; set; }

		public virtual object? ProvideValue(IServiceProvider serviceProvider)
		{
			if (Name == null)
				return null;

			var resourceName = "." + Name.Trim().Replace('/', '.').Replace('\\', '.');

			var assembly = typeof(MauiVisualRunnerApp).Assembly;
			foreach (var name in assembly.GetManifestResourceNames())
			{
				if (name.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase))
					return assembly.GetManifestResourceStream(name);
			}

			return null;
		}
	}
}