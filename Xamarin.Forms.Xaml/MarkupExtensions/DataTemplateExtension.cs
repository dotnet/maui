using System;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Type")]
	public sealed class DataTemplateExtension : IMarkupExtension<DataTemplate>
	{
		public string Type { get; set; }

		public DataTemplate ProvideValue(IServiceProvider serviceProvider)
		{
			var typeResolver = serviceProvider.GetService<IXamlTypeResolver>();
			if (typeResolver.TryResolve(Type, out var type))
				return new DataTemplate(type);
			throw new Exception("Could not locate type for " + Type);
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<DataTemplate>).ProvideValue(serviceProvider);
		}
	}
}