using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// Per-target-framework proof that the platform-neutral contracts are usable from an external
	/// assembly on every target framework .NET MAUI ships, and that the backend's own native type
	/// survives the round trip.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This file compiles unchanged on the platform-neutral target framework and on every platform
	/// target framework. That is the point: the aliased handler interfaces cannot be implemented on any
	/// of them by an external backend, while these contracts can be implemented on all of them.
	/// </para>
	/// <para>
	/// Every conversion below is an implicit reference conversion resolved at compile time. If the
	/// contracts regressed, this file would stop compiling — on the specific target framework that
	/// regressed.
	/// </para>
	/// </remarks>
	public static class ExternalBackendTfmContracts
	{
		/// <summary>
		/// The backend's own native type — which on a platform target framework derives from that
		/// platform's base view type, and on the neutral one derives from nothing — flows out of the
		/// contract strongly typed.
		/// </summary>
		public static FakeNativeLabel TypedPlatformViewSurvivesRoundTrip(FakeLabelHandler handler)
		{
			IViewHandler<ILabel, FakeNativeLabel> typed = handler;

			return typed.PlatformView;
		}

		/// <summary>
		/// The same handler is matchable without naming the backend's type at all.
		/// </summary>
		public static object NeutralPlatformViewSurvivesRoundTrip(FakeLabelHandler handler)
		{
			IViewHandler<ILabel, object> neutral = handler;

			return neutral.PlatformView;
		}

		/// <summary>
		/// The layout contract carries behavior, so verify the members are callable through it.
		/// </summary>
		public static int LayoutBehaviorIsReachableThroughContract(FakeLayoutHandler handler, IView child)
		{
			ILayoutHandler<FakeNativeLayoutView> typed = handler;
			ILayoutHandler<object> neutral = typed;

			neutral.Add(child);
			neutral.Insert(0, child);
			neutral.Update(0, child);
			neutral.UpdateZIndex(child);
			neutral.Remove(child);
			neutral.Clear();

			return typed.PlatformView.FakeChildren.Count;
		}

		/// <summary>
		/// A window is an element rather than a view, and its native type is unconstrained on every
		/// target framework.
		/// </summary>
		public static FakeNativeWindow WindowPlatformViewSurvivesRoundTrip(FakeWindowHandler handler)
		{
			IElementHandler<IWindow, FakeNativeWindow> typed = handler;
			IElementHandler<IWindow, object> neutral = typed;

			_ = neutral.PlatformView;

			return typed.PlatformView;
		}

		/// <summary>
		/// .NET MAUI's own in-box handlers satisfy the same neutral contracts on this target framework,
		/// so a consumer can write one code path for in-box and external backends alike.
		/// </summary>
		public static bool InBoxHandlersSatisfyNeutralContracts() =>
			new LabelHandler() is IViewHandler<ILabel, object> &&
			new ContentViewHandler() is IViewHandler<IContentView, object> &&
			new WindowHandler() is IElementHandler<IWindow, object> &&
			new LayoutHandler() is ILayoutHandler<object>;
	}
}
