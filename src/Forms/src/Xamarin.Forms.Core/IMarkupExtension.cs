using System;

namespace Xamarin.Forms.Xaml
{
	public interface IMarkupExtension<out T> : IMarkupExtension
	{
		new T ProvideValue(IServiceProvider serviceProvider);
	}

	public interface IMarkupExtension
	{
		object ProvideValue(IServiceProvider serviceProvider);
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class AcceptEmptyServiceProviderAttribute : Attribute
	{
	}
}