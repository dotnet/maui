using System;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("TypeName")]
	public class TypeExtension : IMarkupExtension<Type>
	{
		public string TypeName { get; set; }

		public Type ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException("serviceProvider");
			var typeResolver = serviceProvider.GetService(typeof (IXamlTypeResolver)) as IXamlTypeResolver;
			if (typeResolver == null)
				throw new ArgumentException("No IXamlTypeResolver in IServiceProvider");

			return typeResolver.Resolve(TypeName, serviceProvider);
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<Type>).ProvideValue(serviceProvider);
		}
	}
}