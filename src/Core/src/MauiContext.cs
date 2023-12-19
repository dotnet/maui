using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		readonly WrappedServiceProvider _services;
		readonly Lazy<IMauiHandlersFactory> _handlers;

#if ANDROID
		readonly Lazy<Android.Content.Context?> _context;

		public Android.Content.Context? Context => _context.Value;

		public MauiContext(IServiceProvider services, Android.Content.Context context)
			: this(services)
		{
			AddWeakSpecific(context);
		}
#endif

		public MauiContext(IServiceProvider services)
		{
			_services = new WrappedServiceProvider(services ?? throw new ArgumentNullException(nameof(services)));
			_handlers = new Lazy<IMauiHandlersFactory>(() => _services.GetRequiredService<IMauiHandlersFactory>());
#if ANDROID
			_context = new Lazy<Android.Content.Context?>(() => _services.GetService<Android.Content.Context>());
#endif
		}

		public IServiceProvider Services => _services;

		public IMauiHandlersFactory Handlers => _handlers.Value;

		internal void AddSpecific<TService>(TService instance)
			where TService : class
		{
			_services.AddSpecific(typeof(TService), static state => state, instance);
		}

		internal void AddWeakSpecific<TService>(TService instance)
			where TService : class
		{
			_services.AddSpecific(typeof(TService), static state => ((WeakReference)state).Target, new WeakReference(instance));
		}

		class WrappedServiceProvider : IKeyedServiceProvider, IServiceProvider
		{
			readonly ConcurrentDictionary<Type, (object, Func<object, object?>)> _scopeStatic = new();

			public WrappedServiceProvider(IServiceProvider serviceProvider)
			{
				Inner = serviceProvider;
			}

			public IServiceProvider Inner { get; }

			public object? GetService(Type serviceType)
			{
				if (_scopeStatic.TryGetValue(serviceType, out var scope))
				{
					var (state, getter) = scope;
					return getter.Invoke(state);
				}

				return Inner.GetService(serviceType);
			}

			public object? GetKeyedService(Type serviceType, object? serviceKey)
			{
				if (Inner is IKeyedServiceProvider wrapped)
					return wrapped.GetKeyedService(serviceType, serviceKey);

				throw new InvalidOperationException($"WrappedServiceProvider's Inner service provider `{Inner.GetType()}` does not implement {nameof(IKeyedServiceProvider)}");
			}

			public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
			{
				if (Inner is IKeyedServiceProvider wrapped)
					return wrapped.GetRequiredKeyedService(serviceType, serviceKey);

				throw new InvalidOperationException($"WrappedServiceProvider's Inner service provider `{Inner.GetType()}` does not implement {nameof(IKeyedServiceProvider)}");
			}

			public void AddSpecific(Type type, Func<object, object?> getter, object state)
			{
				_scopeStatic[type] = (state, getter);
			}
		}
	}
}
