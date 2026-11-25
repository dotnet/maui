#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Attached properties that choose page transitions, and per-navigation overloads.
	/// </summary>
	/// <remarks>
	/// <para>Set <see cref="TransitionProperty"/> on a <see cref="Page"/> to choose how that page is shown and
	/// hidden, or on a <see cref="NavigationPage"/>, <see cref="Shell"/> or <see cref="Window"/> to set the
	/// default for the pages inside it. For a forward navigation the first of these that is set wins:</para>
	/// <list type="number">
	/// <item>an explicit "don't animate" (<c>PushAsync(page, false)</c> and friends), which runs <see cref="PageTransition.None"/>;</item>
	/// <item>the per-navigation argument to one of the overloads on this class;</item>
	/// <item>the attached property on the page being shown;</item>
	/// <item>the nearest ancestor that sets it;</item>
	/// <item><see cref="PageTransition.Platform"/>.</item>
	/// </list>
	/// <para>Going back from a page reverses the transition that actually ran when it was shown, unless a
	/// per-navigation argument is passed to the pop. When the operating system asks for reduced motion, every
	/// transition other than <see cref="PageTransition.Platform"/> runs as <see cref="PageTransition.None"/>.</para>
	/// <para>The navigation task returned by <c>PushAsync</c>, <c>PopAsync</c> and the other navigation methods
	/// completes, and <see cref="Page.NavigatedTo"/> is raised, after the transition finishes on iOS, Mac Catalyst and
	/// Android. On Windows it completes when WinUI reports the navigation done: WinUI's navigation transitions give no
	/// completion callback, so the task can complete while the transition's motion is still running there.</para>
	/// </remarks>
	public static class PageTransitions
	{
		/// <summary>Bindable property for the attached <c>Transition</c> property: the push and pop transition.</summary>
		public static readonly BindableProperty TransitionProperty =
			BindableProperty.CreateAttached("Transition", typeof(PageTransition), typeof(PageTransitions), null);

		/// <summary>Bindable property for the attached <c>ModalTransition</c> property: the modal present and dismiss transition.</summary>
		public static readonly BindableProperty ModalTransitionProperty =
			BindableProperty.CreateAttached("ModalTransition", typeof(PageTransition), typeof(PageTransitions), null);

		/// <summary>
		/// Bindable property for the attached <c>TabTransition</c> property: the transition used to switch to a
		/// Shell section or tab, or to a page of a <see cref="TabbedPage"/>. Set it on a <see cref="ShellContent"/>,
		/// <see cref="ShellSection"/>, <see cref="ShellItem"/>, <see cref="Shell"/> or <see cref="TabbedPage"/>.
		/// </summary>
		public static readonly BindableProperty TabTransitionProperty =
			BindableProperty.CreateAttached("TabTransition", typeof(PageTransition), typeof(PageTransitions), null);

		/// <summary>Gets the push and pop transition set on <paramref name="bindable"/>.</summary>
		public static PageTransition? GetTransition(BindableObject bindable)
			=> (PageTransition?)(bindable ?? throw new ArgumentNullException(nameof(bindable))).GetValue(TransitionProperty);

		/// <summary>Sets the push and pop transition on <paramref name="bindable"/>.</summary>
		public static void SetTransition(BindableObject bindable, PageTransition? value)
			=> (bindable ?? throw new ArgumentNullException(nameof(bindable))).SetValue(TransitionProperty, value);

		/// <summary>Gets the modal transition set on <paramref name="bindable"/>.</summary>
		public static PageTransition? GetModalTransition(BindableObject bindable)
			=> (PageTransition?)(bindable ?? throw new ArgumentNullException(nameof(bindable))).GetValue(ModalTransitionProperty);

		/// <summary>Sets the modal transition on <paramref name="bindable"/>.</summary>
		public static void SetModalTransition(BindableObject bindable, PageTransition? value)
			=> (bindable ?? throw new ArgumentNullException(nameof(bindable))).SetValue(ModalTransitionProperty, value);

		/// <summary>Gets the tab transition set on <paramref name="bindable"/>.</summary>
		public static PageTransition? GetTabTransition(BindableObject bindable)
			=> (PageTransition?)(bindable ?? throw new ArgumentNullException(nameof(bindable))).GetValue(TabTransitionProperty);

		/// <summary>Sets the tab transition on <paramref name="bindable"/>.</summary>
		public static void SetTabTransition(BindableObject bindable, PageTransition? value)
			=> (bindable ?? throw new ArgumentNullException(nameof(bindable))).SetValue(TabTransitionProperty, value);

		/// <summary>Pushes <paramref name="page"/> with <paramref name="transition"/>, ahead of anything the page or app sets.</summary>
		public static Task PushAsync(this INavigation navigation, Page page, PageTransition transition)
		{
			_ = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_ = page ?? throw new ArgumentNullException(nameof(page));
			_ = transition ?? throw new ArgumentNullException(nameof(transition));

			return NavigateAsync(page, NavigationTransitionKind.Push, transition, () => navigation.PushAsync(page, !transition.IsNone));
		}

		/// <summary>
		/// Pops the top page with <paramref name="transition"/> instead of reversing the one it was shown with. The result is
		/// <see langword="null"/> when there was nothing to pop.
		/// </summary>
		public static Task<Page?> PopAsync(this INavigation navigation, PageTransition transition)
		{
			_ = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_ = transition ?? throw new ArgumentNullException(nameof(transition));

			// The root page can't be popped, so nothing is staged for it.
			var stack = navigation.NavigationStack;
			var top = stack.Count > 1 ? stack[stack.Count - 1] : null;
			return NavigateAsync(top, NavigationTransitionKind.Pop, transition, () => navigation.PopAsync(!transition.IsNone));
		}

		/// <summary>Presents <paramref name="page"/> modally with <paramref name="transition"/>.</summary>
		public static Task PushModalAsync(this INavigation navigation, Page page, PageTransition transition)
		{
			_ = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_ = page ?? throw new ArgumentNullException(nameof(page));
			_ = transition ?? throw new ArgumentNullException(nameof(transition));

			return NavigateAsync(page, NavigationTransitionKind.PushModal, transition, () => navigation.PushModalAsync(page, !transition.IsNone));
		}

		/// <summary>
		/// Dismisses the top modal page with <paramref name="transition"/> instead of reversing the one it was presented with.
		/// The result is <see langword="null"/> when the dismissal was cancelled.
		/// </summary>
		public static Task<Page?> PopModalAsync(this INavigation navigation, PageTransition transition)
		{
			_ = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_ = transition ?? throw new ArgumentNullException(nameof(transition));

			return PopModalWithTransitionAsync(navigation, transition);
		}

		// The modal manager attaches the transition to the modal it actually dismisses: one queued ahead of this call can
		// be on top by the time it runs.
		static async Task<Page?> PopModalWithTransitionAsync(INavigation navigation, PageTransition transition)
		{
			PageTransitionResolver.ModalPopTransition = transition.IsNone ? null : transition.Snapshot();
			return await navigation.PopModalAsync(!transition.IsNone).ConfigureAwait(true);
		}

		// Stages the transition for this page and kind only, runs the navigation, and removes the staged value if the
		// platform never consumed it (a no-op or failed navigation, or a platform that doesn't animate this kind),
		// without touching a value a newer navigation has staged since. None is staged too: the navigation runs without
		// animation, and None is what it records, so going back from the page doesn't animate either.
		static async Task NavigateAsync(Page? page, NavigationTransitionKind kind, PageTransition transition, Func<Task> navigate)
		{
			var token = page is null ? null : PageTransitionResolver.SetPending(page, kind, transition);
			try
			{
				await navigate().ConfigureAwait(true);
			}
			finally
			{
				if (token is not null)
				{
					PageTransitionResolver.ClearPending(page!, token);
				}
			}
		}

		static async Task<Page?> NavigateAsync(Page? page, NavigationTransitionKind kind, PageTransition transition, Func<Task<Page>> navigate)
		{
			var token = page is null ? null : PageTransitionResolver.SetPending(page, kind, transition);
			try
			{
				return await navigate().ConfigureAwait(true);
			}
			finally
			{
				if (token is not null)
				{
					PageTransitionResolver.ClearPending(page!, token);
				}
			}
		}

		/// <summary>
		/// Navigates to <paramref name="state"/> with <paramref name="transition"/> for every page the navigation
		/// pushes or pops, ahead of anything the pages or app set.
		/// </summary>
		public static Task GoToAsync(this Shell shell, ShellNavigationState state, PageTransition transition)
			=> GoToAsync(shell, state, transition, null);

		/// <summary>
		/// Navigates to <paramref name="state"/> with <paramref name="transition"/> and query
		/// <paramref name="parameters"/> for every page the navigation pushes or pops. A
		/// <see cref="ShellNavigationQueryParameters"/> stays single-use, as with <see cref="Shell.GoToAsync(ShellNavigationState, bool, ShellNavigationQueryParameters)"/>.
		/// </summary>
		public static async Task GoToAsync(this Shell shell, ShellNavigationState state, PageTransition transition, IDictionary<string, object>? parameters)
		{
			_ = shell ?? throw new ArgumentNullException(nameof(shell));
			_ = transition ?? throw new ArgumentNullException(nameof(transition));

			var animate = !transition.IsNone;
			ShellRouteParameters? routeParameters = parameters switch
			{
				null => null,
				// Single-use parameters stay single-use, as with the matching Shell.GoToAsync overload.
				ShellNavigationQueryParameters singleUse => new ShellRouteParameters(singleUse),
				_ => new ShellRouteParameters(parameters),
			};

			// The transition travels with this navigation's request: a GoToAsync suspended in Navigating, or one that
			// fails, never lends its transition to another.
			await shell.NavigationManager.GoToAsync(new ShellNavigationParameters
			{
				TargetState = state,
				Animated = animate,
				EnableRelativeShellRoutes = false,
				Parameters = routeParameters,
				PageTransition = transition.Snapshot(),
			}).ConfigureAwait(true);
		}

		/// <summary>
		/// Replaces the window's root page with <paramref name="page"/>, animated with <paramref name="transition"/>.
		/// The task completes when the transition finishes.
		/// </summary>
		public static Task ReplacePageAsync(this Window window, Page page, PageTransition transition)
		{
			_ = window ?? throw new ArgumentNullException(nameof(window));
			_ = page ?? throw new ArgumentNullException(nameof(page));
			_ = transition ?? throw new ArgumentNullException(nameof(transition));

			return RootPageTransition.ReplaceAsync(window, page, transition);
		}
	}
}
