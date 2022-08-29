using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tizen.NUI;
using NEntry = Tizen.UIExtensions.NUI.Entry;
using NView = Tizen.NUI.BaseComponents.View;
using NColor = Tizen.NUI.Color;

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
				Color = _defaultUnderlineColor,
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
			_underline.Color = enabled ? _defaultUnderlineColor : NColor.LightGray;
		}
	}
}