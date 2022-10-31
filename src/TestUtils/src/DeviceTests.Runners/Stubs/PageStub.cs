using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class PageStub : StubBase, ITitledElement, IContentView
	{
		private IView _content;
		private string _title;

		public object Content { get => _content; set => this.SetProperty(ref _content, (IView)value); }
		public IView PresentedContent => _content;

		public Thickness Padding { get; set; }

		public string Title { get => _title; set => this.SetProperty(ref _title, value); }

		public Size CrossPlatformArrange(Rect bounds)
		{
			return bounds.Size;
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}
	}
}
