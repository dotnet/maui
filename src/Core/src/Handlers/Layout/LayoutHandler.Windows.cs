using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutView>
	{
		public void Add(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.Children.Add(child.ToNative(MauiContext));
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.Measure;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;

			foreach (var child in VirtualView.Children)
			{
				Add(child);
			}
		}


		public void Remove(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.Handler?.NativeView is UIElement view)
			{
				NativeView.Children.Remove(view);
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
				CrossPlatformArrangeChildren = () =>
				{
					foreach (var element in VirtualView.Children)
					{
						element.Handler?.SetFrame(element.Frame);
					}
				},
				CrossPlatformInvalidateChildrenMeasure = () =>
				{
					foreach (var element in VirtualView.Children)
					{
						element.InvalidateMeasure();
					}
				}
			};

			return view;
		}
	}
}
