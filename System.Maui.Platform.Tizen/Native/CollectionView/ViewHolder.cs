using System;
using ElmSharp;
using ERectangle = ElmSharp.Rectangle;
using EColor = ElmSharp.Color;


namespace Xamarin.Forms.Platform.Tizen.Native
{

	public enum ViewHolderState
	{
		Normal,
		Selected,
	}

	public class ViewHolder : Box
	{
		static readonly EColor s_defaultFocusEffectColor = EColor.FromRgba(244, 244, 244, 200);
		static readonly EColor s_defaultSelectedColor = EColor.FromRgba(227, 242, 253, 200);

		ERectangle _background;
		Button _focusArea;
		EvasObject _content;
		ViewHolderState _state;

		public ViewHolder(EvasObject parent) : base(parent)
		{
			Initialize(parent);
		}

		public object ViewCategory { get; set; }
		public EColor FocusedColor { get; set; }
		public EColor SelectedColor { get; set; }

		EColor EffectiveFocusedColor => FocusedColor == EColor.Default ? s_defaultFocusEffectColor : FocusedColor;
		EColor EffectiveSelectedColor => SelectedColor == EColor.Default ? s_defaultSelectedColor : FocusedColor;

		EColor FocusSelectedColor
		{
			get
			{
				var color1 = EffectiveFocusedColor;
				var color2 = EffectiveSelectedColor;
				return new EColor(
					(color1.R + color2.R) / 2,
					(color1.G + color2.G) / 2,
					(color1.B + color2.B) / 2,
					(color1.A + color2.A) / 2);
			}
		}

		public EvasObject Content
		{
			get
			{
				return _content;
			}
			set
			{
				if (_content != null)
				{
					UnPack(_content);
				}
				_content = value;
				if (_content != null)
				{
					PackAfter(_content, _background);
					_content.StackBelow(_focusArea);
				}
			}
		}

		public ViewHolderState State
		{
			get { return _state; }
			set
			{
				_state = value;
				UpdateState();
			}
		}

		public event EventHandler Selected;
		public event EventHandler RequestSelected;

		public void ResetState()
		{
			State = ViewHolderState.Normal;
			_background.Color = EColor.Transparent;
		}

		protected void SendSelected()
		{
			Selected?.Invoke(this, EventArgs.Empty);
		}

		protected void Initialize(EvasObject parent)
		{
			SetLayoutCallback(OnLayout);

			_background = new ERectangle(parent)
			{
				Color = EColor.Transparent
			};
			_background.Show();

			_focusArea = new Button(parent);
			_focusArea.Color = EColor.Transparent;
			_focusArea.BackgroundColor = EColor.Transparent;
			_focusArea.SetPartColor("effect", EColor.Transparent);
			_focusArea.Clicked += OnClicked;
			_focusArea.Focused += OnFocused;
			_focusArea.Unfocused += OnFocused;
			_focusArea.KeyUp += OnKeyUp;
			_focusArea.RepeatEvents = true;
			_focusArea.Show();

			PackEnd(_background);
			PackEnd(_focusArea);
			FocusedColor = EColor.Default;
			Show();
		}

		protected virtual void OnFocused(object sender, EventArgs e)
		{
			if (_focusArea.IsFocused)
			{
				_background.Color = State == ViewHolderState.Selected ? FocusSelectedColor : EffectiveFocusedColor;
			}
			else
			{
				_background.Color = State == ViewHolderState.Selected ? EffectiveSelectedColor : EColor.Transparent;
			}
		}

		protected virtual void OnClicked(object sender, EventArgs e)
		{
			RequestSelected?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnLayout()
		{
			_background.Geometry = Geometry;
			_focusArea.Geometry = Geometry;
			if (_content != null)
			{
				_content.Geometry = Geometry;
			}
		}

		protected virtual void UpdateState()
		{
			if (State == ViewHolderState.Normal)
			{
				_background.Color = _focusArea.IsFocused ? EffectiveFocusedColor : EColor.Transparent;
			} else
			{
				_background.Color = _focusArea.IsFocused ? FocusSelectedColor : SelectedColor;
				SendSelected();
			}
		}


		void OnKeyUp(object sender, EvasKeyEventArgs e)
		{
			if (e.KeyName == "Enter" && _focusArea.IsFocused)
			{
				RequestSelected?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
