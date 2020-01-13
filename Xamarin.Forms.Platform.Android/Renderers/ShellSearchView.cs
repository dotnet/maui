using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using ASupportDrawable = AndroidX.AppCompat.Graphics.Drawable;
using AndroidX.CardView.Widget;
#else
using Android.Support.V7.Widget;
using ASupportDrawable = Android.Support.V7.Graphics.Drawable;
#endif
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android.FastRenderers;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;
using AImageButton = Android.Widget.ImageButton;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellSearchView : FrameLayout, IShellSearchView, TextView.IOnEditorActionListener, ITextWatcher
	{
		#region IShellSearchView

		public event EventHandler SearchConfirmed;

		public SearchHandler SearchHandler { get; set; }

		public bool ShowKeyboardOnAttached { get; set; }

		AView IShellSearchView.View
		{
			get
			{
				if (_searchButton == null)
					throw new InvalidOperationException("LoadView must be called before accessing View");
				return this;
			}
		}

		void IShellSearchView.LoadView()
		{
			LoadView(SearchHandler);
			if (_searchHandlerAppearanceTracker == null)
				_searchHandlerAppearanceTracker = CreateSearchHandlerAppearanceTracker();
		}

		protected virtual SearchHandlerAppearanceTracker CreateSearchHandlerAppearanceTracker()
		{
			return new SearchHandlerAppearanceTracker(this);
		}

		#endregion IShellSearchView

		#region ITextWatcher

		void ITextWatcher.AfterTextChanged(IEditable s)
		{
			var text = _textBlock.Text;

			if (text == ShellSearchViewAdapter.DoNotUpdateMarker)
			{
				return;
			}

			UpdateClearButtonState();

			SearchHandler.SetValueCore(SearchHandler.QueryProperty, text);

			if (SearchHandler.ShowsResults)
			{
				if (string.IsNullOrEmpty(text))
				{
					_textBlock.DismissDropDown();
				}
				else
				{
					_textBlock.ShowDropDown();
				}
			}
		}

		void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
		{
		}

		void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
		{
		}

		#endregion ITextWatcher

		IShellContext _shellContext;
		CardView _cardView;
		AImageButton _clearButton;
		AImageButton _clearPlaceholderButton;
		AImageButton _searchButton;
		AppCompatAutoCompleteTextView _textBlock;
		bool _disposed;
		SearchHandlerAppearanceTracker _searchHandlerAppearanceTracker;

		public ShellSearchView(Context context, IShellContext shellContext) : base(context)
		{
			_shellContext = shellContext;
		}

		ISearchHandlerController Controller => SearchHandler;

		bool TextView.IOnEditorActionListener.OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
		{
			// Fire Completed and dismiss keyboard for hardware / physical keyboards
			if (actionId == ImeAction.Done || (actionId == ImeAction.ImeNull && e.KeyCode == Keycode.Enter && e.Action == KeyEventActions.Up))
			{
				_textBlock.ClearFocus();
				v.HideKeyboard();
				SearchConfirmed?.Invoke(this, EventArgs.Empty);
				Controller.QueryConfirmed();
				_textBlock.Text = "";
			}

			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_disposed = true;

				SearchHandler.PropertyChanged -= OnSearchHandlerPropertyChanged;

				_textBlock.ItemClick -= OnTextBlockItemClicked;
				_textBlock.RemoveTextChangedListener(this);
				_textBlock.SetOnEditorActionListener(null);
				_textBlock.DropDownBackground.Dispose();
				_textBlock.SetDropDownBackgroundDrawable(null);

				_clearButton.Click -= OnClearButtonClicked;
				_clearPlaceholderButton.Click -= OnClearPlaceholderButtonClicked;
				_searchButton.Click -= OnSearchButtonClicked;

				_textBlock.Adapter.Dispose();
				_textBlock.Adapter = null;
				_searchHandlerAppearanceTracker?.Dispose();
				_textBlock.Dispose();
				_clearButton.Dispose();
				_searchButton.Dispose();
				_cardView.Dispose();
				_clearPlaceholderButton.Dispose();
			}

			_textBlock = null;
			_clearButton = null;
			_searchButton = null;
			_cardView = null;
			_clearPlaceholderButton = null;
			_shellContext = null;
			_searchHandlerAppearanceTracker = null;

			SearchHandler = null;

			base.Dispose(disposing);
		}

		protected virtual void LoadView(SearchHandler searchHandler)
		{
			var query = searchHandler.Query;
			var placeholder = searchHandler.Placeholder;

			LP lp;
			var context = Context;
			_cardView = new CardView(context);
			using (lp = new LayoutParams(LP.MatchParent, LP.MatchParent))
				_cardView.LayoutParameters = lp;

			var linearLayout = new LinearLayout(context);
			using (lp = new LP(LP.MatchParent, LP.MatchParent))
				linearLayout.LayoutParameters = lp;
			linearLayout.Orientation = Orientation.Horizontal;

			_cardView.AddView(linearLayout);

			int padding = (int)context.ToPixels(8);

			_searchButton = CreateImageButton(context, searchHandler, SearchHandler.QueryIconProperty, Resource.Drawable.abc_ic_search_api_material, padding, 0, "SearchIcon");

			lp = new LinearLayout.LayoutParams(0, LP.MatchParent)
			{
				Gravity = GravityFlags.Fill,
				Weight = 1
			};
			_textBlock = new AppCompatAutoCompleteTextView(context)
			{
				LayoutParameters = lp,
				Text = query,
				Hint = placeholder,
				ImeOptions = ImeAction.Done
			};
			lp.Dispose();
			_textBlock.Enabled = searchHandler.IsSearchEnabled;
			_textBlock.SetBackground(null);
			_textBlock.SetPadding(padding, 0, padding, 0);
			_textBlock.SetSingleLine(true);
			_textBlock.Threshold = 1;
			_textBlock.Adapter = new ShellSearchViewAdapter(SearchHandler, _shellContext);
			_textBlock.ItemClick += OnTextBlockItemClicked;
			_textBlock.SetDropDownBackgroundDrawable(new ClipDrawableWrapper(_textBlock.DropDownBackground));

			// A note on accessibility. The _textBlocks hint is what android defaults to reading in the screen
			// reader. Therefore, we do not need to set something else.

			_clearButton = CreateImageButton(context, searchHandler, SearchHandler.ClearIconProperty, Resource.Drawable.abc_ic_clear_material, 0, padding, nameof(SearchHandler.ClearIcon));
			_clearPlaceholderButton = CreateImageButton(context, searchHandler, SearchHandler.ClearPlaceholderIconProperty, -1, 0, padding, nameof(SearchHandler.ClearPlaceholderIcon));

			linearLayout.AddView(_searchButton);
			linearLayout.AddView(_textBlock);
			linearLayout.AddView(_clearButton);
			linearLayout.AddView(_clearPlaceholderButton);
		
			UpdateClearButtonState();

			// hook all events down here to avoid getting events while doing setup
			searchHandler.PropertyChanged += OnSearchHandlerPropertyChanged;
			_textBlock.AddTextChangedListener(this);
			_textBlock.SetOnEditorActionListener(this);
			_clearButton.Click += OnClearButtonClicked;
			_clearPlaceholderButton.Click += OnClearPlaceholderButtonClicked;
			_searchButton.Click += OnSearchButtonClicked;
			
			AddView(_cardView);

			linearLayout.Dispose();
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.IsSearchEnabledProperty.PropertyName)
			{
				_textBlock.Enabled = SearchHandler.IsSearchEnabled;
			}
		}

		protected override async void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (!ShowKeyboardOnAttached)
				return;

			Alpha = 0;
			Animate().Alpha(1).SetDuration(200).SetListener(null);

			// need to wait so keyboard will show
			await Task.Delay(200);

			if (_disposed)
				return;

			_textBlock.RequestFocus();
			Context.ShowKeyboard(_textBlock);
		}

		protected virtual void OnClearButtonClicked(object sender, EventArgs e)
		{
			_textBlock.Text = "";
		}

		protected virtual void OnClearPlaceholderButtonClicked(object sender, EventArgs e)
		{
			Controller.ClearPlaceholderClicked();
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			var width = right - left;
			width -= (int)Context.ToPixels(25);
			var height = bottom - top;
			for (int i = 0; i < ChildCount; i++)
			{
				var child = GetChildAt(i);
				child.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
							  MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
				child.Layout(0, 0, width, height);
			}

			_textBlock.DropDownHorizontalOffset = -_textBlock.Left;
			_textBlock.DropDownVerticalOffset = -(int)System.Math.Ceiling(_cardView.Radius);
			_textBlock.DropDownWidth = width;
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			var measureWidth = MeasureSpecFactory.GetSize(widthMeasureSpec);
			var measureHeight = MeasureSpecFactory.GetSize(heightMeasureSpec);

			SetMeasuredDimension(measureWidth, (int)Context.ToPixels(35));
		}

		protected virtual void OnSearchButtonClicked(object sender, EventArgs e)
		{
		}

		AImageButton CreateImageButton(Context context, BindableObject bindable, BindableProperty property, int defaultImage, int leftMargin, int rightMargin, string tag)
		{
			var result = new AImageButton(context);
			result.Tag = tag;
			result.SetPadding(0, 0, 0, 0);
			result.Focusable = false;
			result.SetScaleType(ImageView.ScaleType.FitCenter);

			string defaultHint = null;
			string defaultDescription = null;
			if (bindable.GetValue(property) is ImageSource image)
				AutomationPropertiesProvider.SetContentDescription(result, image, ref defaultDescription, ref defaultHint);

			_shellContext.ApplyDrawableAsync(bindable, property, drawable =>
			{
				if (drawable != null)
					result.SetImageDrawable(drawable);
				else if (defaultImage > 0)
					result.SetImageResource(defaultImage);
				else
					result.SetImageDrawable(null);
			});
			var lp = new LinearLayout.LayoutParams((int)Context.ToPixels(22), LP.MatchParent)
			{
				LeftMargin = leftMargin,
				RightMargin = rightMargin
			};
			result.LayoutParameters = lp;
			lp.Dispose();
			result.SetBackground(null);

			return result;
		}

		void OnTextBlockItemClicked(object sender, AdapterView.ItemClickEventArgs e)
		{
			var index = e.Position;
			var item = Controller.ListProxy[index];

			_textBlock.Text = "";
			_textBlock.HideKeyboard();
			SearchConfirmed?.Invoke(this, EventArgs.Empty);
			Controller.ItemSelected(item);
		}

		void UpdateClearButtonState()
		{
			if (string.IsNullOrEmpty(_textBlock.Text))
			{
				_clearButton.Visibility = ViewStates.Gone;
				if (SearchHandler.ClearPlaceholderIcon != null && SearchHandler.ClearPlaceholderEnabled)
					_clearPlaceholderButton.Visibility = ViewStates.Visible;
				else
					_clearPlaceholderButton.Visibility = ViewStates.Gone;
			}
			else
			{
				_clearPlaceholderButton.Visibility = ViewStates.Gone;
				_clearButton.Visibility = ViewStates.Visible;
			}
		}

		class ClipDrawableWrapper : ASupportDrawable.DrawableWrapper
		{
			public ClipDrawableWrapper(Drawable dr) : base(dr)
			{
			}

			public override void Draw(Canvas canvas)
			{
				base.Draw(canvas);

				// Step 1: Clip out the top shadow that was drawn as it wont look right when lined up
				var paint = new Paint
				{
					Color = AColor.Black
				};
				paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));

				canvas.DrawRect(0, -100, canvas.Width, 0, paint);

				// Step 2: Draw separator line

				paint = new Paint
				{
					Color = AColor.LightGray
				};
				canvas.DrawLine(0, 0, canvas.Width, 0, paint);
			}
		}
	}
}