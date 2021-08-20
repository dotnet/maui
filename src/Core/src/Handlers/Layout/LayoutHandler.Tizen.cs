using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutCanvas>
	{
		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Clip != null ||
			base.NeedsContainer;

		protected override LayoutCanvas CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Canvas");
			}

			if (NativeParent == null)
			{
				throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");
			}

			var view = new LayoutCanvas(NativeParent, VirtualView)
			{
				CrossPlatformMeasure = VirtualView.LayoutManager.Measure,
				CrossPlatformArrange = VirtualView.LayoutManager.ArrangeChildren
			};

			view.Show();
			return view;
		}

		public override Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return VirtualView.LayoutManager.Measure(widthConstraint, heightConstraint);
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.LayoutManager.Measure;
			NativeView.CrossPlatformArrange = VirtualView.LayoutManager.ArrangeChildren;

			NativeView.Children.Clear();

			foreach (var child in VirtualView)
			{
				NativeView.Children.Add(child.ToNative(MauiContext));
				if (child.Handler is INativeViewHandler thandler)
				{
					thandler?.SetParent(this);
				}
			}
		}

		public void Add(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.Children.Add(child.ToNative(MauiContext));
			if (child.Handler is INativeViewHandler childHandler)
			{
				childHandler?.SetParent(this);
			}
		}

		public void Remove(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child.Handler is INativeViewHandler thandler && thandler.NativeView is EvasObject childView)
			{
				NativeView.Children.Remove(childView);
				thandler.Dispose();
			}
		}

		public void Clear()
		{
			if (NativeView == null)
				return;

			foreach (var child in NativeView.Children)
			{
				child.Unrealize();
			}
			NativeView.Children.Clear();
		}

		public void Insert(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.Children.Insert(index, child.ToNative(MauiContext));
			if (child.Handler is INativeViewHandler childHandler)
			{
				childHandler?.SetParent(this);
			}
		}

		public void Update(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var toBeRemoved = NativeView.Children[index];
			NativeView.Children.RemoveAt(index);
			toBeRemoved.Unrealize();

			NativeView.Children.Insert(index, child.ToNative(MauiContext));
			if (child.Handler is INativeViewHandler childHandler)
			{
				childHandler?.SetParent(this);
			}
		}
	}
}
