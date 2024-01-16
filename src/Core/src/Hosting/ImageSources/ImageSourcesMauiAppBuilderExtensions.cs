using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			builder.Services.AddKeyedSingleton<IImageSourceService>(typeof(IFileImageSource), (svcs, _) => new FileImageSourceService(svcs.CreateLogger<FileImageSourceService>()));
			builder.Services.AddKeyedSingleton<IImageSourceService>(typeof(IFontImageSource), (svcs, _) => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
			builder.Services.AddKeyedSingleton<IImageSourceService>(typeof(IStreamImageSource), (svcs, _) => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
			builder.Services.AddKeyedSingleton<IImageSourceService>(typeof(IUriImageSource), (svcs, _) => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
			builder.ConfigureImageSources(services =>
			{
				services.AddService<IFileImageSource>(svcs => new FileImageSourceService(svcs.CreateLogger<FileImageSourceService>()));
				services.AddService<IFontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
				services.AddService<IStreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
				services.AddService<IUriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
			});
			return builder;
		}

		public static MauiAppBuilder ConfigureImageSources(this MauiAppBuilder builder, Action<IImageSourceServiceCollection>? configureDelegate, bool transitionToKeyedServices = false)
		{
			if (configureDelegate != null)
			{
				if (transitionToKeyedServices)
					builder.Services.RegisterConfigureDelegateKeyedServices(configureDelegate);

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
		}

		class ImageSourceServiceCollection : MauiServiceCollection, IImageSourceServiceCollection
		{
		}

		/// <summary>
		/// RegisterConfigureDelegateKeyedServices facilitates the transition from registering custom ImageSource services under
		/// an IImageSourceServiceCollection via a configureDelegate to registering them under the MauiAppBuilder's
		/// ServiceCollection as keyed services. It constructs a keyed service for every non-keyed service that would have been
		/// registered by the configureDelegate, using the ImageSource service's underlying ImageSource type as the key.
		/// Services added to a service collection are described by a ServiceDescriptor, with the main differentiator between keyed
		/// and non-keyed services being the presence of a ServiceKey in the ServiceDescriptor. IImageSourceServiceCollection
		/// service types expected to be in the form IImageSourceService&lt;TImageSource&gt;, so the underlying ImageSource type
		/// is extracted from the original ServiceDescriptor's ServiceType, and a new ServiceDescriptor is constructed using
		/// the ImageSource type key in conjunction with the original ServiceDescriptor's properties.
		/// NOTE: RegisterConfigureDelegateKeyedServices only preserves services generated through the configureDelegate and
		/// cannot preserve non service registration actions or actions delayed to run when an ImageSourceService is retrieved.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configureDelegate"></param>
		/// <exception cref="InvalidOperationException"></exception>
		static void RegisterConfigureDelegateKeyedServices(this IServiceCollection services, Action<IImageSourceServiceCollection> configureDelegate)
		{
			var collection = new ImageSourceServiceCollection();
			configureDelegate(collection);

			foreach (ServiceDescriptor descriptor in collection)
			{
				if (descriptor.ServiceKey != null)
				{
					services.Add(descriptor);
					continue;
				}

				Type serviceType = descriptor.ServiceType;
				if (!serviceType.IsGenericType || serviceType.GetGenericTypeDefinition() != typeof(IImageSourceService<>))
					throw new InvalidOperationException($"Could not bootstrap {nameof(configureDelegate)} into keyed ImageSourceService registration. The service descriptor '{descriptor}' added through {nameof(configureDelegate)} must have a {nameof(descriptor.ServiceType)} that implements {nameof(IImageSourceService<IImageSource>)}.");

				Type imageSourceType = serviceType.GetGenericArguments()[0];
				if (!typeof(IImageSource).IsAssignableFrom(imageSourceType))
					throw new InvalidOperationException($"Could not bootstrap {nameof(configureDelegate)} into keyed ImageSourceService registration. The service descriptor '{descriptor}' added through {nameof(configureDelegate)} must have a {nameof(descriptor.ServiceType)} that implements {nameof(IImageSourceService<IImageSource>)}.");

				ServiceDescriptor? keyedDescriptor = null;
				if (descriptor.ImplementationType != null)
				{
					keyedDescriptor = new ServiceDescriptor(typeof(IImageSourceService), imageSourceType, descriptor.ImplementationType, descriptor.Lifetime);
				}
				else if (descriptor.ImplementationInstance != null)
				{
					keyedDescriptor = new ServiceDescriptor(typeof(IImageSourceService), imageSourceType, descriptor.ImplementationInstance);
				}
				else if (descriptor.ImplementationFactory != null)
				{
					keyedDescriptor = new ServiceDescriptor(typeof(IImageSourceService), imageSourceType, (svcs, _) => descriptor.ImplementationFactory(svcs), descriptor.Lifetime);
				}

				if (keyedDescriptor == null)
					throw new InvalidOperationException($"Could not bootstrap {nameof(configureDelegate)} into keyed ImageSourceService registration. The service descriptor '{descriptor}' added through {nameof(configureDelegate)} did not have an {nameof(descriptor.ImplementationType)}, an {nameof(descriptor.ImplementationFactory)} or an {nameof(descriptor.ImplementationInstance)}.");

				services.Add(keyedDescriptor);
			}
		}
	}
}
