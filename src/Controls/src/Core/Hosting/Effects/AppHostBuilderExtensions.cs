using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureEffects(this MauiAppBuilder builder, Action<IEffectsBuilder> configureDelegate)
		{
			builder.Services.TryAddSingleton<EffectsFactory>();
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<EffectsRegistration>(new EffectsRegistration(configureDelegate));
			}

			return builder;
		}
	}

	internal class EffectsRegistration
	{
		private readonly Action<IEffectsBuilder> _registerEffects;

		public EffectsRegistration(Action<IEffectsBuilder> registerEffects)
		{
			_registerEffects = registerEffects;
		}

		internal void AddEffects(IEffectsBuilder effects)
		{
			_registerEffects(effects);
		}
	}

	internal class EffectCollectionBuilder : IEffectsBuilder
	{
		internal Dictionary<Type, Func<PlatformEffect>> RegisteredEffects { get; } = new Dictionary<Type, Func<PlatformEffect>>();

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

	internal class EffectsFactory
	{
		private readonly Dictionary<Type, Func<PlatformEffect>> _registeredEffects;

		public EffectsFactory(IEnumerable<EffectsRegistration> effectsRegistrations)
		{
			if (effectsRegistrations != null)
			{
				var effectsBuilder = new EffectCollectionBuilder();
				foreach (var effectRegistration in effectsRegistrations)
				{
					effectRegistration.AddEffects(effectsBuilder);
				}
				_registeredEffects = effectsBuilder.RegisteredEffects;
			}
		}

		internal PlatformEffect CreateEffect(Effect fromEffect)
		{
			if (_registeredEffects != null && _registeredEffects.TryGetValue(fromEffect.GetType(), out Func<PlatformEffect> effectType))
			{
				return effectType();
			}

			return null;
		}
	}
}