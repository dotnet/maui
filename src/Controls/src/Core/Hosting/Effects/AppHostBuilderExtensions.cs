using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureEffects(this IAppHostBuilder builder, Action<IEffectsBuilder> configureDelegate)
		{
			builder.ConfigureServices<EffectCollectionBuilder>(b => configureDelegate(b));
			return builder;
		}

		class EffectCollectionBuilder : IMauiServiceBuilder, IEffectsBuilder
		{
			internal Dictionary<Type, Func<PlatformEffect>> RegisteredEffects { get; } = new Dictionary<Type, Func<PlatformEffect>>();

			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
				services.AddSingleton<EffectsFactory>();
			}

			public void Configure(HostBuilderContext context, IServiceProvider services)
			{
				var effectsProvider = services.GetRequiredService<EffectsFactory>();
				effectsProvider.SetRegisteredEffects(RegisteredEffects);
			}

			public IEffectsBuilder Add<TEffect, TPlatformEffect>()
				where TEffect : RoutingEffect
				where TPlatformEffect : PlatformEffect, new()
			{
				RegisteredEffects.Add(typeof(TEffect), () =>
				{
					if (DependencyResolver.Resolve(typeof(TPlatformEffect)) is TPlatformEffect pe)
						return pe;

					return new TPlatformEffect();
				});
				return this;
			}

			public IEffectsBuilder Add(Type TEffect, Type TPlatformEffect)
			{
				RegisteredEffects.Add(TEffect, () =>
				{
					return (PlatformEffect)DependencyResolver.ResolveOrCreate(TPlatformEffect);
				});

				return this;
			}
		}
	}

	internal class EffectsFactory
	{
		Dictionary<Type, Func<PlatformEffect>> _registeredEffects;

		internal void SetRegisteredEffects(Dictionary<Type, Func<PlatformEffect>> registeredEffects)
		{
			_registeredEffects = registeredEffects;
		}

		internal PlatformEffect CreateEffect(Effect fromEffect)
		{
			if (_registeredEffects.TryGetValue(fromEffect.GetType(), out Func<PlatformEffect> effectType))
			{
				return effectType();
			}

			return null;
		}
	}
}