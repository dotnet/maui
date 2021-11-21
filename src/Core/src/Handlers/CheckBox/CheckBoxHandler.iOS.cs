using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, MauiCheckBox>
	{
		protected virtual float MinimumSize => 44f;

		protected override MauiCheckBox CreateNativeView()
		{
			return new MauiCheckBox
			{
				MinimumViewSize = MinimumSize
			};
		}

		protected override void ConnectHandler(MauiCheckBox nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.CheckedChanged += OnCheckedChanged;
		}

		protected override void DisconnectHandler(MauiCheckBox nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.CheckedChanged -= OnCheckedChanged;
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}

		public static void MapForeground(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateForeground(check);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = base.GetDesiredSize(widthConstraint, heightConstraint);

			var set = false;

			var width = widthConstraint;
			var height = heightConstraint;

			if (size.Width == 0)
			{
				if (widthConstraint <= 0 || double.IsInfinity(widthConstraint))
				{
					width = MinimumSize;
					set = true;
				}
			}

			if (size.Height == 0)
			{
				if (heightConstraint <= 0 || double.IsInfinity(heightConstraint))
				{
					height = MinimumSize;
					set = true;
				}
			}

			if (set)
			{
				size = new Size(width, height);
			}

			return size;
		}

		void OnCheckedChanged(object? sender, EventArgs e)
		{
			if (sender is MauiCheckBox nativeView && VirtualView != null)
			{
				VirtualView.IsChecked = nativeView.IsChecked;
			}
		}
	}
}