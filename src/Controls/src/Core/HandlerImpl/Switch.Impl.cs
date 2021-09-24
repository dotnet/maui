using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Switch : ISwitch
	{
		Color ISwitch.TrackColor
		{
			get
			{
#if WINDOWS
				return OnColor;
#else
				if (IsToggled)
					return OnColor;

				return null;
#endif
			}
		}

		bool ISwitch.IsOn
		{
			get => IsToggled;
			set => IsToggled = value;
		}
	}
}