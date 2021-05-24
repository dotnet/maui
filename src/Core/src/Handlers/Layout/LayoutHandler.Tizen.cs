using System;
using ElmSharp;
using Tizen.UIExtensions.Common;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : EViewHandler<ILayout, LayoutCanvas>
	{
		bool _layoutUpdatedRegistered = false;

		public void RegisterOnLayoutUpdated()
		{
			if (!_layoutUpdatedRegistered)
			{
				if (NativeView!= null)
				{
					NativeView.LayoutUpdated += OnLayoutUpdated;		
				}
				_layoutUpdatedRegistered = true;
			}
		}

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

			var view = new LayoutCanvas(NativeParent)
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange
			};
			view.Show();
			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.Measure;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;
			NativeView.Clear();

			foreach (var child in VirtualView.Children)
			{
				NativeView.Children.Add(child.ToNative(MauiContext, false));
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

			NativeView.Children.Add(child.ToNative(MauiContext, false));
		}

		public void Remove(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child.Handler is INativeViewHandler thandler && thandler.NativeView is EvasObject nativeView)
			{
				NativeView.Children.Remove(nativeView);
				thandler.Dispose();
			}
		}

		protected void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			if (VirtualView != null && NativeView != null)
			{
				var nativeGeometry = NativeView.Geometry.ToDP();
				if (nativeGeometry.Width > 0 && nativeGeometry.Height > 0 )
				{
					VirtualView.InvalidateMeasure();
					VirtualView.InvalidateArrange();
					VirtualView.Measure(nativeGeometry.Width, nativeGeometry.Height);
					VirtualView.Arrange(nativeGeometry);
				}
			}
		}
	}
}
