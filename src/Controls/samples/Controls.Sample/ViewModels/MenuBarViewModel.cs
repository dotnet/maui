using Maui.Controls.Sample.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.ViewModels
{
	public class MenuBarViewModel : BaseViewModel
	{
		const string runnerGlyph = "\u26f9";
		const string tentGlyph = "\u26fa";

		string icon1;
		string icon2;
		FontImageSource iconImage1;
		FontImageSource iconImage2;

		public string Icon1
		{
			get => icon1;
			set => SetProperty(ref icon1, value);
		}

		public string Icon2
		{
			get => icon2;
			set => SetProperty(ref icon2, value);
		}

		public FontImageSource IconImage1
		{
			get => iconImage1;
			set => SetProperty(ref iconImage1, value);
		}

		public FontImageSource IconImage2
		{
			get => iconImage2;
			set => SetProperty(ref iconImage2, value);
		}

		public MenuBarViewModel()
		{
			Icon1 = runnerGlyph;
			Icon2 = tentGlyph;

			IconImage1 = new FontImageSource
			{
				FontFamily = "IconsFont",
				Glyph = runnerGlyph
			};

			IconImage2 = new FontImageSource
			{
				FontFamily = "IconsFont",
				Glyph = tentGlyph
			};
		}

		public void SwapIcon()
		{
			Icon1 = Icon1 == runnerGlyph ? tentGlyph : runnerGlyph;
			Icon2 = Icon2 == runnerGlyph ? tentGlyph : runnerGlyph;

			IconImage1.Glyph = IconImage1.Glyph == runnerGlyph ? tentGlyph : runnerGlyph;
			IconImage2.Glyph = IconImage2.Glyph == runnerGlyph ? tentGlyph : runnerGlyph;
		}
	}
}