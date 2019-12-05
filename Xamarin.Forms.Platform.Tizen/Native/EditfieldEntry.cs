using System;
using ElmSharp;
using ELayout = ElmSharp.Layout;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class EditfieldEntry : Native.Entry
	{
		Button _clearButton;
		ELayout _editfieldLayout;
		bool _enableClearButton;
		int _heightPadding = 0;

		public EditfieldEntry(EvasObject parent) : base(parent)
		{
		}

		public EditfieldEntry(EvasObject parent, string style) : base(parent)
		{
			if (!string.IsNullOrEmpty(style))
				_editfieldLayout.SetTheme("layout", "editfield", style);
		}

		public event EventHandler TextBlockFocused;
		public event EventHandler TextBlockUnfocused;

		public event EventHandler LayoutFocused;
		public event EventHandler LayoutUnfocused;

		public bool IsTextBlockFocused { get; private set; }

		public override EColor BackgroundColor
		{
			get
			{
				return _editfieldLayout.BackgroundColor;
			}
			set
			{
				_editfieldLayout.BackgroundColor = value;
			}
		}

		public bool EnableClearButton
		{
			get => _enableClearButton;
			set
			{
				_enableClearButton = value;
				UpdateEnableClearButton();
			}
		}

		public EColor ClearButtonColor
		{
			get => _clearButton?.GetPartColor("icon") ?? EColor.Default;
			set
			{
				if (_clearButton != null)
				{
					_clearButton.SetPartColor("icon", value);
					_clearButton.SetPartColor("icon_pressed", value);
				}
			}
		}

		public void SetFocusOnTextBlock(bool isFocused)
		{
			SetFocus(isFocused);
			IsTextBlockFocused = isFocused;

			if (isFocused)
				TextBlockFocused?.Invoke(this, EventArgs.Empty);
			else
				TextBlockUnfocused?.Invoke(this, EventArgs.Empty);
		}

		public override ElmSharp.Size Measure(int availableWidth, int availableHeight)
		{
			var textBlockSize = base.Measure(availableWidth, availableHeight);

			// Calculate the minimum size by adding the width of a TextBlock and an Editfield.
			textBlockSize.Width += _editfieldLayout.MinimumWidth;

			// If the height of a TextBlock is shorter than Editfield, use the minimun height of the Editfield.
			// Or add the height of the EditField to the TextBlock
			if (textBlockSize.Height < _editfieldLayout.MinimumHeight)
				textBlockSize.Height = _editfieldLayout.MinimumHeight;
			else
				textBlockSize.Height += _heightPadding;

			return textBlockSize;
		}

		protected override IntPtr CreateHandle(EvasObject parent)
		{
			var handle = base.CreateHandle(parent);
			_editfieldLayout = CreateEditFieldLayout(parent);

			// If true, It means, there is no extra layout on the widget handle
			// We need to set RealHandle, becuase we replace Handle to Layout
			if (RealHandle == IntPtr.Zero)
			{
				RealHandle = handle;
			}
			Handle = handle;

			if (!_editfieldLayout.SetPartContent("elm.swallow.content", this))
			{
				// Restore theme to default if editfield style is not available
				_editfieldLayout.SetTheme("layout", "application", "default");
				_editfieldLayout.SetPartContent("elm.swallow.content", this);
			}

			_heightPadding = _editfieldLayout.EdjeObject["elm.swallow.content"].Geometry.Height;
			return _editfieldLayout;
		}

		protected override void OnTextChanged(string oldValue, string newValue)
		{
			base.OnTextChanged(oldValue, newValue);
			if (EnableClearButton)
			{
				var emission = string.IsNullOrEmpty(newValue) ? "elm,action,hide,button" : "elm,action,show,button";
				_editfieldLayout.SignalEmit(emission, "");
			}
		}

		protected virtual ELayout CreateEditFieldLayout(EvasObject parent)
		{
			var layout = new ELayout(parent);
			layout.SetTheme("layout", "editfield", "singleline");
			layout.AllowFocus(true);
			layout.Unfocused += (s, e) =>
			{
				SetFocusOnTextBlock(false);
				layout.SignalEmit("elm,state,unfocused", "");
				LayoutUnfocused?.Invoke(this, EventArgs.Empty);
			};
			layout.Focused += (s, e) =>
			{
				layout.SignalEmit("elm,state,focused", "");
				LayoutFocused?.Invoke(this, EventArgs.Empty);
			};

			layout.KeyDown += (s, e) =>
			{
				if (e.KeyName == "Return")
				{
					if (!IsTextBlockFocused)
					{
						SetFocusOnTextBlock(true);
						e.Flags |= EvasEventFlag.OnHold;
					}
				}
			};
			Clicked += (s, e) => SetFocusOnTextBlock(true);

			Focused += (s, e) =>
			{
				layout.RaiseTop();
				layout.SignalEmit("elm,state,focused", "");
			};

			Unfocused += (s, e) =>
			{
				layout.SignalEmit("elm,state,unfocused", "");
			};

			return layout;
		}

		protected virtual void UpdateEnableClearButton()
		{
			if (EnableClearButton)
			{
				_clearButton = new Button(_editfieldLayout)
				{
					Style = "editfield_clear"
				};
				_clearButton.AllowFocus(false);
				_clearButton.Clicked += OnClearButtonClicked;

				_editfieldLayout.SetPartContent("elm.swallow.button", _clearButton);
				_editfieldLayout.SignalEmit("elm,action,show,button", "");
			}
			else
			{
				_editfieldLayout.SetPartContent("elm.swallow.button", null);
				_clearButton = null;
			}
		}

		void OnClearButtonClicked(object sender, EventArgs e)
		{
			Text = string.Empty;
		}
	}
}