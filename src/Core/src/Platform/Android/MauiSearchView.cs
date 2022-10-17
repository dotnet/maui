using Android.Content;
using Android.Views;
using Android.Widget;
using Java.IO;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchView : SearchView
	{
		internal EditText? _queryEditor;

		public MauiSearchView(Context context) : base(context)
		{
			Initialize();
		}

		void Initialize()
		{
			SetIconifiedByDefault(false);

			_queryEditor = this.GetFirstChildOfType<EditText>();

			if (_queryEditor?.LayoutParameters is LinearLayout.LayoutParams layoutParams)
			{
				layoutParams.Height = LinearLayout.LayoutParams.MatchParent;
				layoutParams.Gravity = GravityFlags.FillVertical;
			}

			var searchCloseButtonIdentifier = Resource.Id.search_close_btn;
			if (searchCloseButtonIdentifier > 0)
			{
				var image = FindViewById<ImageView>(searchCloseButtonIdentifier);

				if (image != null)
					image.SetMinimumWidth((int?)Context?.ToPixels(44) ?? 0);
			}
		}
	}
}
