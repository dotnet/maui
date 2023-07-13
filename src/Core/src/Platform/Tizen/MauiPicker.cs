using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tizen.NUI;
using NColor = Tizen.NUI.Color;
using NEntry = Tizen.UIExtensions.NUI.Entry;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public class MauiPicker : NEntry
	{
		readonly NColor _defaultUnderlineColor = NColor.DarkGray;
		NView _underline;

		public MauiPicker()
		{
			_underline = new NView
			{
				BackgroundColor = _defaultUnderlineColor,
				SizeHeight = 1d.ToScaledPixel(),
				WidthResizePolicy = ResizePolicyType.FillToParent,
				ParentOrigin = Position.ParentOriginBottomLeft
			};

			IsReadOnly = true;
			Focusable = true;
			VerticalAlignment = VerticalAlignment.Center;
			Add(_underline);
		}

		protected override void OnEnabled(bool enabled)
		{
			base.OnEnabled(enabled);
			_underline.BackgroundColor = enabled ? _defaultUnderlineColor : NColor.LightGray;
		}
	}
}
