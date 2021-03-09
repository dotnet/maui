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

				return Color.Default;
			}
		}
	}
}