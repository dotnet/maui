namespace Maui.Controls.Sample;

#pragma warning disable CS0618 // Type or member is obsolete
internal class FrameCoreGalleryPage : CoreGalleryPage<Frame>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(Frame element)
	{
		element.HeightRequest = 50;
		element.WidthRequest = 100;
		element.BorderColor = Colors.Olive;
	}

	protected override void Build()
	{
		base.Build();
	}
}
#pragma warning restore CS0618 // Type or member is obsolete