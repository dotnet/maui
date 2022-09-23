using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Shapes;
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
		static bool _compatibilityBehaviorsConfigured;

		public static MauiAppBuilder ConfigureEffects(this MauiAppBuilder builder, Action<IEffectsBuilder> configureDelegate)
		{
			builder.Services.TryAddSingleton<EffectsFactory>();
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<EffectsRegistration>(new EffectsRegistration(configureDelegate));
			}

			return builder;
		}

		public static MauiAppBuilder ConfigureCompatibilityBehaviors(this MauiAppBuilder builder, Action<CompatibilityOptions> configure)
		{
			if (_compatibilityBehaviorsConfigured)
			{
				return builder;
			}

			var options = new CompatibilityOptions();
			configure(options);

			// Make sure this is available if anything needs it at runtime
			builder.Services.AddSingleton(options);

			// Update the mappings from Core to match the Controls behaviors carried over from old versions
			builder.RemapForControls(options);

			_compatibilityBehaviorsConfigured = true;

			return builder;
		}

		internal static MauiAppBuilder RemapForControls(this MauiAppBuilder builder, CompatibilityOptions flags)
		{
			// Update the mappings for IView/View to work specifically for Controls
			Application.RemapForControls();
			VisualElement.RemapForControls();
			Label.RemapForControls(flags);
			Button.RemapForControls();
			CheckBox.RemapForControls();
			DatePicker.RemapForControls();
			RadioButton.RemapForControls();
			FlyoutPage.RemapForControls();
			Toolbar.RemapForControls();
			Window.RemapForControls();
			Editor.RemapForControls();
			Entry.RemapForControls();
			Picker.RemapForControls();
			SearchBar.RemapForControls();
			TabbedPage.RemapForControls();
			TimePicker.RemapForControls();
			Layout.RemapForControls();
			ScrollView.RemapForControls();
			RefreshView.RemapForControls();
			Shape.RemapForControls();
			WebView.RemapForControls();

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