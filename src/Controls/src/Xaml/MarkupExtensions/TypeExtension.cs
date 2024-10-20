using System;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(TypeName))]
	[ProvideCompiled("Microsoft.Maui.Controls.Build.Tasks.TypeExtension")]
	[RequireService([typeof(IXamlTypeResolver), typeof(IXmlLineInfoProvider)])]
	public class TypeExtension : IMarkupExtension<Type>
	{
		public string TypeName { get; set; }

		public Type ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver))
				throw new ArgumentException("No IXamlTypeResolver in IServiceProvider");
			if (string.IsNullOrEmpty(TypeName))
			{
				var li = (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lip) ? lip.XmlLineInfo : new XmlLineInfo();
				throw new XamlParseException("TypeName isn't set.", li);
			}

			return typeResolver.Resolve(TypeName, serviceProvider);
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<Type>).ProvideValue(serviceProvider);
		}
	}
}