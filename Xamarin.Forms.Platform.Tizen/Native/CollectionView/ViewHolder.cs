using System;
using ElmSharp;
using EColor = ElmSharp.Color;


namespace Xamarin.Forms.Platform.Tizen.Native
{

	public enum ViewHolderState
	{
		Normal,
		Selected,
		Focused,
	}

	public class ViewHolder : Box
	{
		Button _focusArea;
		EvasObject _content;
		ViewHolderState _state;
		bool _isSelected;

		public ViewHolder(EvasObject parent) : base(parent)
		{
			Initialize(parent);
		}

		public object ViewCategory { get; set; }

		[Obsolete("FocusedColor is obsolete. Use VisualStateManager")]
		public EColor FocusedColor { get; set; }

		[Obsolete("SelectedColor is obsolete. Use VisualStateManager")]
		public EColor SelectedColor { get; set; }

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
					PackEnd(_content);
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

		public event EventHandler RequestSelected;

		public event EventHandler StateUpdated;

		public void ResetState()
		{
			State = ViewHolderState.Normal;
		}

		protected void Initialize(EvasObject parent)
		{
			SetLayoutCallback(OnLayout);

			_focusArea = new Button(parent);
			_focusArea.Color = EColor.Transparent;
			_focusArea.BackgroundColor = EColor.Transparent;
			_focusArea.SetEffectColor(EColor.Transparent);
			_focusArea.Clicked += OnClicked;
			_focusArea.Focused += OnFocused;
			_focusArea.Unfocused += OnFocused;
			_focusArea.KeyUp += OnKeyUp;
			_focusArea.RepeatEvents = true;
			_focusArea.Show();

			PackEnd(_focusArea);
			Show();
		}

		protected virtual void OnFocused(object sender, EventArgs e)
		{
			if (_focusArea.IsFocused)
			{
				State = ViewHolderState.Focused;
			}
			else
			{
				State = _isSelected ? ViewHolderState.Selected : ViewHolderState.Normal;
			}
		}

		protected virtual void OnClicked(object sender, EventArgs e)
		{
			RequestSelected?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnLayout()
		{
			_focusArea.Geometry = Geometry;
			if (_content != null)
			{
				_content.Geometry = Geometry;
			}
		}

		protected virtual void UpdateState()
		{
			if (State == ViewHolderState.Selected)
				_isSelected = true;
			else if (State == ViewHolderState.Normal)
				_isSelected = false;

			StateUpdated?.Invoke(this, EventArgs.Empty);
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
