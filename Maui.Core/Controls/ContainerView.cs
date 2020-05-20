using System;
using System.Maui.Shapes;

namespace System.Maui.Core.Controls {
	public partial class ContainerView
	{
		private IShape _clipShape;
		public IShape ClipShape {
			get => _clipShape;
			set
			{
				if (_clipShape == value)
					return;
				_clipShape = value;
				ClipShapeChanged();
			}
		}

		partial void ClipShapeChanged();
	}
}
