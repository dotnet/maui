using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Switch.xml" path="Type[@FullName='Microsoft.Maui.Controls.Switch']/Docs" />
	public partial class Switch : ISwitch
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == IsToggledProperty.PropertyName)
				Handler?.UpdateValue(nameof(ISwitch.IsOn));
		}

		Color ISwitch.TrackColor
		{
			get
			{
#if WINDOWS
				return OnColor;
#else
				if (IsToggled)
					return OnColor ?? DefaultStyles.GetColor(this, OnColorProperty)?.Value as Color;

				return null;
#endif
			}
		}

		Color ISwitch.ThumbColor
		{
			get => ThumbColor ??
				DefaultStyles.GetColor(this, ThumbColorProperty)?.Value as Color;
		}

		bool ISwitch.IsOn
		{
			get => IsToggled;
			set => IsToggled = value;
		}
	}
}