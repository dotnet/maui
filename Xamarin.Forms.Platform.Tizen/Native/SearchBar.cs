using System;
using ElmSharp;
using EColor = ElmSharp.Color;
using ESize = ElmSharp.Size;
using ERect = ElmSharp.Rect;
using ERectangle = ElmSharp.Rectangle;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Provides implementation of the search bar widget.
	/// </summary>
	public class SearchBar : Canvas, IMeasurable
	{
		/// <summary>
		/// The height of the background of the search bar.
		/// </summary>
		const int BackgroundHeight = 120;

		/// <summary>
		/// The style of the cancel button.
		/// </summary>
		const string CancelButtonLayoutStyle = "editfield_clear";

		/// <summary>
		/// The horizontal padding of the cancel button.
		/// </summary>
		const int CancelButtonPaddingHorizontal = 17;

		/// <summary>
		/// The size of the cancel button.
		/// </summary>
		const int CancelButtonSize = 80;

		/// <summary>
		/// The height of the entry.
		/// </summary>
		const int EntryHeight = 54;

		/// <summary>
		/// The horizontal padding of the entry.
		/// </summary>
		const int EntryPaddingHorizontal = 42;

		/// <summary>
		/// The vertical padding of the entry.
		/// </summary>
		const int EntryPaddingVertical = 33;

		/// <summary>
		/// The height of the rectangle used to draw underline effect.
		/// </summary>
		const int RectangleHeight = 2;

		/// <summary>
		/// The bottom padding of the rectangle used to draw underline effect.
		/// </summary>
		const int RectanglePaddingBottom = 20;

		/// <summary>
		/// The horizontal padding of the rectangle used to draw underline effect.
		/// </summary>
		const int RectanglePaddingHorizontal = 32;

		/// <summary>
		/// The top padding of the rectangle used to draw underline effect.
		/// </summary>
		const int RectanglePaddingTop = 11;

		//TODO: read default platform color

		/// <summary>
		/// The color of the underline rectangle.
		/// </summary>
		static readonly EColor s_underlineColor = EColor.Aqua;

		/// <summary>
		/// The dimmed color of the underline rectangle.
		/// </summary>
		static readonly EColor s_underlineDimColor = EColor.Gray;

		/// <summary>
		/// The cancel button.
		/// </summary>
		Button _cancelButton;

		/// <summary>
		/// The text entry.
		/// </summary>
		Entry _entry;

		/// <summary>
		/// The underline rectangle.
		/// </summary>
		ERectangle _underlineRectangle;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.SearchBar"/> class.
		/// </summary>
		/// <param name="parent">Parent evas object.</param>
		public SearchBar(EvasObject parent) : base(parent)
		{
			_entry = new Entry(parent)
			{
				IsSingleLine = true,
			};
			_entry.SetInputPanelReturnKeyType(InputPanelReturnKeyType.Search);
			_entry.TextChanged += EntryTextChanged;
			_entry.Activated += EntryActivated;
			_entry.Focused += EntryFocused;
			_entry.Unfocused += EntryUnfocused;
			_entry.Show();

			_cancelButton = new Button(parent);
			_cancelButton.Style = CancelButtonLayoutStyle;
			_cancelButton.Clicked += CancelButtonClicked;

			_underlineRectangle = new ERectangle(parent)
			{
				Color = IsEnabled ? s_underlineColor : s_underlineDimColor,
			};
			_underlineRectangle.Show();

			Children.Add(_entry);
			Children.Add(_cancelButton);
			Children.Add(_underlineRectangle);

			Show();

			this.LayoutUpdated += SearchBarLayoutUpdated;
		}

		/// <summary>
		/// Occurs when the search button on the keyboard is pressed.
		/// </summary>
		public event EventHandler SearchButtonPressed;

		/// <summary>
		/// Occurs when the entry's text has changed.
		/// </summary>
		public event EventHandler<TextChangedEventArgs> TextChanged;

		/// <summary>
		/// Gets or sets the color of the cancel button.
		/// </summary>
		/// <value>Color of the cancel button.</value>
		public EColor CancelButtonColor
		{
			get
			{
				return _cancelButton.Color;
			}

			set
			{
				if (!_cancelButton.Color.Equals(value))
				{
					_cancelButton.Color = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the font attributes of the search bar's entry.
		/// </summary>
		/// <value>The font attributes.</value>
		public FontAttributes FontAttributes
		{
			get
			{
				return _entry.FontAttributes;
			}

			set
			{
				if (value != _entry.FontAttributes)
				{
					_entry.FontAttributes = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the font family of the search bar's entry.
		/// </summary>
		/// <value>The font family.</value>
		public string FontFamily
		{
			get
			{
				return _entry.FontFamily;
			}

			set
			{
				if (value != _entry.FontFamily)
				{
					_entry.FontFamily = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the size of the font of the search bar's entry.
		/// </summary>
		/// <value>The size of the font.</value>
		public double FontSize
		{
			get
			{
				return _entry.FontSize;
			}

			set
			{
				if (value != _entry.FontSize)
				{
					_entry.FontSize = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the horizontal text alignment of the search bar's entry.
		/// </summary>
		/// <value>The horizontal text alignment.</value>
		public TextAlignment HorizontalTextAlignment
		{
			get
			{
				return _entry.HorizontalTextAlignment;
			}

			set
			{
				if (value != _entry.HorizontalTextAlignment)
				{
					_entry.HorizontalTextAlignment = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the placeholder of the search bar's entry.
		/// </summary>
		/// <value>The placeholder.</value>
		public string Placeholder
		{
			get
			{
				return _entry.Placeholder;
			}

			set
			{
				if (value != _entry.Placeholder)
				{
					_entry.Placeholder = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the color of the placeholder.
		/// </summary>
		/// <value>The color of the placeholder.</value>
		public EColor PlaceholderColor
		{
			get
			{
				return _entry.PlaceholderColor;
			}

			set
			{
				if (!_entry.PlaceholderColor.Equals(value))
				{
					_entry.PlaceholderColor = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the text of the search bar's entry.
		/// </summary>
		/// <value>The text.</value>
		public override string Text
		{
			get
			{
				return _entry.Text;
			}

			set
			{
				if (value != _entry.Text)
				{
					_entry.Text = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		/// <value>The color of the text.</value>
		public EColor TextColor
		{
			get
			{
				return _entry.TextColor;
			}

			set
			{
				if (!_entry.TextColor.Equals(value))
				{
					_entry.TextColor = value;
				}
			}
		}

		/// <summary>
		/// Implementation of the IMeasurable.Measure() method.
		/// </summary>
		public ESize Measure(int availableWidth, int availableHeight)
		{
			ESize entrySize = _entry.Measure(availableWidth, availableHeight);
			int width = entrySize.Width + (CancelButtonPaddingHorizontal * 2) + CancelButtonSize;
			return new ESize(width, BackgroundHeight);
		}

		internal void BatchBegin()
		{
			_entry.BatchBegin();
		}

		internal void BatchCommit()
		{
			_entry.BatchCommit();
		}

		/// <summary>
		/// Handles the event triggered by the cancel button being clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments, ignored.</param>
		void CancelButtonClicked(object sender, EventArgs e)
		{
			_entry.Text = string.Empty;
			_cancelButton.Hide();
		}

		/// <summary>
		/// Handles the event triggered by clicking the search button on the keyboard.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments, ignored.</param>
		void EntryActivated(object sender, EventArgs e)
		{
			SearchButtonPressed?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Handles the event triggered by entry gaining the focus.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments, ignored.</param>
		void EntryFocused(object sender, EventArgs e)
		{
			_underlineRectangle.Color = s_underlineColor;
		}

		/// <summary>
		/// Handles the event triggered by entry's text being changed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		void EntryTextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.NewTextValue))
			{
				_cancelButton.Hide();
			}
			else if (!_cancelButton.IsVisible)
			{
				_cancelButton.Show();
			}
			TextChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Handles the event triggered by entry losing the focus.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments, ignored.</param>
		void EntryUnfocused(object sender, EventArgs e)
		{
			_underlineRectangle.Color = s_underlineDimColor;
		}

		/// <summary>
		/// Handles the event triggered by search bar's layout being changed.
		/// </summary>
		/// <remarks>
		/// Updates the geometry of the widgets comprising the search bar.
		/// </remarks>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		void SearchBarLayoutUpdated(object sender, LayoutEventArgs e)
		{
			_underlineRectangle.Geometry = new ERect(e.Geometry.Left + RectanglePaddingHorizontal,
				e.Geometry.Top + EntryPaddingVertical + EntryHeight + RectanglePaddingTop,
				e.Geometry.Width - (RectanglePaddingHorizontal * 2),
				RectangleHeight);

			_entry.Geometry = new ERect(e.Geometry.Left + EntryPaddingHorizontal,
				e.Geometry.Top + EntryPaddingVertical,
				e.Geometry.Width - (EntryPaddingHorizontal + (CancelButtonPaddingHorizontal * 2) + CancelButtonSize),
				EntryHeight);

			_cancelButton.Geometry = new ERect(e.Geometry.Right - CancelButtonSize - CancelButtonPaddingHorizontal,
				e.Geometry.Top + RectanglePaddingBottom,
				CancelButtonSize,
				CancelButtonSize);
		}
	}
}
