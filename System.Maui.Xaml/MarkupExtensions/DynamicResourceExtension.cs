using System;
using System.Maui.Internals;

namespace System.Maui.Xaml
{
	[ContentProperty(nameof(Key))]
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