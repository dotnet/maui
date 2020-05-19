using System;
using ElmSharp;
using Tizen.NET.MaterialComponents;
using System.Maui.Platform.Tizen.Native;

namespace System.Maui.Material.Tizen.Native
{
	public class MEditor : MaterialEntry
	{
		bool _isTexstBlockFocused = false;
		int _heightPadding = 0;

		public MEditor(EvasObject parent) : base(parent)
		{
			Initialize();
		}

		public override ElmSharp.Size Measure(int availableWidth, int availableHeight)
		{
			var textBlockSize = base.Measure(availableWidth, availableHeight);

			textBlockSize.Width += Layout.MinimumWidth;

			if (textBlockSize.Height < Layout.MinimumHeight)
				textBlockSize.Height = Layout.MinimumHeight;
			else
				textBlockSize.Height += _heightPadding;

			return textBlockSize;
		}

		protected override void OnFocused(object sender, EventArgs args)
		{
			//To prevent enabling Label
		}

		protected override void OnUnfocused(object sender, EventArgs args)
		{
			//To prevent enabling Label
		}

		protected override void OnLayoutFocused(object sender, EventArgs args)
		{
			Layout.SignalEmit(States.Focused, "");
		}

		protected override void OnLayoutUnFocused(object sender, EventArgs args)
		{
			_isTexstBlockFocused = false;
			Layout.SignalEmit(States.Unfocused, "");
		}

		void Initialize()
		{
			Layout.KeyDown += (s, e) =>
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

			var gesture = new GestureLayer(Layout);
			gesture.Attach(Layout);
			gesture.SetTapCallback(GestureLayer.GestureType.Tap, GestureLayer.GestureState.End, (data) => SetFocusOnTextBlock(true));

			Clicked += (s, e) => SetFocusOnTextBlock(true);
		}

		void SetFocusOnTextBlock(bool isFocused)
		{
			AllowFocus(isFocused);
			SetFocus(isFocused);
			_isTexstBlockFocused = isFocused;

			if (isFocused)
				OnTextBlockFocused();
			else
				OnTextBlcokUnfocused();
		}
	}
}