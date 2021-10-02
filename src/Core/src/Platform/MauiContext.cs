using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		readonly WrappedServiceProvider _services;
		readonly IMauiContext? _parent;

#if __ANDROID__
		public MauiContext(IServiceProvider services, Android.App.Application application, IMauiContext? parent = null)
			: this(services, (Android.Content.Context)application, parent)
		{
			AddSpecific(application);
		}

		public MauiContext(IServiceProvider services, Android.Content.Context context, IMauiContext? parent = null)
			: this(services, parent)
		{
			AddWeakSpecific(context);
		}
#elif __IOS__
		public MauiContext(IServiceProvider services, UIKit.UIApplicationDelegate application, IMauiContext? parent = null)
			: this(services, parent)
		{
			AddSpecific(application);
		}
#elif WINDOWS
		public MauiContext(IServiceProvider services, UI.Xaml.Application application, IMauiContext? parent = null)
			: this(services, parent)
		{
			AddSpecific(application);
		}
#endif

		public MauiContext(IMauiContext parent)
			: this(parent.Services, parent)
		{
		}

		public MauiContext(IServiceProvider services, IMauiContext? parent = null)
		{
			_services = new WrappedServiceProvider(services ?? throw new ArgumentNullException(nameof(services)));
			_parent = parent;

			// the animation manager should only be fetched once per context
			// TODO: maybe this should be set from outside?
			AddSpecific(() => services.GetRequiredService<IAnimationManager>());
		}

		public IServiceProvider Services => _services;

		public IMauiHandlersServiceProvider Handlers =>
			Services.GetRequiredService<IMauiHandlersServiceProvider>();

#if __ANDROID__
		public Android.Content.Context? Context =>
			Services.GetService<Android.Content.Context>();
#endif

		internal void AddSpecific<TService>(TService instance)
			where TService : class
		{
			_services.AddSpecific(typeof(TService), () => instance);
		}

		internal void AddWeakSpecific<TService>(TService instance)
			where TService : class
		{
			var weak = new WeakReference(instance);

			_services.AddSpecific(typeof(TService), () => weak.Target);
		}

		internal void AddSpecific<TService>(Func<TService> factory)
			where TService : class
		{
			var lazy = new Lazy<TService>(() => factory());

			_services.AddSpecific(typeof(TService), () => lazy.Value);
		}

		internal void AddSpecific<TService, TImplementation>(Func<TImplementation> factory)
			where TService : class
			where TImplementation : class, TService
		{
			var lazy = new Lazy<TImplementation>(() => factory());

			_services.AddSpecific(typeof(TService), () => lazy.Value);
		}

		class WrappedServiceProvider : IServiceProvider
		{
			readonly IServiceProvider _serviceProvider;
			readonly ConcurrentDictionary<Type, Func<object?>> _scopeStatic = new();

			public WrappedServiceProvider(IServiceProvider serviceProvider)
			{
				_serviceProvider = serviceProvider;
			}

			public object? GetService(Type serviceType)
			{
				if (_scopeStatic.TryGetValue(serviceType, out var getter))
					return getter.Invoke();

				return _serviceProvider.GetService(serviceType);
			}

			public void AddSpecific(Type type, Func<object?> getter)
			{
				_scopeStatic[type] = getter;
			}
		}
	}
}