using System;

namespace Xamarin.Forms.Controls
{
	public class StyleGallery:ContentPage
	{
		public StyleGallery ()
		{
			Content = new StackLayout {
				Children = {
					new Label {
						Text = "This uses TitleStyle",
						Style = Device.Styles.TitleStyle
					},
					new Label {
						Text = "This uses SubtitleStyle",
						Style = Device.Styles.SubtitleStyle
					},
					new Label {
						Text = "This uses BodyStyle",
						Style = Device.Styles.BodyStyle
					},
					new Label {
						Text = "This uses CaptionStyle",
						Style = Device.Styles.CaptionStyle
					},
					new Label {
						Text = "This uses a custom style inherited dynamically from SubtitleStyle",
						Style = new Style (typeof(Label)) {
							BaseResourceKey = Device.Styles.SubtitleStyleKey,
							Setters = {
								new Setter {Property = Label.TextColorProperty, Value = Color.Pink}
							}
						}
					},
				}
			};
		}
	}
}

