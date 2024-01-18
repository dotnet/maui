using System;

namespace Microsoft.Maui.Hosting
{
	public interface IImageSourceServiceCollection : IMauiServiceCollection
	{
#pragma warning disable RS0016 // Symbol '...' is not part of the declared public API
		(Type ImageSourceType, Type ImageSourceServiceType) FindImageSourceToImageSourceServiceTypeMapping(Type imageSourceType);
		void AddImageSourceToImageSourceServiceTypeMapping(Type imageSourceType, Type imageSourceServiceType);
#pragma warning restore RS0016
	}
}