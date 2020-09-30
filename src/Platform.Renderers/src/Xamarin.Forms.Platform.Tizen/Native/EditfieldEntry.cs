using System;
using ElmSharp;
using EColor = ElmSharp.Color;
using ELayout = ElmSharp.Layout;

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
			if (!string.IsNullOrEmpty(style) && _editfieldLayout is FormsLayout formsLayout)
				formsLayout.SetTheme(formsLayout.ThemeClass, formsLayout.ThemeGroup, style);
		}

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
			get => _clearButton.GetIconColor();
			set => _clearButton.SetIconColor(value);
		}

		public void SetFocusOnTextBlock(bool isFocused)
		{
			SetFocus(isFocused);
			IsTextBlockFocused = isFocused;

			if (isFocused)
				OnTextBlockFocused();
			else
				OnTextBlcokUnfocused();
		}

		public override ElmSharp.Size Measure(int availableWidth, int availableHeight)
		{
			var textBlockSize = base.Measure(availableWidth, availableHeight);

			// Calculate the minimum size by adding the width of a TextBlock and an Editfield.
			textBlockSize.Width += _editfieldLayout.MinimumWidth;

			// If the height of a TextBlock is shorter than Editfield, use the minimun height of the Editfield.
			// Or add the height of the EditField to the TextBlock
			if (textBlockSize.Height < _editfieldLayout.MinimumHeight)
			{
				if (Device.Idiom == TargetIdiom.TV || Device.Idiom == TargetIdiom.Watch)
				{
					textBlockSize.Height = _editfieldLayout.MinimumHeight;
				}
				else
				{
					// Since the minimum height of EditFieldLayout too large, adjust it to an appropriate height.
					var adjustedMinHeight = _editfieldLayout.MinimumHeight - (_editfieldLayout.MinimumHeight - _heightPadding) / 2;
					textBlockSize.Height = textBlockSize.Height < adjustedMinHeight ? adjustedMinHeight : _editfieldLayout.MinimumHeight;
				}
			}
			else
			{
				textBlockSize.Height += _heightPadding;
			}

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

			_editfieldLayout.SetContentPart(this);
			_heightPadding = _editfieldLayout.GetContentPartEdjeObject().Geometry.Height;
			return _editfieldLayout;
		}

		protected override void OnTextChanged(string oldValue, string newValue)
		{
			base.OnTextChanged(oldValue, newValue);
			if (EnableClearButton && _editfieldLayout is EditFieldEntryLayout layout)
			{
				layout.SendButtonActionSignal(!string.IsNullOrEmpty(newValue));
			}
		}

		protected virtual ELayout CreateEditFieldLayout(EvasObject parent)
		{
			var layout = new EditFieldEntryLayout(parent, EditFieldEntryLayout.Styles.SingleLine);
			layout.AllowFocus(true);
			layout.Unfocused += (s, e) =>
			{
				SetFocusOnTextBlock(false);
				layout.SendFocusStateSignal(false);
				OnEntryLayoutUnfocused();
			};
			layout.Focused += (s, e) =>
			{
				layout.SendFocusStateSignal(true);
				OnEntryLayoutFocused();
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
				layout.SendFocusStateSignal(true);
			};

			Unfocused += (s, e) =>
			{
				layout.SendFocusStateSignal(false);
			};

			return layout;
		}

		protected virtual void UpdateEnableClearButton()
		{
			if (_editfieldLayout is EditFieldEntryLayout layout)
			{
				if (EnableClearButton)
				{
					_clearButton = (Button)new Button(_editfieldLayout).SetEditFieldClearStyle();
					_clearButton.AllowFocus(false);
					_clearButton.Clicked += OnClearButtonClicked;

					layout.SetButtonPart(_clearButton);
					layout.SendFocusStateSignal(true);
				}
				else
				{
					layout.SetButtonPart(null);
					_clearButton = null;
				}
			}
		}

		void OnClearButtonClicked(object sender, EventArgs e)
		{
			Text = string.Empty;
		}
	}
}