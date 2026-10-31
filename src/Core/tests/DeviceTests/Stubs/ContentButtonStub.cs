using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ContentButtonStub : StubBase, IContentButton
	{
		public IView Content { get; set; }
		object IContentView.Content => Content;
		public IView PresentedContent => Content;
		public Thickness Padding { get; set; }
		public Color StrokeColor { get; set; }
		public double StrokeThickness { get; set; } = -1;
		public int CornerRadius { get; set; } = -1;
		public Action Clicked { get; set; }
		public Action Pressed { get; set; }
		public Action Released { get; set; }

		void IButton.Clicked() => Clicked?.Invoke();
		void IButton.Pressed() => Pressed?.Invoke();
		void IButton.Released() => Released?.Invoke();

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
			ContentButtonLayout.Measure(this, widthConstraint, heightConstraint);
		public Size CrossPlatformArrange(Rect bounds) => ContentButtonLayout.Arrange(this, bounds);
	}
}
