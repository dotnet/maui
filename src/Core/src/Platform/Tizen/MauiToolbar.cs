using System;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using NColor = Tizen.NUI.Color;
using NShadow = Tizen.NUI.Shadow;
using NVector2 = Tizen.NUI.Vector2;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public class MauiToolbar : TitleView
	{
		const double s_toolbarTextSize = 20d;
		const double s_toolbarHeight = 50d;

		//It is workaround, Tizen.UIExtension should be updated to show either Title or Content.
		public NView? SearchBar
		{
			get => base.Content;
			set
			{
				base.Content = value;

				if (base.Content == null)
					Label.SizeWidth = SizeWidth;
				else
					Label.SizeWidth = 0;
			}
		}

		public MauiToolbar()
		{
			BoxShadow = new NShadow(5d.ToScaledPixel(), NColor.Black, new NVector2(0, 0));
			Label.FontSize = s_toolbarTextSize.ToScaledPoint();
			SizeHeight = s_toolbarHeight.ToScaledPixel();
		}

		public event EventHandler? IconPressed;

		public void Expand()
		{
			SizeHeight = s_toolbarHeight.ToScaledPixel();
		}

		public void Collapse()
		{
			SizeHeight = 0;
		}

		public void SendIconPressed()
		{
			IconPressed?.Invoke(this, EventArgs.Empty);
		}
	}
}