using System;
using ElmSharp;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using ELayout = ElmSharp.Layout;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class SearchBar : Native.EditfieldEntry
	{
		EButton _clearButton;
		ELayout _layout;

		public SearchBar(EvasObject parent) : base(parent)
		{
		}

		public void SetClearButtonColor(EColor color)
		{
			_clearButton.Color = color;
		}

		protected override ElmSharp.Layout CreateEditFieldLayout(EvasObject parent)
		{
			_layout =  base.CreateEditFieldLayout(parent);

			_clearButton = new EButton(_layout)
			{
				Style = "editfield_clear"
			};
			_clearButton.AllowFocus(false);
			_clearButton.Clicked += ClearButtonClicked;

			_layout.SetPartContent("elm.swallow.button", _clearButton);
			_layout.SignalEmit("elm,action,show,button", "");

			return _layout;
		}

		protected override void OnTextChanged(string oldValue, string newValue)
		{
			base.OnTextChanged(oldValue, newValue);

			if (String.IsNullOrEmpty(Text))
			{
				_layout.SignalEmit("elm,action,hide,button", "");
			}
			else
			{
				_layout.SignalEmit("elm,action,show,button", "");
			}
		}

		void ClearButtonClicked(object sender, EventArgs e)
		{
			Text = string.Empty;
		}
	}
}