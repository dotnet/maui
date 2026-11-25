#nullable enable
namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// The seam through which Core's navigation managers ask the Controls layer for a navigation's
	/// transition (the page-transition properties and the provider chain live in Controls).
	/// </summary>
	/// <remarks>
	/// Consulted from the base implementations of the navigation virtuals, so an override of
	/// <c>StackNavigationManager.OnCreateNavigationAnimation</c> (Android) or
	/// <c>StackNavigationManager.GetNavigationTransition</c> (Windows) keeps working and takes precedence.
	/// </remarks>
	internal static class NavigationTransitionHooks
	{
		internal static INavigationTransitionHooks? Current { get; set; }
	}

	internal interface INavigationTransitionHooks
	{
#if IOS || MACCATALYST
		/// <summary>
		/// The animator for a push or pop between <paramref name="from"/> and <paramref name="to"/>, or
		/// <see langword="null"/> for UIKit's own. <paramref name="interaction"/> is set when the animator
		/// brings its own interaction controller for an interactive pop.
		/// </summary>
		UIKit.IUIViewControllerAnimatedTransitioning? GetAnimationController(
			IView? navigationView,
			UIKit.UINavigationControllerOperation operation,
			IView? from,
			IView? to,
			bool isInteractive,
			out UIKit.IUIViewControllerInteractiveTransitioning? interaction);

		/// <summary>
		/// Called for a push or pop made without animation, for which UIKit never asks for an animator, so the
		/// navigation is still resolved and recorded (going back reverses what the page was shown with).
		/// </summary>
		void OnUnanimatedNavigation(IView? navigationView, UIKit.UINavigationControllerOperation operation, IView? from, IView? to);
#elif ANDROID
		/// <summary>
		/// Called once per stack navigation, before the fragment transaction, to resolve its transition.
		/// Returns a token handed back to <see cref="TryCreateAnimation"/>, or <see langword="null"/> for the platform default.
		/// </summary>
		object? PrepareNavigation(IView? navigationView, IView? from, IView? to, bool isPopping, bool animated);

		/// <summary>
		/// The fragment animation for one side of a prepared navigation. Returns <see langword="false"/> to
		/// use the platform default; <see langword="true"/> with a <see langword="null"/> animation suppresses it.
		/// </summary>
		bool TryCreateAnimation(object prepared, global::Android.Content.Context context, bool enter, out global::Android.Views.Animations.Animation? animation);
#elif WINDOWS
		/// <summary>
		/// The WinUI transition for a stack navigation. Returns <see langword="false"/> to use the platform default.
		/// </summary>
		bool TryGetNavigationTransition(IView? navigationView, IView? from, IView? to, bool isPopping, bool animated, out Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo? transition);
#endif
	}
}
