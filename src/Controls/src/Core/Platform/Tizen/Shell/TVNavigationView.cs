using ElmSharp;
using EColor = ElmSharp.Color;
using TNavigationView = Tizen.UIExtensions.ElmSharp.NavigationView;

namespace Microsoft.Maui.Controls.Platform
{
	public class TVNavigationView : TNavigationView
	{
		EColor _backgroundColor;

		public TVNavigationView(EvasObject parent) : base(parent)
		{
			BackgroundColor = this.GetTvDefaultBackgroundColor().ToNativeEFL();
		}

		public override EColor BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				base.BackgroundColor = _backgroundColor.IsDefault ? this.GetTvDefaultBackgroundColor().ToNativeEFL() : _backgroundColor;
			}
		}

	}
}
