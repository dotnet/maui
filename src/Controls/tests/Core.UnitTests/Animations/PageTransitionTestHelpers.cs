using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Animations
{
	/// <summary>
	/// A NavigationPage handler that resolves each navigation's transition the way the platform navigation
	/// managers do (once, before the platform navigation) and lets a test hold the navigation open, as a
	/// running transition would.
	/// </summary>
	internal class TransitionRecordingNavigationHandler : ViewHandler<NavigationPage, object>
	{
		public static CommandMapper<IStackNavigationView, TransitionRecordingNavigationHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IStackNavigation.RequestNavigation)] = (handler, view, args) => handler.OnRequestNavigation((NavigationRequest)args),
		};

		IReadOnlyList<IView> _stack = Array.Empty<IView>();
		TaskCompletionSource<bool> _hold;
		readonly TaskCompletionSource<bool> _holdStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TransitionRecordingNavigationHandler() : base(new PropertyMapper<IStackNavigationView, TransitionRecordingNavigationHandler>(), CommandMapper)
		{
		}

		internal List<ResolvedNavigationTransition> Resolved { get; } = new();

		internal ResolvedNavigationTransition Last => Resolved[Resolved.Count - 1];

		/// <summary>When set, navigations stay open until <see cref="FinishTransition"/> is called.</summary>
		public bool HoldTransitions { get; set; }

		public bool IsHolding => _hold is not null;

		public void FinishTransition() => _hold?.TrySetResult(true);

		/// <summary>Completes once a navigation is being held open.</summary>
		public Task WaitForHoldAsync() => _holdStarted.Task;

		protected override object CreatePlatformView() => new object();

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			// The stack the page already has; the initial navigation request can arrive after the first push.
			_stack = ((NavigationPage)view).Navigation.NavigationStack.Cast<IView>().ToList();
		}

		async void OnRequestNavigation(NavigationRequest request)
		{
			var previous = _stack;
			var next = request.NavigationStack.ToList();

			// The stack is updated before anything is awaited, so a queued navigation sees this one's result.
			_stack = next;

			if (previous.Count > 0 && next.Count > 0 && !ReferenceEquals(previous[previous.Count - 1], next[next.Count - 1]))
			{
				var isPop = next.Count < previous.Count;
				var kind = isPop ? NavigationTransitionKind.Pop : NavigationTransitionKind.Push;
				Resolved.Add(PageTransitionResolver.Resolve(kind, previous[previous.Count - 1] as Page, next[next.Count - 1] as Page, request.Animated));

				if (HoldTransitions)
				{
					_hold = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
					_holdStarted.TrySetResult(true);
					await _hold.Task;
					_hold = null;
				}
			}

			await Task.Yield();
			((IStackNavigation)VirtualView).NavigationFinished(next);
		}
	}

	public sealed class RecordingProvider : INavigationTransitionProvider
	{
		readonly Func<NavigationTransitionRequest, PlatformNavigationTransition> _result;

		public RecordingProvider(Func<NavigationTransitionRequest, PlatformNavigationTransition> result) => _result = result;

		public List<NavigationTransitionRequest> Requests { get; } = new();

		public PlatformNavigationTransition GetTransition(NavigationTransitionRequest request)
		{
			Requests.Add(request);
			return _result(request);
		}
	}

	internal static class PageTransitionTestHelpers
	{
		public static (NavigationPage Navigation, TransitionRecordingNavigationHandler Handler) CreateNavigationPage(Page root, params INavigationTransitionProvider[] providers)
		{
			var services = new ServiceCollection();
			foreach (var provider in providers)
			{
				services.AddSingleton<INavigationTransitionProvider>(provider);
			}

			var handler = new TransitionRecordingNavigationHandler();
			handler.SetMauiContext(new MauiContext(services.BuildServiceProvider()));

			var navigation = new NavigationPage(true, root);
			_ = new TestWindow(navigation);
			navigation.Handler = handler;

			// Connecting the handler sends the initial navigation; let it finish so it can't interleave with the test's.
			navigation.CurrentNavigationTask?.Wait(TimeSpan.FromSeconds(5));
			return (navigation, handler);
		}
	}
}
