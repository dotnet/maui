using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Name))]
	[RequireService([typeof(IReferenceProvider), typeof(IProvideValueTarget)])]
	public class ReferenceExtension : IMarkupExtension
	{
		public string Name { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			var referenceProvider = serviceProvider.GetService<IReferenceProvider>();
			var value = referenceProvider?.FindByName(Name);
			if (value != null)
				return value;

			//fallback
			var valueProvider = serviceProvider.GetService<IProvideValueTarget>() as IProvideParentValues
				?? throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");

			foreach (var target in valueProvider.ParentObjects)
			{
				if (target is not BindableObject bo)
					continue;
				if (NameScope.GetNameScope(bo) is not INameScope ns)
					continue;
				value = ns.FindByName(Name);
				if (value != null)
					return value;
			}

			throw new XamlParseException($"Cannot find the object referenced by `{Name}`", serviceProvider);
		}
	}
}
