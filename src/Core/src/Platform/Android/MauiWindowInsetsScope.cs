using System;
using System.Collections.Generic;
using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;
using AInsets = AndroidX.Core.Graphics.Insets;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform;

[Flags]
internal enum SafeAreaInvalidationReason
{
	None = 0,
	InsetsChanged = 1 << 0,
	SafeAreaEdgesChanged = 1 << 1,
	ParticipantAttached = 1 << 2,
	ParticipantDetached = 1 << 3,
	ParentChanged = 1 << 4,
	BoundsChanged = 1 << 5,
	NavigationChromeChanged = 1 << 6,
	OrientationChanged = 1 << 7,
	FlowDirectionChanged = 1 << 8,
	SoftInputModeChanged = 1 << 9,
}

internal readonly record struct WindowInsetEdges(int Left, int Top, int Right, int Bottom)
{
	public static WindowInsetEdges FromInsets(AInsets? insets) =>
		new(insets?.Left ?? 0, insets?.Top ?? 0, insets?.Right ?? 0, insets?.Bottom ?? 0);
}

internal readonly record struct WindowInsetsSnapshot(
	WindowInsetEdges SystemBars,
	WindowInsetEdges DisplayCutout,
	WindowInsetEdges Ime,
	bool SystemBarsVisible,
	bool DisplayCutoutVisible,
	bool ImeVisible,
	long Generation)
{
	public static WindowInsetsSnapshot Create(WindowInsetsCompat insets, long generation)
	{
		ArgumentNullException.ThrowIfNull(insets);

		var systemBarsType = WindowInsetsCompat.Type.SystemBars();
		var displayCutoutType = WindowInsetsCompat.Type.DisplayCutout();
		var imeType = WindowInsetsCompat.Type.Ime();

		return new(
			WindowInsetEdges.FromInsets(insets.GetInsets(systemBarsType)),
			WindowInsetEdges.FromInsets(insets.GetInsets(displayCutoutType)),
			WindowInsetEdges.FromInsets(insets.GetInsets(imeType)),
			insets.IsVisible(systemBarsType),
			insets.IsVisible(displayCutoutType),
			insets.IsVisible(imeType),
			generation);
	}

	public bool HasSameValues(in WindowInsetsSnapshot other) =>
		SystemBars == other.SystemBars &&
		DisplayCutout == other.DisplayCutout &&
		Ime == other.Ime &&
		SystemBarsVisible == other.SystemBarsVisible &&
		DisplayCutoutVisible == other.DisplayCutoutVisible &&
		ImeVisible == other.ImeVisible;
}

internal interface IMauiSafeAreaParticipant
{
	AView PlatformView { get; }

	void ApplySafeArea(in SafeAreaPadding safeArea);

	void ResetSafeArea();
}

internal interface IMauiWindowInsetsRouter
{
	AView RootView { get; }

	SourceEdgeMask Claims { get; }

	void Apply(in WindowInsetsSnapshot snapshot);

	void Reset();
}

[Flags]
internal enum SourceEdgeMask
{
	None = 0,
}

internal sealed class MauiWindowInsetsScope : IDisposable
{
	readonly Dictionary<IMauiSafeAreaParticipant, ParticipantNode> _participants = [];
	readonly Dictionary<AView, ParticipantNode> _participantsByView = [];
	readonly List<IMauiWindowInsetsRouter> _routers = [];
	readonly MauiWindowInsetsScopeTag _scopeTag;
	readonly RootWindowInsetsListener _rootInsetsListener;
	readonly ScopeImeAnimationCallback _imeAnimationCallback;
	readonly HostLayoutChangeListener _hostLayoutChangeListener;
	readonly Java.Lang.Runnable _resolveRunnable;
	readonly StructuralPaddingApplicator _appBarApplicator = new();
	readonly StructuralPaddingApplicator _toolbarApplicator = new();
	readonly StructuralPaddingApplicator _contentApplicator = new();
	AView? _hostContentBranch;
	SourceEdgeMask _hostContentClaims;
	bool _isDisposed;
	bool _isResolvePosted;
	int _stableHostHeight;
	long _snapshotGeneration;
	long _evaluationRevision;

	public MauiWindowInsetsScope(AView hostView)
	{
		HostView = hostView ?? throw new ArgumentNullException(nameof(hostView));
		_scopeTag = new(this);
		_rootInsetsListener = new(this);
		_imeAnimationCallback = new(this);
		_hostLayoutChangeListener = new(this);
		_resolveRunnable = new Java.Lang.Runnable(ResolvePostedSafeAreas);
		HostView.SetTag(Resource.Id.maui_window_insets_scope, _scopeTag);
		ViewCompat.SetOnApplyWindowInsetsListener(HostView, _rootInsetsListener);
		ViewCompat.SetWindowInsetsAnimationCallback(HostView, _imeAnimationCallback);
		HostView.AddOnLayoutChangeListener(_hostLayoutChangeListener);
		_stableHostHeight = HostView.Height;
	}

	public AView HostView { get; }

	public WindowInsetsSnapshot Snapshot { get; private set; }

	public long EvaluationRevision => _evaluationRevision;

	public SafeAreaInvalidationReason PendingInvalidationReasons { get; private set; }

	public int ParticipantCount => _participants.Count;

	public static MauiWindowInsetsScope? FindForView(AView view)
	{
		ArgumentNullException.ThrowIfNull(view);

		AView? current = view;
		while (current is not null)
		{
			if (current.GetTag(Resource.Id.maui_window_insets_scope) is MauiWindowInsetsScopeTag scopeTag)
			{
				return scopeTag.Scope;
			}

			current = current.Parent as AView;
		}

		return null;
	}

	public static IDisposable RegisterAppBarRouter(AView rootView, AppBarLayout appBar) =>
		new AppBarRouterRegistration(rootView, appBar);

	public static IDisposable RegisterFlyoutRouter(AView rootView) =>
		new FlyoutRouterRegistration(rootView);

	public bool UpdateSnapshot(WindowInsetsCompat insets)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		var nextSnapshot = WindowInsetsSnapshot.Create(insets, _snapshotGeneration + 1);
		if (Snapshot.HasSameValues(nextSnapshot))
		{
			return false;
		}

		_snapshotGeneration++;
		Snapshot = nextSnapshot with { Generation = _snapshotGeneration };
		Invalidate(SafeAreaInvalidationReason.InsetsChanged);
		return true;
	}

	public void Register(IMauiSafeAreaParticipant participant)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		ArgumentNullException.ThrowIfNull(participant);

		if (!_participants.ContainsKey(participant))
		{
			var node = new ParticipantNode(participant);
			_participants.Add(participant, node);
			_participantsByView.Add(participant.PlatformView, node);
			RebuildParticipantTree();
			Invalidate(SafeAreaInvalidationReason.ParticipantAttached);
		}
	}

	public void Unregister(IMauiSafeAreaParticipant participant)
	{
		ArgumentNullException.ThrowIfNull(participant);

		if (_participants.Remove(participant, out var node))
		{
			_participantsByView.Remove(participant.PlatformView);
			node.Detach();
			participant.ResetSafeArea();
			RebuildParticipantTree();
			Invalidate(SafeAreaInvalidationReason.ParticipantDetached);
		}
	}

	public void ResolvePendingSafeAreas()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		if (PendingInvalidationReasons == SafeAreaInvalidationReason.None)
		{
			return;
		}

		for (int i = 0; i < _routers.Count; i++)
		{
			_routers[i].Apply(Snapshot);
		}

		ApplyStructuralInsets();
		var context = new SafeAreaBranchContext();
		foreach (var pair in _participants)
		{
			var node = pair.Value;
			if (node.Parent is null)
			{
				ResolveNode(node, context);
			}
		}

		PendingInvalidationReasons = SafeAreaInvalidationReason.None;
	}

	public void Invalidate(SafeAreaInvalidationReason reason)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		if (reason == SafeAreaInvalidationReason.None)
		{
			return;
		}

		PendingInvalidationReasons |= reason;
		_evaluationRevision++;
		ScheduleResolve();
	}

	public SafeAreaInvalidationReason ConsumePendingInvalidations()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		var reasons = PendingInvalidationReasons;
		PendingInvalidationReasons = SafeAreaInvalidationReason.None;
		return reasons;
	}

	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}

		_isDisposed = true;
		HostView.RemoveCallbacks(_resolveRunnable);
		_isResolvePosted = false;
		ViewCompat.SetOnApplyWindowInsetsListener(HostView, null);
		ViewCompat.SetWindowInsetsAnimationCallback(HostView, null);
		HostView.RemoveOnLayoutChangeListener(_hostLayoutChangeListener);
		_appBarApplicator.Reset();
		_toolbarApplicator.Reset();
		_contentApplicator.Reset();

		foreach (var pair in _participants)
		{
			pair.Key.ResetSafeArea();
		}

		_participants.Clear();
		_participantsByView.Clear();
		for (int i = 0; i < _routers.Count; i++)
		{
			_routers[i].Reset();
		}
		_routers.Clear();
		PendingInvalidationReasons = SafeAreaInvalidationReason.None;

		if (ReferenceEquals(HostView.GetTag(Resource.Id.maui_window_insets_scope), _scopeTag))
		{
			HostView.SetTag(Resource.Id.maui_window_insets_scope, null);
		}

		_scopeTag.Dispose();
		_rootInsetsListener.Dispose();
		_imeAnimationCallback.Dispose();
		_hostLayoutChangeListener.Dispose();
	}

	void ScheduleResolve()
	{
		if (_isResolvePosted || !HostView.IsAttachedToWindow)
		{
			return;
		}

		_isResolvePosted = HostView.Post(_resolveRunnable);
	}

	void ResolvePostedSafeAreas()
	{
		_isResolvePosted = false;
		if (!_isDisposed)
		{
			ResolvePendingSafeAreas();
		}
	}

	WindowInsetsCompat OnApplyWindowInsets(WindowInsetsCompat insets)
	{
		UpdateSnapshot(insets);
		return insets;
	}

	void OnHostLayoutChanged()
	{
		if (!Snapshot.ImeVisible && HostView.Height > 0)
		{
			_stableHostHeight = HostView.Height;
		}

		Invalidate(SafeAreaInvalidationReason.NavigationChromeChanged | SafeAreaInvalidationReason.BoundsChanged);
	}

	void ApplyStructuralInsets()
	{
		var appBar = HostView.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
		var content = HostView.FindViewById<AView>(Resource.Id.navigationlayout_content);
		var bottomTabs = HostView.FindViewById<AView>(Resource.Id.navigationlayout_bottomtabs);
		var appBarHasContent = HasMeasuredContent(appBar);
		var bottomTabsVisible = bottomTabs is not null && bottomTabs.Visibility == ViewStates.Visible && bottomTabs.MeasuredHeight > 0;
		var containerTop = Math.Max(Snapshot.SystemBars.Top, Snapshot.DisplayCutout.Top);
		var containerBottom = Math.Max(Snapshot.SystemBars.Bottom, Snapshot.DisplayCutout.Bottom);

		_appBarApplicator.Apply(
			appBar,
			appBarHasContent ? Snapshot.SystemBars.Left : 0,
			appBarHasContent ? containerTop : 0,
			appBarHasContent ? Snapshot.SystemBars.Right : 0,
			0);

		var toolbar = FindDescendant<MaterialToolbar>(appBar);
		_toolbarApplicator.Apply(
			toolbar,
			appBarHasContent ? Snapshot.DisplayCutout.Left : 0,
			0,
			appBarHasContent ? Snapshot.DisplayCutout.Right : 0,
			0);

		_contentApplicator.Apply(content, 0, 0, 0, bottomTabsVisible ? containerBottom : 0);

		_hostContentBranch = content;
		_hostContentClaims = SourceEdgeMask.None;
		if (appBarHasContent)
		{
			_hostContentClaims |= GetSourceEdgeMask(InsetSource.SystemBars, 1);
			_hostContentClaims |= GetSourceEdgeMask(InsetSource.DisplayCutout, 1);
		}

		if (bottomTabsVisible)
		{
			_hostContentClaims |= GetSourceEdgeMask(InsetSource.SystemBars, 3);
			_hostContentClaims |= GetSourceEdgeMask(InsetSource.DisplayCutout, 3);
		}
	}

	static bool HasMeasuredContent(AppBarLayout? appBar)
	{
		if (appBar is null || appBar.Visibility != ViewStates.Visible)
		{
			return false;
		}

		if (appBar.MeasuredHeight > 0)
		{
			return true;
		}

		for (int i = 0; i < appBar.ChildCount; i++)
		{
			if (appBar.GetChildAt(i)?.MeasuredHeight > 0)
			{
				return true;
			}
		}

		return false;
	}

	static T? FindDescendant<T>(AView? root) where T : AView
	{
		if (root is T match)
		{
			return match;
		}

		if (root is not ViewGroup group)
		{
			return null;
		}

		for (int i = 0; i < group.ChildCount; i++)
		{
			if (FindDescendant<T>(group.GetChildAt(i)) is T descendant)
			{
				return descendant;
			}
		}

		return null;
	}

	static bool IsDescendantOrSelf(AView view, AView ancestor)
	{
		AView? current = view;
		while (current is not null)
		{
			if (ReferenceEquals(current, ancestor))
			{
				return true;
			}

			current = current.Parent as AView;
		}

		return false;
	}

	void RebuildParticipantTree()
	{
		foreach (var pair in _participants)
		{
			pair.Value.Detach();
		}

		foreach (var pair in _participants)
		{
			var node = pair.Value;
			var parentView = node.Participant.PlatformView.Parent as AView;
			while (parentView is not null && parentView != HostView)
			{
				if (_participantsByView.TryGetValue(parentView, out var parentNode))
				{
					node.AttachTo(parentNode);
					break;
				}

				parentView = parentView.Parent as AView;
			}
		}
	}

	void RegisterRouter(IMauiWindowInsetsRouter router)
	{
		if (!_routers.Contains(router))
		{
			_routers.Add(router);
			Invalidate(SafeAreaInvalidationReason.NavigationChromeChanged);
		}
	}

	void UnregisterRouter(IMauiWindowInsetsRouter router)
	{
		if (_routers.Remove(router))
		{
			router.Reset();
			if (!_isDisposed)
			{
				Invalidate(SafeAreaInvalidationReason.NavigationChromeChanged);
			}
		}
	}

	void ResolveNode(ParticipantNode node, SafeAreaBranchContext inheritedContext)
	{
		var view = node.Participant.PlatformView;
		if (!view.IsAttachedToWindow || view.Visibility != ViewStates.Visible)
		{
			node.ResetContribution();
			return;
		}

		var safeAreaView = view is ICrossPlatformLayoutBacking backing
			? SafeAreaExtensions.GetSafeAreaView2(backing.CrossPlatformLayout)
			: null;

		if (safeAreaView is null ||
			!ShouldApplySafeArea(view, safeAreaView.HasExplicitSafeAreaEdges) ||
			!TryGetSlotInScope(view, safeAreaView as IView, out var slot))
		{
			node.ResetContribution();
			ResolveChildren(node, inheritedContext);
			return;
		}

		var context = inheritedContext;
		if (_hostContentBranch is not null && IsDescendantOrSelf(view, _hostContentBranch))
		{
			context.Claims |= _hostContentClaims;
		}

		for (int i = 0; i < _routers.Count; i++)
		{
			var router = _routers[i];
			if (IsDescendantOrSelf(view, router.RootView))
			{
				context.Claims |= router.Claims;
			}
		}

		var isExplicit = safeAreaView.HasExplicitSafeAreaEdges;
		var contribution = ResolveContribution(safeAreaView, isExplicit, slot, ref context);
		node.ApplyContribution(contribution);

		ResolveChildren(node, context);
	}

	void ResolveChildren(ParticipantNode node, SafeAreaBranchContext context)
	{
		for (int i = 0; i < node.Children.Count; i++)
		{
			ResolveNode(node.Children[i], context);
		}
	}

	static bool ShouldApplySafeArea(AView view, bool hasExplicitSafeAreaEdges)
	{
		var parent = view.Parent;
		var isInsideRecyclerEmptyView = false;

		while (parent is not null)
		{
			if (parent is IMauiRecyclerViewEmptyView)
			{
				isInsideRecyclerEmptyView = true;
			}

			if (parent is AppBarLayout ||
				parent is MauiScrollView ||
				(parent is IMauiRecyclerView && !isInsideRecyclerEmptyView && !hasExplicitSafeAreaEdges))
			{
				return false;
			}

			parent = parent.Parent;
		}

		return true;
	}

	SafeAreaPadding ResolveContribution(
		ISafeAreaView2 safeAreaView,
		bool isExplicit,
		in ParticipantSlot slot,
		ref SafeAreaBranchContext context)
	{
		double left = 0;
		double right = 0;
		double top = 0;
		double bottom = 0;

		for (int edge = 0; edge < 4; edge++)
		{
			var region = safeAreaView.GetSafeAreaRegionsForEdge(edge);

			if (isExplicit && region == SafeAreaRegions.None)
			{
				continue;
			}

			double resolved = 0;
			if (region == SafeAreaRegions.Default ||
				SafeAreaEdges.IsContainer(region) ||
				(edge != 3 && SafeAreaEdges.IsSoftInput(region)))
			{
				var systemBars = ResolveSourceOverlap(InsetSource.SystemBars, edge, slot, context.Claims);
				var displayCutout = ResolveSourceOverlap(InsetSource.DisplayCutout, edge, slot, context.Claims);
				resolved = Math.Max(systemBars, displayCutout);

				if (systemBars > 0)
				{
					context.Claims |= GetSourceEdgeMask(InsetSource.SystemBars, edge);
				}

				if (displayCutout > 0)
				{
					context.Claims |= GetSourceEdgeMask(InsetSource.DisplayCutout, edge);
				}
			}

			if (edge == 3 && SafeAreaEdges.IsSoftInput(region) && Snapshot.ImeVisible)
			{
				var ime = ResolveSourceOverlap(InsetSource.Ime, edge, slot, context.Claims);
				resolved = Math.Max(resolved, ime);
				if (ime > 0)
				{
					context.Claims |= GetSourceEdgeMask(InsetSource.Ime, edge);
				}
			}

			switch (edge)
			{
				case 0:
					left = resolved;
					break;
				case 1:
					top = resolved;
					break;
				case 2:
					right = resolved;
					break;
				case 3:
					bottom = resolved;
					break;
			}
		}

		return new SafeAreaPadding(left, right, top, bottom);
	}

	double ResolveSourceOverlap(
		InsetSource source,
		int edge,
		in ParticipantSlot slot,
		SourceEdgeMask claims)
	{
		if ((claims & GetSourceEdgeMask(source, edge)) != 0)
		{
			return 0;
		}

		var sourceInsets = source switch
		{
			InsetSource.SystemBars => Snapshot.SystemBars,
			InsetSource.DisplayCutout => Snapshot.DisplayCutout,
			InsetSource.Ime => GetEffectiveImeInsets(),
			_ => default,
		};

		return edge switch
		{
			0 => Math.Clamp(sourceInsets.Left - slot.Left, 0, sourceInsets.Left),
			1 => Math.Clamp(sourceInsets.Top - slot.Top, 0, sourceInsets.Top),
			2 => Math.Clamp(slot.Right - (HostView.Width - sourceInsets.Right), 0, sourceInsets.Right),
			3 => Math.Clamp(slot.Bottom - (HostView.Height - sourceInsets.Bottom), 0, sourceInsets.Bottom),
			_ => 0,
		};
	}

	int ResolveBottomOverlap(AView view, int bottomInset)
	{
		if (bottomInset <= 0 || view.Height <= 0 || HostView.Height <= 0)
		{
			return bottomInset;
		}

		var viewLocation = new int[2];
		var hostLocation = new int[2];
		view.GetLocationInWindow(viewLocation);
		HostView.GetLocationInWindow(hostLocation);

		var viewBottom = viewLocation[1] - hostLocation[1] + view.Height;
		return Math.Clamp(viewBottom - (HostView.Height - bottomInset), 0, bottomInset);
	}

	WindowInsetEdges GetEffectiveImeInsets()
	{
		if (!Snapshot.ImeVisible || HostView.Context?.GetActivity()?.Window?.Attributes is not WindowManagerLayoutParams attributes)
		{
			return Snapshot.Ime;
		}

		var adjustMode = attributes.SoftInputMode & SoftInput.MaskAdjust;
		if (adjustMode == SoftInput.AdjustPan)
		{
			return default;
		}

		if (adjustMode != SoftInput.AdjustResize)
		{
			return Snapshot.Ime;
		}

		var platformResize = Math.Max(0, _stableHostHeight - HostView.Height);
		return Snapshot.Ime with
		{
			Bottom = Math.Max(0, Snapshot.Ime.Bottom - platformResize),
		};
	}

	bool TryGetSlotInScope(AView view, IView? virtualView, out ParticipantSlot slot)
	{
		if (view.Width <= 0 || view.Height <= 0 || HostView.Width <= 0 || HostView.Height <= 0)
		{
			slot = default;
			return false;
		}

		var left = view.Left;
		var top = view.Top;
		var parent = view.Parent as AView;
		while (parent is not null && parent != HostView)
		{
			left += parent.Left;
			top += parent.Top;
			parent = parent.Parent as AView;
		}

		if (parent != HostView && view != HostView)
		{
			slot = default;
			return false;
		}

		var right = left + view.Width;
		var bottom = top + view.Height;

		if (virtualView is not null)
		{
			var margin = virtualView.Margin;
			var context = view.Context;
			left = Math.Max(0, left - (int)context.ToPixels(margin.Left));
			top = Math.Max(0, top - (int)context.ToPixels(margin.Top));
			right += (int)context.ToPixels(margin.Right);
			bottom += (int)context.ToPixels(margin.Bottom);
		}

		slot = new ParticipantSlot(left, top, right, bottom);
		return true;
	}

	static SourceEdgeMask GetSourceEdgeMask(InsetSource source, int edge) =>
		(SourceEdgeMask)(1 << (((int)source * 4) + edge));

	sealed class MauiWindowInsetsScopeTag : Java.Lang.Object
	{
		public MauiWindowInsetsScopeTag(MauiWindowInsetsScope scope)
		{
			Scope = scope;
		}

		public MauiWindowInsetsScope Scope { get; }
	}

	sealed class RootWindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
	{
		readonly MauiWindowInsetsScope _scope;

		public RootWindowInsetsListener(MauiWindowInsetsScope scope)
		{
			_scope = scope;
		}

		public WindowInsetsCompat? OnApplyWindowInsets(AView? view, WindowInsetsCompat? insets) =>
			insets is null ? null : _scope.OnApplyWindowInsets(insets);
	}

	sealed class ScopeImeAnimationCallback : WindowInsetsAnimationCompat.Callback
	{
		readonly MauiWindowInsetsScope _scope;

		public ScopeImeAnimationCallback(MauiWindowInsetsScope scope) : base(DispatchModeContinueOnSubtree)
		{
			_scope = scope;
		}

		public override WindowInsetsCompat? OnProgress(
			WindowInsetsCompat? insets,
			IList<WindowInsetsAnimationCompat>? runningAnimations)
		{
			if (insets is not null && HasImeAnimation(runningAnimations))
			{
				_scope.UpdateSnapshot(insets);
			}

			return insets;
		}

		public override void OnEnd(WindowInsetsAnimationCompat? animation)
		{
			base.OnEnd(animation);
			if (animation is not null && (animation.TypeMask & WindowInsetsCompat.Type.Ime()) != 0)
			{
				ViewCompat.RequestApplyInsets(_scope.HostView);
			}
		}

		static bool HasImeAnimation(IList<WindowInsetsAnimationCompat>? animations)
		{
			if (animations is null)
			{
				return false;
			}

			for (int i = 0; i < animations.Count; i++)
			{
				if ((animations[i].TypeMask & WindowInsetsCompat.Type.Ime()) != 0)
				{
					return true;
				}
			}

			return false;
		}
	}

	sealed class HostLayoutChangeListener : Java.Lang.Object, AView.IOnLayoutChangeListener
	{
		readonly MauiWindowInsetsScope _scope;

		public HostLayoutChangeListener(MauiWindowInsetsScope scope)
		{
			_scope = scope;
		}

		public void OnLayoutChange(
			AView? view,
			int left,
			int top,
			int right,
			int bottom,
			int oldLeft,
			int oldTop,
			int oldRight,
			int oldBottom)
		{
			_scope.OnHostLayoutChanged();
		}
	}

	sealed class StructuralPaddingApplicator
	{
		AView? _view;
		(int Left, int Top, int Right, int Bottom) _originalPadding;

		public void Apply(AView? view, int left, int top, int right, int bottom)
		{
			if (!ReferenceEquals(_view, view))
			{
				Reset();
				_view = view;
				if (view is not null)
				{
					_originalPadding = (view.PaddingLeft, view.PaddingTop, view.PaddingRight, view.PaddingBottom);
				}
			}

			view?.SetPadding(
				_originalPadding.Left + left,
				_originalPadding.Top + top,
				_originalPadding.Right + right,
				_originalPadding.Bottom + bottom);
		}

		public void Reset()
		{
			_view?.SetPadding(
				_originalPadding.Left,
				_originalPadding.Top,
				_originalPadding.Right,
				_originalPadding.Bottom);
			_view = null;
			_originalPadding = default;
		}
	}

	abstract class StructuralRouterRegistration : Java.Lang.Object, AView.IOnAttachStateChangeListener, IMauiWindowInsetsRouter
	{
		MauiWindowInsetsScope? _scope;
		bool _isDisposed;

		protected StructuralRouterRegistration(AView rootView)
		{
			RootView = rootView;
			rootView.AddOnAttachStateChangeListener(this);
			if (rootView.IsAttachedToWindow)
			{
				AttachToScope();
			}
		}

		public AView RootView { get; }

		public SourceEdgeMask Claims { get; protected set; }

		protected MauiWindowInsetsScope? Scope => _scope;

		public abstract void Apply(in WindowInsetsSnapshot snapshot);

		public abstract void Reset();

		public void OnViewAttachedToWindow(AView? attachedView) => AttachToScope();

		public void OnViewDetachedFromWindow(AView? detachedView) => DetachFromScope();

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				_isDisposed = true;
				RootView.RemoveOnAttachStateChangeListener(this);
				DetachFromScope();
			}

			base.Dispose(disposing);
		}

		void AttachToScope()
		{
			if (_isDisposed)
			{
				return;
			}

			var scope = FindForView(RootView);
			if (ReferenceEquals(scope, _scope))
			{
				return;
			}

			DetachFromScope();
			_scope = scope;
			_scope?.RegisterRouter(this);
		}

		void DetachFromScope()
		{
			var scope = _scope;
			_scope = null;
			scope?.UnregisterRouter(this);
		}
	}

	sealed class AppBarRouterRegistration : StructuralRouterRegistration
	{
		readonly AppBarLayout _appBar;
		readonly StructuralPaddingApplicator _appBarApplicator = new();
		readonly StructuralPaddingApplicator _toolbarApplicator = new();

		public AppBarRouterRegistration(AView rootView, AppBarLayout appBar) : base(rootView)
		{
			_appBar = appBar;
		}

		public override void Apply(in WindowInsetsSnapshot snapshot)
		{
			var hasContent = HasMeasuredContent(_appBar);
			_appBarApplicator.Apply(
				_appBar,
				hasContent ? snapshot.SystemBars.Left : 0,
				hasContent ? Math.Max(snapshot.SystemBars.Top, snapshot.DisplayCutout.Top) : 0,
				hasContent ? snapshot.SystemBars.Right : 0,
				0);

			var toolbar = FindDescendant<MaterialToolbar>(_appBar);
			_toolbarApplicator.Apply(
				toolbar,
				hasContent ? snapshot.DisplayCutout.Left : 0,
				0,
				hasContent ? snapshot.DisplayCutout.Right : 0,
				0);

			Claims = hasContent
				? GetSourceEdgeMask(InsetSource.SystemBars, 1) | GetSourceEdgeMask(InsetSource.DisplayCutout, 1)
				: SourceEdgeMask.None;
		}

		public override void Reset()
		{
			Claims = SourceEdgeMask.None;
			_appBarApplicator.Reset();
			_toolbarApplicator.Reset();
		}
	}

	sealed class FlyoutRouterRegistration : StructuralRouterRegistration
	{
		readonly StructuralPaddingApplicator _applicator = new();

		public FlyoutRouterRegistration(AView rootView) : base(rootView)
		{
		}

		public override void Apply(in WindowInsetsSnapshot snapshot)
		{
			var bottom = Math.Max(snapshot.SystemBars.Bottom, snapshot.DisplayCutout.Bottom);
			if (Scope is not null)
			{
				bottom = Scope.ResolveBottomOverlap(RootView, bottom);
			}

			_applicator.Apply(
				RootView,
				Math.Max(snapshot.SystemBars.Left, snapshot.DisplayCutout.Left),
				Math.Max(snapshot.SystemBars.Top, snapshot.DisplayCutout.Top),
				Math.Max(snapshot.SystemBars.Right, snapshot.DisplayCutout.Right),
				bottom);

			Claims = SourceEdgeMask.None;
			for (int edge = 0; edge < 4; edge++)
			{
				Claims |= GetSourceEdgeMask(InsetSource.SystemBars, edge);
				Claims |= GetSourceEdgeMask(InsetSource.DisplayCutout, edge);
			}
		}

		public override void Reset()
		{
			Claims = SourceEdgeMask.None;
			_applicator.Reset();
		}
	}

	sealed class ParticipantNode
	{
		SafeAreaPadding _contribution;

		public ParticipantNode(IMauiSafeAreaParticipant participant)
		{
			Participant = participant;
		}

		public IMauiSafeAreaParticipant Participant { get; }

		public ParticipantNode? Parent { get; private set; }

		public List<ParticipantNode> Children { get; } = [];

		public void AttachTo(ParticipantNode parent)
		{
			Parent = parent;
			parent.Children.Add(this);
		}

		public void Detach()
		{
			if (Parent is { } parent)
			{
				parent.Children.Remove(this);
				Parent = null;
			}

			Children.Clear();
		}

		public void ApplyContribution(in SafeAreaPadding contribution)
		{
			if (_contribution == contribution)
			{
				return;
			}

			_contribution = contribution;
			Participant.ApplySafeArea(contribution);
		}

		public void ResetContribution()
		{
			if (_contribution.IsEmpty)
			{
				return;
			}

			_contribution = SafeAreaPadding.Empty;
			Participant.ResetSafeArea();
		}
	}

	readonly record struct ParticipantSlot(double Left, double Top, double Right, double Bottom);

	struct SafeAreaBranchContext
	{
		public SourceEdgeMask Claims;
	}

	enum InsetSource
	{
		SystemBars,
		DisplayCutout,
		Ime,
	}
}
