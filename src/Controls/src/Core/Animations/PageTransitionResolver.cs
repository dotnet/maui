#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// The outcome of resolving one navigation: MAUI's transition, and the platform animation the provider chain chose for it.
	/// </summary>
	internal sealed class ResolvedNavigationTransition
	{
		public ResolvedNavigationTransition(NavigationTransitionRequest request, PlatformNavigationTransition platform)
		{
			Request = request;
			Platform = platform;
		}

		public NavigationTransitionRequest Request { get; }

		public PlatformNavigationTransition Platform { get; }

		public PageTransition Transition => Request.Transition;

		/// <summary>Whether the platform's own animation runs unchanged.</summary>
		public bool IsPlatformDefault => Platform.IsPlatform;

		/// <summary>Whether the navigation runs without any animation.</summary>
		public bool IsSuppressed => Platform.IsSuppress;
	}

	/// <summary>
	/// Decides which transition a navigation runs (the resolution order on <see cref="PageTransitions"/>),
	/// records what ran so that going back can reverse it, and runs the provider chain.
	/// </summary>
	internal static class PageTransitionResolver
	{
		// What actually ran when a page was shown. Going back from the page reverses it.
		static readonly ConditionalWeakTable<Page, PageTransition> s_shown = new();

		// One-shot overrides from the per-navigation overloads, consumed by the next navigation of that page.
		static readonly ConditionalWeakTable<Page, PageTransition> s_pendingForward = new();
		static readonly ConditionalWeakTable<Page, PageTransition> s_pendingBack = new();

		// GoToAsync(state, transition) applies to every page that navigation pushes or pops.
		static readonly ConditionalWeakTable<Shell, ShellOverride> s_shellOverrides = new();

		static readonly ConditionalWeakTable<IServiceProvider, INavigationTransitionProvider[]> s_providers = new();

		/// <summary>Replaces the operating system's reduced-motion setting, for tests.</summary>
		internal static bool? ReducedMotionOverride { get; set; }

		internal static bool IsReducedMotion => ReducedMotionOverride ?? PageTransitionMotion.IsReducedMotionEnabled();

		/// <summary>
		/// Replaces MAUI's built-in transitions, for tests. When set, it is consulted after the app's providers in their place.
		/// </summary>
		internal static INavigationTransitionProvider? BuiltInOverride { get; set; }

		internal static void SetPendingForward(Page page, PageTransition transition)
			=> s_pendingForward.Put(page, transition.Snapshot());

		internal static void SetPendingBack(Page page, PageTransition transition)
			=> s_pendingBack.Put(page, transition.Snapshot());

		internal static object BeginShellOverride(Shell shell, PageTransition transition)
		{
			var entry = new ShellOverride(transition.Snapshot());
			s_shellOverrides.Put(shell, entry);
			return entry;
		}

		internal static void EndShellOverride(Shell shell, object token)
		{
			// A later GoToAsync may have replaced the entry; only remove our own.
			if (s_shellOverrides.TryGetValue(shell, out var current) && ReferenceEquals(current, token))
			{
				s_shellOverrides.Remove(shell);
			}
		}

		/// <summary>The transition recorded for <paramref name="page"/> when it was last shown.</summary>
		internal static PageTransition? GetShownTransition(Page page)
			=> s_shown.TryGetValue(page, out var transition) ? transition : null;

		/// <summary>
		/// Resolves a push, pop, modal push or pop, or root replacement, then runs the provider chain.
		/// Call it once per navigation: it consumes per-navigation overrides and records what ran.
		/// </summary>
		internal static ResolvedNavigationTransition Resolve(NavigationTransitionKind kind, Page? from, Page? to, bool animated, bool isInteractive = false)
		{
			bool isBack = kind is NavigationTransitionKind.Pop or NavigationTransitionKind.PopModal;
			bool isModal = kind is NavigationTransitionKind.PushModal or NavigationTransitionKind.PopModal;

			PageTransition transition = isBack
				? ResolveBack(from, to, isModal, animated)
				: ResolveForward(to, isModal, animated);

			var request = new NavigationTransitionRequest(kind, from, to, transition, isInteractive, isBack);
			return new ResolvedNavigationTransition(request, ResolvePlatform(request));
		}

		/// <summary>
		/// Resolves a switch to <paramref name="target"/>, a Shell section or item or a page of a TabbedPage.
		/// <paramref name="reverse"/> is set when moving to an earlier tab.
		/// </summary>
		internal static ResolvedNavigationTransition ResolveTab(Element? target, Page? from, Page? to, bool reverse, bool animated = true)
		{
			PageTransition transition = PageTransition.Platform;
			if (!animated)
			{
				transition = PageTransition.None;
			}
			else if (FindAttached(target, PageTransitions.TabTransitionProperty) is { } attached)
			{
				transition = attached.Snapshot();
			}

			transition = ApplyReducedMotion(transition);
			var request = new NavigationTransitionRequest(NavigationTransitionKind.TabSwitch, from, to, transition, false, reverse);
			return new ResolvedNavigationTransition(request, ResolvePlatform(request));
		}

		static PageTransition ResolveForward(Page? to, bool isModal, bool animated)
		{
			PageTransition? chosen = null;

			if (to is not null && s_pendingForward.TryGetValue(to, out var pending))
			{
				s_pendingForward.Remove(to);
				chosen = pending;
			}

			chosen ??= FindShellOverride(to);
			chosen ??= FindAttached(to, isModal ? PageTransitions.ModalTransitionProperty : PageTransitions.TransitionProperty);

			var ran = (chosen ?? PageTransition.Platform).Snapshot();

			// What going back reverses. An explicit "don't animate" isn't recorded as None: a page pushed
			// without animation still leaves with its own transition when it is popped with animation.
			if (to is not null)
			{
				s_shown.Put(to, ran);
			}

			return ApplyReducedMotion(animated ? ran : PageTransition.None);
		}

		static PageTransition ResolveBack(Page? from, Page? to, bool isModal, bool animated)
		{
			PageTransition? chosen = null;

			if (from is not null && s_pendingBack.TryGetValue(from, out var pending))
			{
				s_pendingBack.Remove(from);
				chosen = pending;
			}

			chosen ??= FindShellOverride(from) ?? FindShellOverride(to);

			if (chosen is null && from is not null && s_shown.TryGetValue(from, out var shown))
			{
				chosen = shown;
			}

			// A page that was never shown through a navigation (the root, or one inserted below the top)
			// reverses the transition it would have been shown with.
			chosen ??= FindAttached(from, isModal ? PageTransitions.ModalTransitionProperty : PageTransitions.TransitionProperty);

			var transition = (chosen ?? PageTransition.Platform).Snapshot();
			return ApplyReducedMotion(animated ? transition : PageTransition.None);
		}

		static PageTransition ApplyReducedMotion(PageTransition transition)
		{
			// The platform's own animation follows the platform's own reduced-motion handling.
			if (transition.IsPlatform || transition.IsNone)
			{
				return transition;
			}

			return IsReducedMotion ? PageTransition.None : transition;
		}

		/// <summary>The value of <paramref name="property"/> on <paramref name="element"/> or the nearest ancestor that sets it.</summary>
		internal static PageTransition? FindAttached(Element? element, BindableProperty property)
		{
			Window? window = (element as Page)?.Window;
			var current = element;
			while (current is not null)
			{
				if (current.IsSet(property) && current.GetValue(property) is PageTransition value)
				{
					return value;
				}

				if (current is Window)
				{
					window = null;
				}

				current = current.Parent;
			}

			// A modal page's parent isn't its window, so the window is checked last.
			if (window is not null && window.IsSet(property) && window.GetValue(property) is PageTransition fromWindow)
			{
				return fromWindow;
			}

			return null;
		}

		static PageTransition? FindShellOverride(Element? element)
		{
			var current = element;
			while (current is not null)
			{
				if (current is Shell shell && s_shellOverrides.TryGetValue(shell, out var entry))
				{
					return entry.Transition;
				}

				current = current.Parent;
			}

			// A page being popped is often already detached from its section.
			if (Shell.Current is { } active && s_shellOverrides.TryGetValue(active, out var activeEntry))
			{
				return activeEntry.Transition;
			}

			return null;
		}

		/// <summary>Runs the provider chain: the app's providers in order, then MAUI's built-in transitions.</summary>
		internal static PlatformNavigationTransition ResolvePlatform(NavigationTransitionRequest request)
		{
			// An explicit "don't animate", a None transition or reduced motion never reaches a provider.
			if (request.Transition.IsNone)
			{
				return PlatformNavigationTransition.Suppress;
			}

			var providers = GetProviders(request);
			for (int i = 0; i < providers.Length; i++)
			{
				var result = providers[i].GetTransition(request);
				if (result is not null)
				{
					return result;
				}
			}

			var builtIn = BuiltInOverride is { } testProvider
				? testProvider.GetTransition(request)
				: BuiltInNavigationTransitions.GetTransition(request);

			return builtIn ?? PlatformNavigationTransition.Platform;
		}

		static INavigationTransitionProvider[] GetProviders(NavigationTransitionRequest request)
		{
			var services = (request.To ?? request.From)?.FindMauiContext(true)?.Services
				?? Application.Current?.FindMauiContext()?.Services;

			if (services is null)
			{
				return Array.Empty<INavigationTransitionProvider>();
			}

			return s_providers.GetValue(services, static s => Collect(s));
		}

		static INavigationTransitionProvider[] Collect(IServiceProvider services)
		{
			var list = new List<INavigationTransitionProvider>();
			foreach (var provider in services.GetServices<INavigationTransitionProvider>())
			{
				if (provider is not null)
				{
					list.Add(provider);
				}
			}

			return list.ToArray();
		}

		sealed class ShellOverride
		{
			public ShellOverride(PageTransition transition) => Transition = transition;

			public PageTransition Transition { get; }
		}
	}

	static class ConditionalWeakTableExtensions
	{
		// ConditionalWeakTable.AddOrUpdate doesn't exist on netstandard2.0.
		internal static void Put<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> table, TKey key, TValue value)
			where TKey : class
			where TValue : class
		{
			table.Remove(key);
			table.Add(key, value);
		}
	}

	/// <summary>Whether the operating system asks apps to reduce motion.</summary>
	internal static partial class PageTransitionMotion
	{
		internal static bool IsReducedMotionEnabled() => IsReducedMotionEnabledPlatform();

		private static partial bool IsReducedMotionEnabledPlatform();
	}
}
