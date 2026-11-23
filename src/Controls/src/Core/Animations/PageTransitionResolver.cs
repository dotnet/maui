#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		// What actually ran when a page was shown, and the flow direction it ran in. Going back from the page reverses it.
		static readonly ConditionalWeakTable<Page, ShownTransition> s_shown = new();

		// One-shot overrides from the per-navigation overloads, consumed only by the navigation of that page and kind
		// they were staged for. The overload removes an entry the navigation didn't consume once its task ends.
		static readonly ConditionalWeakTable<Page, PendingTransition> s_pending = new();

		// GoToAsync(state, transition) applies to every page that navigation pushes or pops.
		// Kept as a stack per Shell: a GoToAsync suspended in Navigating keeps its override while a later one runs
		// (or fails), and each removes only its own entry.
		static readonly ConditionalWeakTable<Shell, ShellOverrides> s_shellOverrides = new();

		static readonly ConditionalWeakTable<IServiceProvider, INavigationTransitionProvider[]> s_providers = new();

		/// <summary>Replaces the operating system's reduced-motion setting, for tests.</summary>
		internal static bool? ReducedMotionOverride { get; set; }

		internal static bool IsReducedMotion => ReducedMotionOverride ?? PageTransitionMotion.IsReducedMotionEnabled();

		/// <summary>
		/// Replaces MAUI's built-in transitions, for tests. When set, it is consulted after the app's providers in their place.
		/// </summary>
		internal static INavigationTransitionProvider? BuiltInOverride { get; set; }

		/// <summary>
		/// Stages <paramref name="transition"/> for the next <paramref name="kind"/> navigation of <paramref name="page"/>.
		/// Returns a token for <see cref="ClearPending"/>.
		/// </summary>
		internal static object SetPending(Page page, NavigationTransitionKind kind, PageTransition transition)
		{
			var entry = new PendingTransition(kind, transition.Snapshot());
			s_pending.Put(page, entry);
			return entry;
		}

		/// <summary>
		/// Removes a staged transition the navigation didn't consume (it was a no-op, failed, or the platform never asked),
		/// unless a newer navigation has staged its own since.
		/// </summary>
		internal static void ClearPending(Page page, object token)
		{
			if (s_pending.TryGetValue(page, out var current) && ReferenceEquals(current, token))
			{
				s_pending.Remove(page);
			}
		}

		internal static bool HasPending(Page page) => s_pending.TryGetValue(page, out _);

		/// <summary>
		/// Takes the transition staged for <paramref name="page"/>'s <paramref name="kind"/> navigation, for a navigation the
		/// platform runs later (a modal request deferred until the window is ready). Stage it again with
		/// <see cref="Restage"/> just before the platform runs it.
		/// </summary>
		internal static PageTransition? TakePendingTransition(Page page, NavigationTransitionKind kind) => TakePending(page, kind);

		/// <summary>Drops a restaged transition the platform didn't consume.</summary>
		internal static void DropPending(Page page, NavigationTransitionKind kind) => TakePending(page, kind);

		internal static void Restage(Page page, NavigationTransitionKind kind, PageTransition? transition)
		{
			if (transition is not null)
			{
				s_pending.Put(page, new PendingTransition(kind, transition));
			}
		}

		static PageTransition? TakePending(Page? page, NavigationTransitionKind kind)
		{
			if (page is not null && s_pending.TryGetValue(page, out var pending) && pending.Kind == kind)
			{
				s_pending.Remove(page);
				return pending.Transition;
			}

			return null;
		}

		internal static object BeginShellOverride(Shell shell, PageTransition transition)
		{
			var entry = new ShellOverride(transition.Snapshot());
			s_shellOverrides.GetValue(shell, static _ => new ShellOverrides()).Entries.Add(entry);
			return entry;
		}

		internal static void EndShellOverride(Shell shell, object token)
		{
			if (s_shellOverrides.TryGetValue(shell, out var overrides) && token is ShellOverride entry)
			{
				overrides.Entries.Remove(entry);
				if (overrides.Entries.Count == 0)
				{
					s_shellOverrides.Remove(shell);
				}
			}
		}

		static PageTransition? LatestOverride(Shell shell)
			=> s_shellOverrides.TryGetValue(shell, out var overrides) && overrides.Entries.Count > 0
				? overrides.Entries[overrides.Entries.Count - 1].Transition
				: null;

		/// <summary>The flow direction recorded for <paramref name="page"/> when it was last shown.</summary>
		internal static FlowDirection? GetShownFlowDirection(Page page)
			=> s_shown.TryGetValue(page, out var shown) ? shown.FlowDirection : null;

		/// <summary>The transition recorded for <paramref name="page"/> when it was last shown.</summary>
		internal static PageTransition? GetShownTransition(Page page)
			=> s_shown.TryGetValue(page, out var shown) ? shown.Transition : null;

		/// <summary>
		/// Resolves a push, pop, or modal push or pop, then runs the provider chain.
		/// Call it once per navigation: it consumes per-navigation overrides and records what ran.
		/// </summary>
		internal static ResolvedNavigationTransition Resolve(NavigationTransitionKind kind, Page? from, Page? to, bool animated, bool isInteractive = false)
		{
			var request = ResolveRequest(kind, from, to, animated, isInteractive);
			return new ResolvedNavigationTransition(request, ResolvePlatform(request));
		}

		/// <summary>
		/// Resolves which transition a navigation runs now (consuming per-navigation arguments and recording what ran), and
		/// leaves the provider chain until <see cref="PreparedNavigationTransition.Platform"/> is first read, so a platform
		/// override that owns the animation and never asks the base keeps providers out of the navigation.
		/// </summary>
		internal static PreparedNavigationTransition Prepare(NavigationTransitionKind kind, Page? from, Page? to, bool animated)
			=> new PreparedNavigationTransition(ResolveRequest(kind, from, to, animated));

		/// <summary>
		/// Resolves which transition a navigation runs, without consulting providers (root replacement has no platform
		/// animation for a provider to supply).
		/// </summary>
		internal static NavigationTransitionRequest ResolveRequest(NavigationTransitionKind kind, Page? from, Page? to, bool animated, bool isInteractive = false, FlowDirection? forwardFlowDirection = null)
		{
			bool isBack = kind is NavigationTransitionKind.Pop or NavigationTransitionKind.PopModal;
			bool isModal = kind is NavigationTransitionKind.PushModal or NavigationTransitionKind.PopModal;

			var (transition, flowDirection) = isBack
				? ResolveBack(kind, from, to, isModal, animated)
				: ResolveForward(kind, to, isModal, animated, forwardFlowDirection);

			return new NavigationTransitionRequest(kind, from, to, transition, isInteractive, isBack, flowDirection);
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
			var request = new NavigationTransitionRequest(NavigationTransitionKind.TabSwitch, from, to, transition, false, reverse, FlowDirectionOf(to ?? from));
			return new ResolvedNavigationTransition(request, ResolvePlatform(request));
		}

		/// <summary>The flow direction a page lays out in.</summary>
		internal static FlowDirection FlowDirectionOf(Element? page)
		{
			var effective = page switch
			{
				IVisualElementController visual => visual.EffectiveFlowDirection,
				IFlowDirectionController window => window.EffectiveFlowDirection,
				_ => default,
			};

			return effective.IsRightToLeft() ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
		}

		static (PageTransition, FlowDirection) ResolveForward(NavigationTransitionKind kind, Page? to, bool isModal, bool animated, FlowDirection? flowDirectionOverride)
		{
			PageTransition? chosen = TakePending(to, kind);
			chosen ??= FindShellOverride(to);
			chosen ??= FindAttached(to, isModal ? PageTransitions.ModalTransitionProperty : PageTransitions.TransitionProperty);

			var ran = (chosen ?? PageTransition.Platform).Snapshot();
			var flowDirection = flowDirectionOverride ?? FlowDirectionOf(to);

			// What going back reverses. An explicit "don't animate" isn't recorded as None: a page pushed
			// without animation still leaves with its own transition when it is popped with animation.
			if (to is not null)
			{
				s_shown.Put(to, new ShownTransition(ran, flowDirection));
			}

			return (ApplyReducedMotion(animated ? ran : PageTransition.None), flowDirection);
		}

		static (PageTransition, FlowDirection) ResolveBack(NavigationTransitionKind kind, Page? from, Page? to, bool isModal, bool animated)
		{
			PageTransition? chosen = TakePending(from, kind);
			chosen ??= FindShellOverride(from) ?? FindShellOverride(to);

			// Going back runs in the flow direction the page was shown in, whatever the destination uses.
			var flowDirection = FlowDirectionOf(from);
			if (from is not null && s_shown.TryGetValue(from, out var shown))
			{
				chosen ??= shown.Transition;
				flowDirection = shown.FlowDirection;
			}

			// A page that was never shown through a navigation (the root, or one inserted below the top)
			// reverses the transition it would have been shown with.
			chosen ??= FindAttached(from, isModal ? PageTransitions.ModalTransitionProperty : PageTransitions.TransitionProperty);

			var transition = (chosen ?? PageTransition.Platform).Snapshot();
			return (ApplyReducedMotion(animated ? transition : PageTransition.None), flowDirection);
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
				if (current is Shell shell && LatestOverride(shell) is { } fromShell)
				{
					return fromShell;
				}

				// A modal (and a stack inside a modal NavigationPage) is a child of the window, not of the Shell that
				// routed to it: the override of the Shell that is this window's page applies.
				if (current is Window { Page: Shell windowShell } && LatestOverride(windowShell) is { } fromWindowShell)
				{
					return fromWindowShell;
				}

				current = current.Parent;
			}

			// Only a Shell that owns the page counts: never Shell.Current, which throws with several windows and may
			// belong to another window entirely. A popped page detached from its section is resolved through the
			// destination page instead (the caller tries both).
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

		sealed class ShownTransition
		{
			public ShownTransition(PageTransition transition, FlowDirection flowDirection)
			{
				Transition = transition;
				FlowDirection = flowDirection;
			}

			public PageTransition Transition { get; }

			public FlowDirection FlowDirection { get; }
		}

		sealed class PendingTransition
		{
			public PendingTransition(NavigationTransitionKind kind, PageTransition transition)
			{
				Kind = kind;
				Transition = transition;
			}

			public NavigationTransitionKind Kind { get; }

			public PageTransition Transition { get; }
		}

		sealed class ShellOverrides
		{
			public List<ShellOverride> Entries { get; } = new();
		}

		sealed class ShellOverride
		{
			public ShellOverride(PageTransition transition) => Transition = transition;

			public PageTransition Transition { get; }
		}
	}

	/// <summary>A navigation whose transition is resolved and whose provider-chain result is computed on first use.</summary>
	internal sealed class PreparedNavigationTransition
	{
		PlatformNavigationTransition? _platform;

		public PreparedNavigationTransition(NavigationTransitionRequest request) => Request = request;

		public NavigationTransitionRequest Request { get; }

		public bool HasResolvedPlatform => _platform is not null;

		public PlatformNavigationTransition Platform => _platform ??= PageTransitionResolver.ResolvePlatform(Request);
	}

	static class ConditionalWeakTableExtensions
	{
		// ConditionalWeakTable.AddOrUpdate doesn't exist on netstandard2.0.
		internal static void Put<TKey, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TValue>(this ConditionalWeakTable<TKey, TValue> table, TKey key, TValue value)
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
