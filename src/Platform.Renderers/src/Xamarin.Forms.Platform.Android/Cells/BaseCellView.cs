using System;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class BaseCellView : LinearLayout, INativeElementView
	{
		public const double DefaultMinHeight = 44;

		readonly Color _androidDefaultTextColor;
		Cell _cell;
		readonly TextView _detailText;
		readonly ImageView _imageView;
		readonly TextView _mainText;
		Color _defaultDetailColor;
		Color _defaultMainTextColor;
		Color _detailTextColor;
		string _detailTextText;
		ImageSource _imageSource;
		Color _mainTextColor;
		string _mainTextText;

		public BaseCellView(Context context, Cell cell) : base(context)
		{
			_cell = cell;
			SetMinimumWidth((int)context.ToPixels(25));
			SetMinimumHeight((int)context.ToPixels(25));
			Orientation = Orientation.Horizontal;

			var padding = (int)context.FromPixels(8);
			SetPadding(padding, padding, padding, padding);

			_imageView = new ImageView(context);
			var imageParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
			{
				Width = (int)context.ToPixels(60),
				Height = (int)context.ToPixels(60),
				RightMargin = 0,
				Gravity = GravityFlags.Center
			};
			using (imageParams)
				AddView(_imageView, imageParams);

			var textLayout = new LinearLayout(context) { Orientation = Orientation.Vertical };

			_mainText = new TextView(context);
			_mainText.SetSingleLine(true);
			_mainText.Ellipsize = TextUtils.TruncateAt.End;
			_mainText.SetPadding((int)context.ToPixels(15), padding, padding, padding);
			TextViewCompat.SetTextAppearance(_mainText, global::Android.Resource.Style.TextAppearanceSmall);

			using (var lp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent))
				textLayout.AddView(_mainText, lp);

			_detailText = new TextView(context);
			_detailText.SetSingleLine(true);
			_detailText.Ellipsize = TextUtils.TruncateAt.End;
			_detailText.SetPadding((int)context.ToPixels(15), padding, padding, padding);
			_detailText.Visibility = ViewStates.Gone;
			TextViewCompat.SetTextAppearance(_detailText, global::Android.Resource.Style.TextAppearanceSmall);

			using (var lp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent))
				textLayout.AddView(_detailText, lp);

			var layoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent) { Width = 0, Weight = 1, Gravity = GravityFlags.Center };

			using (layoutParams)
				AddView(textLayout, layoutParams);

			SetMinimumHeight((int)context.ToPixels(DefaultMinHeight));
			_androidDefaultTextColor = Color.FromUint((uint)_mainText.CurrentTextColor);

			if ((int)Forms.SdkInt > 16)
			{
				_mainText.TextAlignment = global::Android.Views.TextAlignment.ViewStart;
				_detailText.TextAlignment = global::Android.Views.TextAlignment.ViewStart;
			}
		}

		public AView AccessoryView { get; private set; }

		public string DetailText
		{
			get { return _detailTextText; }
			set
			{
				if (_detailTextText == value)
					return;

				_detailTextText = value;
				_detailText.Text = value;
				_detailText.Visibility = string.IsNullOrEmpty(value) ? ViewStates.Gone : ViewStates.Visible;
			}
		}

		public string MainText
		{
			get { return _mainTextText; }
			set
			{
				if (_mainTextText == value)
					return;

				_mainTextText = value;
				_mainText.Text = value;
			}
		}

		Element INativeElementView.Element
		{
			get { return _cell; }
		}

		public void SetAccessoryView(AView view)
		{
			if (AccessoryView != null)
				RemoveView(AccessoryView);

			if (view != null)
			{
				using (var layout = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent))
					AddView(view, layout);

				AccessoryView = view;
			}
		}

		public void SetDefaultMainTextColor(Color defaultColor)
		{
			_defaultMainTextColor = defaultColor;
			if (_mainTextColor == Color.Default)
				_mainText.SetTextColor(defaultColor.ToAndroid());
		}

		public void SetDetailTextColor(Color color)
		{
			if (_detailTextColor == color)
				return;

			if (_defaultDetailColor == Color.Default)
				_defaultDetailColor = Color.FromUint((uint)_detailText.CurrentTextColor);

			_detailTextColor = color;
			_detailText.SetTextColor(color.ToAndroid(_defaultDetailColor));
		}

		public void SetImageSource(ImageSource source)
		{
			UpdateBitmap(source, _imageSource);
			_imageSource = source;
		}

		public void SetImageVisible(bool visible)
		{
			_imageView.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
		}

		public void SetIsEnabled(bool isEnable)
		{
			_mainText.Enabled = isEnable;
			_detailText.Enabled = isEnable;
		}

		public void SetMainTextColor(Color color)
		{
			Color defaultColorToSet = _defaultMainTextColor == Color.Default ? _androidDefaultTextColor : _defaultMainTextColor;

			_mainTextColor = color;
			_mainText.SetTextColor(color.ToAndroid(defaultColorToSet));
		}

		public void SetRenderHeight(double height)
		{
			height = Context.ToPixels(height);
			LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(height == -1 ? ViewGroup.LayoutParams.WrapContent : height));
		}

		async void UpdateBitmap(ImageSource source, ImageSource previousSource = null)
		{
			try
			{
				await _imageView.UpdateBitmap(source, previousSource).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Warning(nameof(BaseCellView), "Error loading image: {0}", ex);
			}
		}
	}
}