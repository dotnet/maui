using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		readonly WrappedServiceProvider _services;

#if __ANDROID__
		public MauiContext(IServiceProvider services, Android.Content.Context context)
			: this(services)
		{
			AddWeakSpecific(context);
		}
#endif

		public MauiContext(IServiceProvider services)
		{
			_services = new WrappedServiceProvider(services ?? throw new ArgumentNullException(nameof(services)));
		}

		public IServiceProvider Services => _services;

		public IMauiHandlersFactory Handlers =>
			Services.GetRequiredService<IMauiHandlersFactory>();

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

		class WrappedServiceProvider : IServiceProvider
		{
			readonly ConcurrentDictionary<Type, Func<object?>> _scopeStatic = new();

			public WrappedServiceProvider(IServiceProvider serviceProvider)
			{
				Inner = serviceProvider;
			}

			public IServiceProvider Inner { get; }

			public object? GetService(Type serviceType)
			{
				if (_scopeStatic.TryGetValue(serviceType, out var getter))
					return getter.Invoke();

				return Inner.GetService(serviceType);
			}

			public void AddSpecific(Type type, Func<object?> getter)
			{
				_scopeStatic[type] = getter;
			}
		}
	}
}