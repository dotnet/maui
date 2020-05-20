using System.Collections.Specialized;
using System.Linq;
using System.Maui.Core.Controls;
using Android.App;
using Android.Text;
using Android.Text.Style;

namespace System.Maui.Platform
{
	public partial class PickerRenderer : AbstractViewRenderer<IPicker, MauiPicker>
	{
		AlertDialog _dialog;
		TextColorSwitcher _textColorSwitcher;
		TextColorSwitcher _hintColorSwitcher;

		protected override MauiPicker CreateView()
		{
			var mauiPicker = new MauiPicker(Context);

			mauiPicker.Click += OnClick;
			((INotifyCollectionChanged)VirtualView.Items).CollectionChanged += OnCollectionChanged;

			return mauiPicker;
		}

		protected override void DisposeView(MauiPicker mauiPickerText)
		{
			mauiPickerText.Click -= OnClick;
			((INotifyCollectionChanged)VirtualView.Items).CollectionChanged -= OnCollectionChanged;

			base.DisposeView(mauiPickerText);
		}

		public static void MapPropertyTitle(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdatePicker();
		}

		public static void MapPropertyTitleColor(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdateTitleColor();
		}

		public static void MapPropertyTextColor(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdateTextColor();
		}

		public static void MapPropertySelectedIndex(IViewRenderer renderer, IPicker picker)
		{
			(renderer as PickerRenderer)?.UpdatePicker();
		}

		void UpdatePicker()
		{
			if (TypedNativeView == null || VirtualView == null)
				return;

			UpdateTitleColor();

			if (VirtualView.SelectedIndex == -1 || VirtualView.Items == null || VirtualView.SelectedIndex >= VirtualView.Items.Count)
				TypedNativeView.Text = null;
			else
				TypedNativeView.Text = VirtualView.Items[VirtualView.SelectedIndex];
		}

		void UpdateTitleColor()
		{
			if (TypedNativeView == null || VirtualView == null)
				return;

			_hintColorSwitcher = _hintColorSwitcher ?? new TextColorSwitcher(TypedNativeView);
			_hintColorSwitcher.UpdateTextColor(VirtualView.TitleColor);
		}

		void UpdateTextColor()
		{
			if (TypedNativeView == null || VirtualView == null)
				return;

			_textColorSwitcher = _textColorSwitcher ?? new TextColorSwitcher(TypedNativeView);
			_textColorSwitcher.UpdateTextColor(VirtualView.TextColor);
		}

		void OnClick(object sender, EventArgs e)
		{
			if (VirtualView == null)
				return;

			if (_dialog == null)
			{
				using (var builder = new AlertDialog.Builder(Context))
				{
					if (VirtualView.TitleColor == Color.Default)
					{
						builder.SetTitle(VirtualView.Title ?? string.Empty);
					}
					else
					{
						var title = new SpannableString(VirtualView.Title ?? string.Empty);
						title.SetSpan(new ForegroundColorSpan(VirtualView.TitleColor.ToNative()), 0, title.Length(), SpanTypes.ExclusiveExclusive);
						builder.SetTitle(title);
					}

					string[] items = VirtualView.Items.ToArray();
					builder.SetItems(items, (s, e) =>
					{
						var selectedIndex = e.Which;
						VirtualView.SelectedIndex = selectedIndex;
						UpdatePicker();
					});

					builder.SetNegativeButton(Android.Resource.String.Cancel, (o, args) => { });

					_dialog = builder.Create();
				}

				_dialog.SetCanceledOnTouchOutside(true);

				_dialog.DismissEvent += (sender, args) =>
				{
					_dialog.Dispose();
					_dialog = null;
				};

				_dialog.Show();
			}
		}

		void OnCollectionChanged(object sender, EventArgs e)
		{
			UpdatePicker();
		}
	}
}