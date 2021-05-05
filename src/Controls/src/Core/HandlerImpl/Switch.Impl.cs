using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Switch : ISwitch
	{
		Color ISwitch.TrackColor
		{
			get
			{
				if (IsToggled)
					return OnColor;

				return null;
			}
		}

		bool ISwitch.IsOn
		{
			get => IsToggled;
			set => IsToggled = value;
		}
	}
}