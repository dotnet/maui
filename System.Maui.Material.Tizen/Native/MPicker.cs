using System;
using ElmSharp;
using Tizen.NET.MaterialComponents;

namespace System.Maui.Material.Tizen.Native
{
	public class MPicker : MEditor
	{
		public MPicker(EvasObject parent) : base(parent)
		{
			IsSingleLine = true;
			IsEditable = false;
			InputPanelShowByOnDemand = true;
			HorizontalTextAlignment = Platform.Tizen.Native.TextAlignment.Center;
			SetVerticalTextAlignment(Parts.Entry.TextEdit, 0.5);
		}
	}
}