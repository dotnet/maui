using System;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : AbstractViewHandler<ICheckBox, NativeCheckBox>
	{
		protected virtual float MinimumSize => 44f;

		protected override NativeCheckBox CreateNativeView()
		{
			return new NativeCheckBox
			{
				MinimumViewSize = MinimumSize
			};
		}

		protected override void ConnectHandler(NativeCheckBox nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.CheckedChanged += OnCheckedChanged;
		}

		protected override void DisconnectHandler(NativeCheckBox nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.CheckedChanged -= OnCheckedChanged;
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
			if (sender is NativeCheckBox nativeView && VirtualView != null)
			{
				VirtualView.IsChecked = nativeView.IsChecked;
			}
		}
	}
}