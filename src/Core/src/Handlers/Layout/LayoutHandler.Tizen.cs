using System;
using System.Collections.Generic;
using System.Linq;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutViewGroup>
	{
		List<IView> _children = new List<IView>();

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Clip != null ||
			base.NeedsContainer;

		protected override LayoutViewGroup CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Canvas");
			}

			var view = new LayoutViewGroup(VirtualView)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};

			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;

			PlatformView.Children.Clear();
			_children.Clear();

			foreach (var child in VirtualView.OrderByZIndex())
			{
				PlatformView.Children.Add(child.ToPlatform(MauiContext));
				_children.Add(child);
			}
		}

		public void Add(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			PlatformView.Children.Insert(targetIndex, child.ToPlatform(MauiContext));
			_children.Insert(targetIndex, child);
			EnsureZIndexOrder(child);
			PlatformView.SetNeedMeasureUpdate();
		}

		public void Remove(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child.Handler is IPlatformViewHandler thandler && child?.ToPlatform() is NView childView)
			{
				PlatformView.Children.Remove(childView);
				_children.Remove(child);
				thandler.Dispose();
			}
			PlatformView.MarkChanged();
			PlatformView.SetNeedMeasureUpdate();
		}

		public void Clear()
		{
			if (PlatformView == null)
				return;

			var children = PlatformView.Children.ToList();
			PlatformView.Children.Clear();
			foreach (var child in children)
			{
				child.Dispose();
			}

			foreach (var child in _children)
			{
				(child.Handler as IPlatformViewHandler)?.Dispose();
			}
			_children.Clear();

			PlatformView.SetNeedMeasureUpdate();
		}

		public void Insert(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			PlatformView.Children.Insert(targetIndex, child.ToPlatform(MauiContext));
			_children.Insert(targetIndex, child);
			EnsureZIndexOrder(child);
			PlatformView.SetNeedMeasureUpdate();
		}

		public void Update(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var toBeRemoved = PlatformView.Children[index];
			PlatformView.Children.RemoveAt(index);
			toBeRemoved.Dispose();
			var childToBeRemoved = _children[index];
			_children.RemoveAt(index);
			(childToBeRemoved as IPlatformViewHandler)?.Dispose();

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			PlatformView.Children.Insert(targetIndex, child.ToPlatform(MauiContext));
			_children.Insert(targetIndex, child);
			EnsureZIndexOrder(child);
			PlatformView.SetNeedMeasureUpdate();
		}

		public void UpdateZIndex(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			EnsureZIndexOrder(child);
		}

		void EnsureZIndexOrder(IView child)
		{
			if (PlatformView.Children.Count == 0)
			{
				return;
			}

			var platformChildView = child.ToPlatform(MauiContext!);
			var currentIndex = PlatformView.Children.IndexOf(platformChildView);

			if (currentIndex == -1)
			{
				return;
			}

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			if (targetIndex > currentIndex)
			{
				child.ToPlatform(MauiContext!).RaiseToTop();
				for (int i = targetIndex + 1; i < PlatformView.Children.Count; i++)
				{
					PlatformView.Children[i].RaiseToTop();
				}
			}
			else
			{
				child.ToPlatform(MauiContext!).LowerToBottom();
				for (int i = targetIndex - 1; i >= 0; i--)
				{
					PlatformView.Children[i].LowerToBottom();
				}
			}
		}

		public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(layout);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var child in VirtualView)
				{
					(child.Handler as IPlatformViewHandler)?.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
