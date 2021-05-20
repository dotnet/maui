using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, object>
	{
		public void Add(IView view) => throw new NotImplementedException();
		public void Remove(IView view) => throw new NotImplementedException();

		protected override object CreateNativeView() => throw new NotImplementedException();

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => throw new NotImplementedException();
	}
}
