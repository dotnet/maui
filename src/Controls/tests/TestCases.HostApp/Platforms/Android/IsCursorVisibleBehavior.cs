using AndroidX.AppCompat.Widget;

namespace Maui.Controls.Sample
{
	public partial class IsCursorVisibleBehavior : PlatformBehavior<Entry, AppCompatEditText>
	{
		protected override void OnAttachedTo(Entry bindable, AppCompatEditText platformView)
		{
			base.OnAttachedTo(bindable, platformView);

			if (bindable is null)
				return;

			if (IsCursorVisible == false)
				HideCursor(platformView);
			else
				ShowCursor(platformView);
		}

		protected override void OnDetachedFrom(Entry bindable, AppCompatEditText platformView)
		{
			base.OnDetachedFrom(bindable, platformView);

			if (bindable is null)
				return;

			HideCursor(platformView);
		}

		void ShowCursor(AppCompatEditText editText)
		{
			editText.SetCursorVisible(true);
		}

		void HideCursor(AppCompatEditText editText)
		{
			editText.SetCursorVisible(false);
		}
	}
}
