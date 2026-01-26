using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that creates a <see cref="DynamicResource"/> for dynamic resource lookup.
	/// </summary>
	[ContentProperty(nameof(Key))]
	[RequireService([typeof(IXmlLineInfoProvider)])]
	public sealed class DynamicResourceExtension : IMarkupExtension<DynamicResource>
	{
		/// <summary>
		/// Gets or sets the key of the dynamic resource to retrieve.
		/// </summary>
		public string Key { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider) => ((IMarkupExtension<DynamicResource>)this).ProvideValue(serviceProvider);

		DynamicResource IMarkupExtension<DynamicResource>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (Key == null)
				throw new XamlParseException("DynamicResource markup require a Key", serviceProvider);
			return new DynamicResource(Key);
		}
	}
}