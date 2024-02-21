#nullable disable
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal class ShellFlyoutHeaderContainer : UIContainerView
	{
		Thickness _safearea = Thickness.Zero;

		public ShellFlyoutHeaderContainer(View view) : base(view)
		{
			UpdateSafeAreaMargin();
		}

		public override Thickness Margin
		{
			get
			{
				if (View.IsSet(View.MarginProperty))
					return View.Margin;

				if (View is ISafeAreaView sav && sav.IgnoreSafeArea)
					return Thickness.Zero;

				var safeArea = UIApplication.SharedApplication.GetSafeAreaInsetsForWindow();

				// As the safe area is for the entire window, we should not account for the bottom area for the header, 
				// otherwise it will be treated as margin added in between the header and content
				return new Thickness(
					safeArea.Left,
					safeArea.Top,
					safeArea.Right,
					0);
			}
		}

		public override void LayoutSubviews()
		{
			if (!UpdateSafeAreaMargin())
				base.LayoutSubviews();
		}

		public override void SafeAreaInsetsDidChange()
		{
			UpdateSafeAreaMargin();
			base.SafeAreaInsetsDidChange();
		}

		bool UpdateSafeAreaMargin()
		{
			var safeArea = UIApplication.SharedApplication.GetSafeAreaInsetsForWindow();

			if (safeArea.Top != _safearea.Top ||
				safeArea.Bottom != _safearea.Bottom ||
				safeArea.Right != _safearea.Right ||
				safeArea.Left != _safearea.Left)
			{
				_safearea =
					new Thickness(
						safeArea.Left,
						safeArea.Top,
						safeArea.Right,
						safeArea.Bottom);

				OnHeaderSizeChanged();
				return true;
			}

			return false;

		}
	}
}
