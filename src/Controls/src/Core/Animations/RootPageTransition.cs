#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Replacing a window's root page with a transition: a picture of the outgoing root is posed out while the
	/// new root is posed in, with the built-in transition's motion.
	/// </summary>
	internal static partial class RootPageTransition
	{
		// The root replacement running on each window. A new one finishes it first, so two never animate the same views.
		static readonly ConditionalWeakTable<Window, RunningTransition> s_running = new();

		/// <summary>Pictures of outgoing roots currently shown over a window, for tests.</summary>
		internal static int ActivePictures;

		internal static Task ReplaceAsync(Window window, Page page, PageTransition transition)
		{
			FinishRunning(window);

			var from = window.Page;
			if (ReferenceEquals(from, page))
			{
				return Task.CompletedTask;
			}

			object? token = transition.IsNone ? null : PageTransitionResolver.SetPending(page, NavigationTransitionKind.Push, transition);
			var animated = !transition.IsNone && window.Handler is not null && from is not null;

			// Root replacement isn't part of the provider contract: there is no platform animation to supply, so only
			// MAUI's own motion runs (and Platform, as nothing to animate, swaps without one).
			// The page lays out in the window's direction once it is the window's page (when it doesn't set its own), and
			// it isn't yet, so the direction is worked out from the window.
			var flowDirection = page.FlowDirection == FlowDirection.MatchParent
				? PageTransitionResolver.FlowDirectionOf(window)
				: page.FlowDirection;
			var request = PageTransitionResolver.ResolveRequest(NavigationTransitionKind.Push, from, page, animated, forwardFlowDirection: flowDirection);
			if (token is not null)
			{
				PageTransitionResolver.ClearPending(page, token);
			}

			var effective = request.Transition;
			if (effective.IsNone || effective.IsPlatform || effective.Duration <= TimeSpan.Zero)
			{
				window.Page = page;
				return Task.CompletedTask;
			}

			return ReplacePlatformAsync(window, page, request, effective);
		}

		/// <summary>
		/// Completes once <paramref name="page"/> has become <paramref name="window"/>'s page: at once if it already is. An
		/// assignment made from inside another assignment's change callbacks is queued until that one finishes (whose own
		/// change is raised first, for its page), so the native root to animate only exists once this has completed. A
		/// replacement that never lands is ended by whatever supersedes it (see <see cref="Track"/>), which the caller
		/// awaits alongside this.
		/// </summary>
		internal static Task WhenAssignedAsync(Window window, Page page)
		{
			if (ReferenceEquals(window.Page, page))
			{
				return Task.CompletedTask;
			}

			var assigned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			System.ComponentModel.PropertyChangedEventHandler? handler = null;
			handler = (_, e) =>
			{
				if (e.PropertyName == Window.PageProperty.PropertyName && ReferenceEquals(window.Page, page))
				{
					window.PropertyChanged -= handler;
					assigned.TrySetResult(true);
				}
			};
			window.PropertyChanged += handler;
			return assigned.Task;
		}

		/// <summary>Ends the root replacement running on <paramref name="window"/>, jumping it to its end state.</summary>
		internal static void FinishRunning(Window window)
		{
			if (s_running.TryGetValue(window, out var running))
			{
				s_running.Remove(window);
				running.Finish();
			}
		}

		/// <summary>
		/// Records the root replacement of <paramref name="page"/> now starting on <paramref name="window"/>, before anything
		/// (the page assignment included) can start another; <paramref name="finish"/> must be idempotent. It is finished,
		/// and <see cref="RunningTransition.IsSuperseded"/> then reports it, by whichever comes first: another replacement,
		/// any other page assigned to the window directly, or the window being destroyed.
		/// </summary>
		internal static RunningTransition Track(Window window, Page page, Action finish)
		{
			var running = new RunningTransition(window, page, finish);
			s_running.Put(window, running);
			running.Observe();
			return running;
		}

		/// <summary>Releases <paramref name="running"/>: stops observing the window and forgets it. Safe to call twice.</summary>
		internal static void Untrack(Window window, RunningTransition running)
		{
			running.StopObserving();
			if (s_running.TryGetValue(window, out var current) && ReferenceEquals(current, running))
			{
				s_running.Remove(window);
			}
		}

		internal sealed class RunningTransition
		{
			readonly Window _window;
			readonly Page _page;
			readonly Action _finish;
			bool _observing;

			// Whether the replacement's own assignment has landed. Until it has, a page change for another page is an older
			// assignment finishing (this one queued behind it), not a replacement of this one.
			bool _assigned;

			public RunningTransition(Window window, Page page, Action finish)
			{
				_window = window;
				_page = page;
				_finish = finish;
			}

			public bool IsSuperseded { get; private set; }

			public void Finish()
			{
				IsSuperseded = true;
				StopObserving();
				_finish();
			}

			public void Observe()
			{
				if (_observing)
				{
					return;
				}

				_observing = true;
				_assigned = ReferenceEquals(_window.Page, _page);
				_window.PropertyChanged += OnWindowPropertyChanged;
				_window.Destroying += OnWindowDestroying;
			}

			public void StopObserving()
			{
				if (!_observing)
				{
					return;
				}

				_observing = false;
				_window.PropertyChanged -= OnWindowPropertyChanged;
				_window.Destroying -= OnWindowDestroying;
			}

			// The replacement's own assignment sets the window's page to _page; a change after that replaced it directly. A
			// change before it is an earlier assignment finishing, ahead of this one in the setter queue.
			void OnWindowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (e.PropertyName != Window.PageProperty.PropertyName)
				{
					return;
				}

				if (ReferenceEquals(_window.Page, _page))
				{
					_assigned = true;
				}
				else if (_assigned)
				{
					FinishThis();
				}
			}

			void OnWindowDestroying(object? sender, EventArgs e) => FinishThis();

			void FinishThis()
			{
				if (s_running.TryGetValue(_window, out var current) && ReferenceEquals(current, this))
				{
					s_running.Remove(_window);
				}

				// Raised from the window's own page change or teardown: a failure restoring the native views is logged and
				// must not break the assignment or teardown that triggered it.
				try
				{
					Finish();
				}
				catch (Exception ex)
				{
					_window.FindMauiContext()?.CreateLogger<RunningTransition>()?.LogWarning(ex, "Finishing a root page transition failed.");
				}
			}
		}

#if !(IOS || MACCATALYST || ANDROID)
		static Task ReplacePlatformAsync(Window window, Page page, NavigationTransitionRequest request, PageTransition transition)
		{
			window.Page = page;
			return Task.CompletedTask;
		}
#endif
	}
}
