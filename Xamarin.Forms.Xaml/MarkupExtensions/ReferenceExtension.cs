using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty(nameof(Name))]
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

			//legacy path. could be hit by code processed by previous versions of XamlC
#pragma warning disable CS0612 // Type or member is obsolete
			value = serviceProvider.GetService<INameScopeProvider>()?.NameScope?.FindByName(Name);
			if (value != null)
				return value;

#pragma warning restore CS0612 // Type or member is obsolete

			//fallback
			var valueProvider = serviceProvider.GetService<IProvideValueTarget>() as IProvideParentValues
								   ?? throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");
			foreach (var target in valueProvider.ParentObjects)
			{
				if (!(target is BindableObject bo))
					continue;
				if (!(NameScope.GetNameScope(bo) is INameScope ns))
					continue;
				value = ns.FindByName(Name);
				if (value != null)
					return value;
			}

			throw new XamlParseException($"Can not find the object referenced by `{Name}`", serviceProvider);
		}
	}
}