using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Key))]
	[RequireService([typeof(IXmlLineInfoProvider)])]
	public sealed class DynamicResourceExtension : IMarkupExtension<DynamicResource>
	{
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