using System;
using System.Collections.Generic;

namespace Xamarin.Platform.Handlers.DeviceTests.Stubs
{
	public class LayoutStub : StubBase, ILayout
	{
		List<IView> _children = new List<IView>();

		public IReadOnlyList<IView> Children => _children.AsReadOnly();

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

		public void Add(IView child)
		{
			_children.Add(child);
		}

		public void Remove(IView child)
		{
			_children.Remove(child);
		}
	}
}
