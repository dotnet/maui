using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class PageStub : StubBase, IPage
	{
		private IView _content;
		private string _title;

		public IView Content { get => _content; set => this.SetProperty(ref _content, value); }

		public string Title { get => _title; set => this.SetProperty(ref _title, value); }
	}
}
