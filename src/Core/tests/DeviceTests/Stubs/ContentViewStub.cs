using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

#nullable enable

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ContentViewStub : StubBase, IContentView
	{
		ILayoutManager? _layoutManager;

		public object? Content { get; set; }

		public IView? PresentedContent { get; set; }

		public Thickness Padding { get; set; }

		ILayoutManager LayoutManager => _layoutManager ??= new LayoutManagerStub();

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return PresentedContent?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		public Size CrossPlatformArrange(Rect bounds)
		{
			return PresentedContent?.Arrange(bounds) ?? Size.Zero;
		}
	}
}