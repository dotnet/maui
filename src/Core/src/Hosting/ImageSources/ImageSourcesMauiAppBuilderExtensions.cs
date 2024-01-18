using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static class ImageSourcesMauiAppBuilderExtensions
	{
		public static MauiAppBuilder ConfigureImageSources(this MauiAppBuilder builder)
		{
			builder.ConfigureImageSources(services =>
			{
				services.AddService<IFileImageSource>(svcs => new FileImageSourceService(svcs.CreateLogger<FileImageSourceService>()));
				services.AddService<IFontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
				services.AddService<IStreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
				services.AddService<IUriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
			});
			return builder;
		}

		public static MauiAppBuilder ConfigureImageSources(this MauiAppBuilder builder, Action<IImageSourceServiceCollection>? configureDelegate)
		{
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<ImageSourceRegistration>(new ImageSourceRegistration(configureDelegate));
			}

			builder.Services.TryAddSingleton<IImageSourceServiceProvider>(svcs => new ImageSourceServiceProvider(svcs.GetRequiredService<IImageSourceServiceCollection>(), svcs));
			builder.Services.TryAddSingleton<IImageSourceServiceCollection>(svcs => new ImageSourceServiceBuilder(svcs.GetServices<ImageSourceRegistration>()));

			return builder;
		}

		class ImageSourceRegistration
		{
			private readonly Action<IImageSourceServiceCollection> _registerAction;

			public ImageSourceRegistration(Action<IImageSourceServiceCollection> registerAction)
			{
				_registerAction = registerAction;
			}

			internal void AddRegistration(IImageSourceServiceCollection builder)
			{
				_registerAction(builder);
			}
		}

		class ImageSourceServiceBuilder : MauiServiceCollection, IImageSourceServiceCollection
		{
			private Dictionary<Type, Type> _typeMappings { get; } = new();

			public ImageSourceServiceBuilder(IEnumerable<ImageSourceRegistration> registrationActions)
			{
				if (registrationActions != null)
				{
					foreach (var effectRegistration in registrationActions)
					{
						effectRegistration.AddRegistration(this);
					}
				}
			}

			public (Type ImageSourceType, Type ImageSourceServiceType) FindImageSourceToImageSourceServiceTypeMapping(Type type)
			{
				List<(Type ImageSourceType, Type ImageSourceServiceType)> matches = new();

				foreach (var mapping in _typeMappings)
				{
					var imageSourceType = mapping.Key;
					if (imageSourceType.IsAssignableFrom(type) || type.IsAssignableFrom(imageSourceType))
					{
						var imageSourceServiceType = mapping.Value;
						matches.Add((imageSourceType, imageSourceServiceType));
					}
				}

				if (matches.Count == 0)
				{
					throw new InvalidOperationException($"Unable to find any configured {nameof(IImageSource)} corresponding to {type.Name}.");
				}
				
				return SelectTheMostSpecificMatch(matches);
			}


			public void AddImageSourceToImageSourceServiceTypeMapping(Type imageSourceType, Type imageSourceServiceType)
			{
				Debug.Assert(typeof(IImageSource).IsAssignableFrom(imageSourceType));
				Debug.Assert(typeof(IImageSourceService).IsAssignableFrom(imageSourceServiceType));

				_typeMappings[imageSourceType] = imageSourceServiceType;
			}

			private static (Type ImageSourceType, Type ImageSourceServiceType) SelectTheMostSpecificMatch(List<(Type ImageSourceType, Type ImageSourceServiceType)> matches)
			{
				var bestImageSourceTypeMatch = matches[0].ImageSourceType;
				var bestImageSourceServiceTypeMatch = matches[0].ImageSourceServiceType;

				foreach (var (imageSourceType, imageSourceServiceType) in matches.Skip(1))
				{
					if (imageSourceType.IsAssignableFrom(bestImageSourceTypeMatch)
						|| bestImageSourceTypeMatch.IsInterface && imageSourceType.IsClass)
					{
						bestImageSourceTypeMatch = imageSourceType;
						bestImageSourceServiceTypeMatch = imageSourceServiceType;
					}

					// TODO we could still improve this to detect truly ambiguous cases, like:
					// - FileImageSourceA (implements IFileImageSource) -> X
					// - FileImageSourceB (implements IFileImageSource) -> Y
					// -> find closest match to `IFileImageSource`
				}

				return (bestImageSourceTypeMatch, bestImageSourceServiceTypeMatch);
			}
		}
	}
}
