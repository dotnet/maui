#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
		static readonly ConditionalWeakTable<Page, PendingTransitions> s_pending = new();

		// GoToAsync(state, transition) applies to every page that navigation pushes or pops, and only to that navigation.
		// It flows with the request's own async flow rather than being stored on the Shell, so overlapping GoToAsync
		// calls (one suspended in Navigating, one presenting a multi-page route) never see each other's transition,
		// and a navigation started later from inside it (a handler) doesn't inherit it once the request has ended.
		static readonly System.Threading.AsyncLocal<ShellOverride?> s_shellOverride = new();

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
			// Several requests for the same page can be outstanding (one queued behind a running navigation, and another
			// that then fails); each keeps its own entry, and navigations consume them in the order they were made.
			var entry = new PendingTransition(kind, transition.Snapshot());
			s_pending.GetValue(page, static _ => new PendingTransitions()).Entries.Add(entry);
			return entry;
		}

		/// <summary>
		/// Removes a staged transition the navigation didn't consume (it was a no-op, failed, or the platform never asked).
		/// Other requests' entries are left alone.
		/// </summary>
		internal static void ClearPending(Page page, object token)
		{
			if (s_pending.TryGetValue(page, out var pending) && token is PendingTransition entry)
			{
				pending.Entries.Remove(entry);
				if (pending.Entries.Count == 0)
				{
					s_pending.Remove(page);
				}
			}
		}

		internal static bool HasPending(Page page) => s_pending.TryGetValue(page, out var pending) && pending.Entries.Count > 0;

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
				// Ahead of anything staged since: this is the request the platform runs next.
				s_pending.GetValue(page, static _ => new PendingTransitions()).Entries.Insert(0, new PendingTransition(kind, transition));
			}
		}

		// PopModalAsync(transition): which modal it dismisses is only known once the modal manager has waited for any
		// presentation in progress, so the transition flows with the call and is attached to the modal selected then.
		static readonly System.Threading.AsyncLocal<PageTransition?> s_modalPopTransition = new();

		internal static PageTransition? ModalPopTransition
		{
			get => s_modalPopTransition.Value;
			set => s_modalPopTransition.Value = value;
		}

		/// <summary>
		/// Stages the running GoToAsync transition of this flow (if <paramref name="shell"/> has one) on the page a Shell
		/// section is about to push or pop, so a platform that applies the request later, outside the request's flow (an
		/// Android navigation queued behind the one running), still resolves it. Returns a token for
		/// <see cref="ClearPendingWhenDone"/>, or <see langword="null"/> when nothing was staged.
		/// </summary>
		internal static object? StageShellOverride(Shell? shell, Page? page, NavigationTransitionKind kind)
			=> shell is not null && page is not null && OverrideFor(shell) is { } transition
				? SetPending(page, kind, transition)
				: null;

		/// <summary>
		/// Removes the staged entry <paramref name="token"/> once <paramref name="navigation"/> has ended (at once when it
		/// is <see langword="null"/>), if the platform never consumed it. A navigation that faults is the caller's to observe.
		/// </summary>
		internal static async void ClearPendingWhenDone(Page page, object token, Task? navigation)
		{
			try
			{
				if (navigation is not null)
				{
					await navigation;
				}
			}
			catch (Exception)
			{
				// The navigation's own caller observes its fault; only the staged entry is this method's business.
			}
			finally
			{
				ClearPending(page, token);
			}
		}

		/// <summary>The GoToAsync transition of the Shell navigation running for <paramref name="page"/>, if any.</summary>
		internal static PageTransition? GetShellOverride(Element? page) => FindShellOverride(page);

		static PageTransition? TakePending(Page? page, NavigationTransitionKind kind)
		{
			if (page is null || !s_pending.TryGetValue(page, out var pending))
			{
				return null;
			}

			var entries = pending.Entries;
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].Kind == kind)
				{
					var transition = entries[i].Transition;
					entries.RemoveAt(i);
					if (entries.Count == 0)
					{
						s_pending.Remove(page);
					}

					return transition;
				}
			}

			return null;
		}

		/// <summary>
		/// Opens a Shell request's scope on this flow: <paramref name="transition"/> applies to the pages it moves, and a
		/// request without one (<see langword="null"/>) masks any enclosing request's transition, so a navigation started
		/// from inside another (its Navigated handler, say) never inherits that one's override.
		/// </summary>
		internal static object BeginShellOverride(Shell shell, PageTransition? transition)
		{
			var entry = new ShellOverride(shell, transition?.Snapshot(), s_shellOverride.Value);
			s_shellOverride.Value = entry;
			return entry;
		}

		/// <summary>
		/// The Shell request scopes of this flow, for carrying across a boundary that doesn't flow the execution context
		/// (Android's dispatcher posts a bare callback): pass it to <see cref="RestoreShellFlow"/> on the other side.
		/// </summary>
		internal static object? CaptureShellFlow() => s_shellOverride.Value;

		/// <summary>
		/// Puts the request scopes <paramref name="captured"/> by <see cref="CaptureShellFlow"/> back on this flow until the
		/// returned scope is disposed; async work started meanwhile keeps them. The scopes still end with their requests.
		/// </summary>
		internal static ShellFlowRestoration RestoreShellFlow(object? captured)
		{
			var previous = s_shellOverride.Value;
			s_shellOverride.Value = captured as ShellOverride;
			return new ShellFlowRestoration(previous);
		}

		internal readonly struct ShellFlowRestoration : IDisposable
		{
			readonly ShellOverride? _previous;

			public ShellFlowRestoration(object? previous) => _previous = previous as ShellOverride;

			public void Dispose() => s_shellOverride.Value = _previous;
		}

		internal static void EndShellOverride(Shell shell, object token)
		{
			if (token is not ShellOverride entry)
			{
				return;
			}

			// Ended for every flow that captured it (a continuation or handler still holding it sees it as over), and
			// this flow goes back to whatever enclosed the request.
			entry.Ended = true;
			if (ReferenceEquals(s_shellOverride.Value, entry))
			{
				s_shellOverride.Value = entry.Previous;
			}
		}

		// The transition of the innermost running Shell request of this flow that belongs to shell: null when that request
		// has none, which masks any request enclosing it.
		static PageTransition? OverrideFor(Shell shell)
		{
			var entry = s_shellOverride.Value;
			while (entry is not null && (entry.Ended || !entry.IsFor(shell)))
			{
				entry = entry.Previous;
			}

			return entry?.Transition;
		}

		/// <summary>The flow direction recorded for <paramref name="page"/> when it was last shown.</summary>
		internal static FlowDirection? GetShownFlowDirection(Page page)
			=> s_shown.TryGetValue(page, out var shown) ? shown.FlowDirection : null;

		/// <summary>
		/// Records <paramref name="transition"/> as what actually showed <paramref name="page"/> when the built-in
		/// transitions substitute one for the resolved transition (the crossfade heroes fly over), so going back reverses
		/// what ran even if the heroes no longer match. The recorded flow direction is kept.
		/// </summary>
		internal static void RecordShown(Page page, PageTransition transition)
		{
			var flowDirection = s_shown.TryGetValue(page, out var shown) ? shown.FlowDirection : FlowDirectionOf(page);
			s_shown.Put(page, new ShownTransition(transition, flowDirection));
		}

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
				if (current is Shell shell && OverrideFor(shell) is { } fromShell)
				{
					return fromShell;
				}

				// A modal (and a stack inside a modal NavigationPage) is a child of the window, not of the Shell that
				// routed to it: the override of the Shell that is this window's page applies.
				if (current is Window { Page: Shell windowShell } && OverrideFor(windowShell) is { } fromWindowShell)
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

		sealed class PendingTransitions
		{
			public List<PendingTransition> Entries { get; } = new();
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

		sealed class ShellOverride
		{
			// Weak: an execution context captured by something long-lived must not keep the Shell alive.
			readonly WeakReference<Shell> _shell;

			public ShellOverride(Shell shell, PageTransition? transition, ShellOverride? previous)
			{
				_shell = new WeakReference<Shell>(shell);
				Transition = transition;
				Previous = previous;
			}

			// Null for a request without its own transition: it masks the requests enclosing it.
			public PageTransition? Transition { get; }

			public ShellOverride? Previous { get; }

			public bool Ended { get; set; }

			public bool IsFor(Shell shell) => _shell.TryGetTarget(out var target) && ReferenceEquals(target, shell);
		}
	}

	/// <summary>
	/// Installs a Shell navigation's own page transition for as long as that navigation runs. Every request has one: a
	/// request without a transition masks the transition of a request it was started from.
	/// </summary>
	internal readonly struct ShellTransitionScope : IDisposable
	{
		readonly Shell? _shell;
		readonly object? _token;

		public ShellTransitionScope(Shell? shell, PageTransition? transition)
		{
			_shell = shell;
			_token = shell is not null ? PageTransitionResolver.BeginShellOverride(shell, transition) : null;
		}

		public void Dispose()
		{
			if (_shell is not null && _token is not null)
			{
				PageTransitionResolver.EndShellOverride(_shell, _token);
			}
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
