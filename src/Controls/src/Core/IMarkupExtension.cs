#nullable disable
using System;

namespace Microsoft.Maui.Controls.Xaml
{
	public interface IMarkupExtension<out T> : IMarkupExtension
	{
		new T ProvideValue(IServiceProvider serviceProvider);
	}

	public interface IMarkupExtension
	{
		object ProvideValue(IServiceProvider serviceProvider);
	}

	/// <summary>Tells the XAML parser and compiler that they may ignore supplied service providers in methods and constructors in the attributed class.</summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class AcceptEmptyServiceProviderAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RequireServiceAttribute : Attribute
	{
		public RequireServiceAttribute(Type[] serviceTypes)
		{
			ServiceTypes = serviceTypes;
		}
		public Type[] ServiceTypes { get; }
	}
}