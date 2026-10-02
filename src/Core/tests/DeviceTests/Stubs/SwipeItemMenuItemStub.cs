using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class SwipeItemMenuItemStub : ElementStub, ISwipeItemMenuItem, ISwipeItemMenuItemIconColor
	{
		public string AutomationId { get; set; }

		public Paint Background { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; } = Font.Default;

		public Color IconColor { get; set; }

		public bool IsAnimationPlaying { get; set; }

		public bool IsEnabled { get; set; } = true;

		public IImageSource Source { get; set; }

		public string Text { get; set; }

		public Color TextColor { get; set; }

		public Visibility Visibility { get; set; } = Visibility.Visible;

		public void Clicked()
		{
		}

		public void OnInvoked()
		{
		}

		public void UpdateIsLoading(bool isLoading)
		{
		}
	}
}
