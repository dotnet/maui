using System;
using ElmSharp;
using ELayout = ElmSharp.Layout;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class EditfieldEntry : Native.Entry
	{
		public event EventHandler TextBlockFocused;
		public event EventHandler TextBlockUnfocused;

		public bool IsTextBlockFocused => _isTexstBlockFocused;

		ELayout _editfieldLayout;
		int _heightPadding = 0;
		bool _isTexstBlockFocused = false;

		public EditfieldEntry(EvasObject parent) : base(parent)
		{
		}

		public EditfieldEntry(EvasObject parent, string style) : base(parent)
		{
			if (!string.IsNullOrEmpty(style))
				_editfieldLayout.SetTheme("layout", "editfield", style);
		}

		public override ElmSharp.Color BackgroundColor
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

		public void SetFocusOnTextBlock(bool isFocused)
		{
			AllowFocus(isFocused);
			SetFocus(isFocused);
			_isTexstBlockFocused = isFocused;

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
			AllowFocus(false);
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

		protected virtual ELayout CreateEditFieldLayout(EvasObject parent)
		{
			var layout = new ELayout(parent);
			layout.SetTheme("layout", "editfield", "singleline");
			layout.AllowFocus(true);
			layout.Unfocused += (s, e) =>
			{
				SetFocusOnTextBlock(false);
				layout.SignalEmit("elm,state,unfocused", "");
			};
			layout.Focused += (s, e) =>
			{
				AllowFocus(false);
				layout.SignalEmit("elm,state,focused", "");
			};

			layout.KeyDown += (s, e) =>
			{
				if (e.KeyName == "Return")
				{
					if (!_isTexstBlockFocused)
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
	}
}