using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Name")]
	public class ReferenceExtension : IMarkupExtension
	{
		public string Name { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			var valueProvider = serviceProvider.GetService(typeof (IProvideValueTarget)) as IProvideParentValues;
			if (valueProvider == null)
				throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");
			var namescopeprovider = serviceProvider.GetService(typeof (INameScopeProvider)) as INameScopeProvider;
			if (namescopeprovider != null && namescopeprovider.NameScope != null)
			{
				var value = namescopeprovider.NameScope.FindByName(Name);
				if (value != null)
					return value;
			}

			foreach (var target in valueProvider.ParentObjects)
			{
				var ns = target as INameScope;
				if (ns == null)
					continue;
				var value = ns.FindByName(Name);
				if (value != null)
					return value;
			}
			throw new Exception("Can't resolve name on Element");
		}
	}
}