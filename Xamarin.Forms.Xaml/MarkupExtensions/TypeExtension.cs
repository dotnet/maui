using System;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty(nameof(TypeName))]
	[ProvideCompiled("Xamarin.Forms.Build.Tasks.TypeExtension")]
	public class TypeExtension : IMarkupExtension<Type>
	{
		public string TypeName { get; set; }

		public Type ProvideValue(IServiceProvider serviceProvider)
		{
			if (string.IsNullOrEmpty(TypeName))
				throw new InvalidOperationException("TypeName isn't set.");
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
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