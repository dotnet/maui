using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.IO;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchView : SearchView
	{
		internal EditText? _queryEditor;
		bool _showCancelButton = true;

		public MauiSearchView(Context context) : base(context)
		{
			Initialize();
		}

		void Initialize()
		{
			SetIconifiedByDefault(false);
			MaxWidth = int.MaxValue;

			_queryEditor = this.GetFirstChildOfType<EditText>();

			// Disable Android's built-in instance state saving on the internal EditText
			// to prevent query text from being incorrectly restored across multiple
			// SearchView instances during navigation. The EditText shares a fixed
			// resource ID (search_src_text) across all SearchViews, causing state
			// to bleed between instances.
			if (_queryEditor is not null)
			{
				_queryEditor.SaveEnabled = false;
				_queryEditor.EnsureNoFullscreenFlag();
			}

			if (_queryEditor?.LayoutParameters is LinearLayout.LayoutParams layoutParams)
			{
				layoutParams.Height = LinearLayout.LayoutParams.MatchParent;
				layoutParams.Gravity = GravityFlags.FillVertical;
			}

			var searchCloseButtonIdentifier = Resource.Id.search_close_btn;
			if (searchCloseButtonIdentifier > 0)
			{
				var image = FindViewById<ImageView>(searchCloseButtonIdentifier);

				image?.SetMinimumWidth((int?)Context?.ToPixels(44) ?? 0);
			}
		}

		internal void UpdateCancelButtonVisibility(bool showCancelButton)
		{
			_showCancelButton = showCancelButton;

			if (this.GetCancelButton() is not ImageView cancelButton)
				return;

			if (!showCancelButton)
				cancelButton.Visibility = ViewStates.Gone;
			else
				cancelButton.Visibility = string.IsNullOrEmpty(Query) ? ViewStates.Gone : ViewStates.Visible;
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);

			if (!_showCancelButton && this.GetCancelButton() is ImageView cancelButton &&
				cancelButton.Visibility != ViewStates.Gone)
			{
				cancelButton.Visibility = ViewStates.Gone;
			}
		}
	}
}
