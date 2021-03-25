using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : AbstractViewHandler<ILayout, LayoutView>
	{
		public void Add(IView child)
		{
			_ = TypedNativeView ?? throw new InvalidOperationException($"{nameof(TypedNativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			TypedNativeView.Children.Add(child.ToNative(MauiContext));
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = TypedNativeView ?? throw new InvalidOperationException($"{nameof(TypedNativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			TypedNativeView.CrossPlatformMeasure = VirtualView.Measure;
			TypedNativeView.CrossPlatformArrange = VirtualView.Arrange;

			foreach (var child in VirtualView.Children)
			{
				Add(child);
			}
		}


		public void Remove(IView child)
		{
			_ = TypedNativeView ?? throw new InvalidOperationException($"{nameof(TypedNativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.Handler?.NativeView is UIElement view)
			{
				TypedNativeView.Children.Remove(view);
			}
		}

		protected override LayoutView CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");
			}

			var view = new LayoutView
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange,
				RetrieveChildren = () => VirtualView.Children
			};

			return view;
		}
	}
}
