#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting
{
	internal static class CompatibilityCheck
	{
		// This code is currently here because RelativeLayout is still in Controls
		// I ran into issues with XamlC and moving RelativeLayout to compatibility
		// If that issue gets resolved then we can move this back to Compatibility
		static bool _compatibilityEnabled = false;
		internal static void CheckForCompatibility([CallerMemberName] string memberName = "")
		{
			if (!_compatibilityEnabled)
			{
				throw new InvalidOperationException(
					$"{memberName} is currently not enabled. To enable compatibility features you will need to call add `builder.UseMauiCompatibility()`.\n\n" +
					"MauiApp\n" +
					"	.CreateBuilder()\n" +
					"	.UseMauiApp<TApp>()\n" +
					"	.UseMauiCompatibility();\n"
					);
			}
		}

		internal static void ResetCompatibilityCheck() =>
			_compatibilityEnabled = false;

		internal static void UseCompatibility() =>
			_compatibilityEnabled = true;
	}

	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureEffects(this MauiAppBuilder builder, Action<IEffectsBuilder> configureDelegate)
		{
			builder.Services.TryAddSingleton(svc => new EffectsFactory(svc.GetServices<EffectsRegistration>()));
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

		public IEffectsBuilder Add(
			Type TEffect,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type TPlatformEffect)
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