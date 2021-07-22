using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;

namespace Microsoft.Maui.Controls
{
	public sealed class ReferenceTypeConverter : TypeConverter, IExtendedTypeConverter
	{
		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			var referenceProvider = serviceProvider.GetService<IReferenceProvider>();
			if (referenceProvider != null)
				return referenceProvider.FindByName(value) ?? throw new XamlParseException($"Can't resolve name '{value}' on Element", serviceProvider);

			if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideParentValues valueProvider))
				throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");

			foreach (var target in valueProvider.ParentObjects)
			{
				if (!(target is INameScope ns))
					continue;
				var element = ns.FindByName(value);
				if (element != null)
					return element;
			}
			throw new Exception("Can't resolve name on Element");
		}

		public override object ConvertFromInvariantString(string value) => throw new NotImplementedException();

		public override string ConvertToInvariantString(object value) => throw new NotSupportedException();
	}
}