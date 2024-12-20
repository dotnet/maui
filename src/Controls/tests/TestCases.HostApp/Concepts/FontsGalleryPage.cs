namespace Maui.Controls.Sample;

internal class FontsGalleryPage : CoreGalleryBasePage
{
	protected override void Build()
	{
		AddImage(Test.Fonts.FromEmbedded_Image, "q", "Dokdo");
		AddLabel(Test.Fonts.FromEmbedded_Label, "q", "Dokdo");

		AddImage(Test.Fonts.FromBundle_Image, "\xf133", "FA");
		// AddLabel(Test.Fonts.FromBundle_Label, "\xf133", "FA");

		

		{
			var label = new Label
			{
				FontFamily = "FA",
				FontSize = 48,
				Text = "\xf133",
				WidthRequest = 48,
				HeightRequest = 48,
			};
			var familyContainer = new StateViewContainer<Label>(Test.Fonts.FromBundle_Label, label);
			familyContainer.StateChangeButton.Clicked += (s, a) =>
			{
				label.FontFamily = label.FontFamily == "FA" ? "Ion" : "FA";
				label.Text = label.FontFamily == "FA" ? "\xf133" : "\xf30c";
			};
			Add(familyContainer);
		}
	}

	ViewContainer<View> AddImage(Test.Fonts test, string text, string fontFamily) =>
		Add(test, new Image
		{
			Source = new FontImageSource
			{
				Glyph = text,
				FontFamily = fontFamily,
				Size = 40,
				Color = Colors.Red
			},
			WidthRequest = 50,
			HeightRequest = 50,
		});

	ViewContainer<View> AddLabel(Test.Fonts test, string text, string fontFamily) =>
		Add(test, new Label
		{
			Text = text,
			FontFamily = fontFamily,
			FontSize = 40,
			TextColor = Colors.Red,
			WidthRequest = 50,
			HeightRequest = 50,
		});

	ViewContainer<View> Add(Test.Fonts test, View view) =>
		Add(new ViewContainer<View>(test, view));
}
