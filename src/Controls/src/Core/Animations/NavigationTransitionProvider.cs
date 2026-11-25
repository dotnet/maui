#nullable enable
using System;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Supplies the platform animation for a navigation.
	/// </summary>
	/// <remarks>
	/// <para>Register providers in the app's service collection when the app is built
	/// (<c>builder.Services.AddSingleton&lt;INavigationTransitionProvider, MyProvider&gt;()</c>). They are
	/// consulted in registration order for every animated navigation, ahead of MAUI's own built-in
	/// transitions, which are a provider of their own and always come last.</para>
	/// <para>A provider can <b>handle</b> a navigation by returning a <see cref="PlatformNavigationTransition"/>,
	/// <b>suppress</b> its animation by returning <see cref="PlatformNavigationTransition.Suppress"/>,
	/// keep the platform's own animation by returning <see cref="PlatformNavigationTransition.Platform"/>,
	/// or <b>decline</b> it by returning <see langword="null"/>, which passes it to the next provider.</para>
	/// <para>Providers are consulted for stack navigations, modals and tab switches. Replacing a window's root page
	/// (<see cref="PageTransitions.ReplacePageAsync"/>) has no platform animation for a provider to supply, so it always
	/// runs MAUI's own motion.</para>
	/// <para>Overrides of the navigation virtual methods on MAUI's handlers (for example
	/// <c>StackNavigationManager.OnCreateNavigationAnimation</c> on Android or
	/// <c>StackNavigationManager.GetNavigationTransition</c> on Windows) keep working and take precedence:
	/// providers are consulted from the base implementations.</para>
	/// </remarks>
	public interface INavigationTransitionProvider
	{
		/// <summary>
		/// Returns the animation for <paramref name="request"/>, or <see langword="null"/> to let the next provider decide.
		/// </summary>
		PlatformNavigationTransition? GetTransition(NavigationTransitionRequest request);
	}

	/// <summary>
	/// The kind of navigation a transition is requested for.
	/// </summary>
	public enum NavigationTransitionKind
	{
		/// <summary>A page pushed onto a <see cref="NavigationPage"/> or Shell section stack.</summary>
		Push,

		/// <summary>A page popped off a <see cref="NavigationPage"/> or Shell section stack.</summary>
		Pop,

		/// <summary>A page presented modally.</summary>
		PushModal,

		/// <summary>A modal page dismissed.</summary>
		PopModal,

		/// <summary>A switch between Shell sections or tabs, or between the pages of a <see cref="TabbedPage"/>.</summary>
		TabSwitch,

	}

	/// <summary>
	/// Describes a navigation that needs a transition.
	/// </summary>
	public sealed class NavigationTransitionRequest
	{
		internal NavigationTransitionRequest(NavigationTransitionKind kind, Page? from, Page? to, PageTransition transition, bool isInteractive, bool isBack, FlowDirection flowDirection)
		{
			IsBack = isBack;
			FlowDirection = flowDirection;
			Kind = kind;
			From = from;
			To = to;
			Transition = transition ?? throw new ArgumentNullException(nameof(transition));
			IsInteractive = isInteractive;
		}

		/// <summary>The kind of navigation.</summary>
		public NavigationTransitionKind Kind { get; }

		/// <summary>The page being hidden or removed, when it is known.</summary>
		public Page? From { get; }

		/// <summary>The page being shown, when it is known.</summary>
		public Page? To { get; }

		/// <summary>
		/// The transition MAUI resolved for this navigation: the per-navigation argument, the page's
		/// attached <see cref="PageTransitions.TransitionProperty"/> (or its ancestors'), or
		/// <see cref="PageTransition.Platform"/>. For a back navigation this is the transition that ran
		/// when the page was shown, which is to be played in reverse. Its values are a snapshot.
		/// </summary>
		public PageTransition Transition { get; }

		/// <summary>
		/// Whether the transition runs in reverse: a back navigation (<see cref="NavigationTransitionKind.Pop"/> or
		/// <see cref="NavigationTransitionKind.PopModal"/>), or a <see cref="NavigationTransitionKind.TabSwitch"/> to an earlier tab.
		/// </summary>
		public bool IsBack { get; }

		/// <summary>
		/// Whether a back gesture (the iOS swipe-back) drives the transition. A transition returned for
		/// an interactive request is scrubbed by the finger and can be cancelled.
		/// </summary>
		public bool IsInteractive { get; }

		/// <summary>
		/// The flow direction the transition is laid out in: the page being shown's, or for a back navigation the
		/// direction the page was shown in, so it leaves the way it came.
		/// </summary>
		public FlowDirection FlowDirection { get; }
	}

	/// <summary>
	/// The platform animation a <see cref="INavigationTransitionProvider"/> returns for a navigation.
	/// </summary>
	/// <remarks>
	/// Each platform adds a constructor that takes its native animation type: an
	/// <c>IUIViewControllerAnimatedTransitioning</c> on iOS and Mac Catalyst, a factory of
	/// <c>Android.Views.Animations.Animation</c> on Android, and a <c>NavigationTransitionInfo</c> on Windows.
	/// </remarks>
	public sealed partial class PlatformNavigationTransition
	{
		readonly PlatformNavigationTransitionKind _kind;

		PlatformNavigationTransition(PlatformNavigationTransitionKind kind)
		{
			_kind = kind;
		}

		/// <summary>Stops the provider chain and keeps the platform's own animation.</summary>
		public static PlatformNavigationTransition Platform { get; } = new PlatformNavigationTransition(PlatformNavigationTransitionKind.Platform);

		/// <summary>Stops the provider chain and navigates without animation.</summary>
		public static PlatformNavigationTransition Suppress { get; } = new PlatformNavigationTransition(PlatformNavigationTransitionKind.Suppress);

		internal bool IsPlatform => _kind == PlatformNavigationTransitionKind.Platform;

		internal bool IsSuppress => _kind == PlatformNavigationTransitionKind.Suppress;

		enum PlatformNavigationTransitionKind
		{
			Platform,
			Suppress,
			Custom,
		}
	}
}
