using ElmSharp;

namespace Microsoft.Maui
{
	public class BorderView : ContentCanvas
	{
		WrapperView? _wrapperView;
		IBorder _borderView;

		public BorderView(EvasObject parent, IBorder view) : base(parent, view)
		{
			_borderView = view;
		}

		public WrapperView? ContainerView
		{
			get
			{
				return _wrapperView;
			}
			set
			{
				_wrapperView = value;
				_wrapperView?.UpdateBorder(_borderView);
				_wrapperView?.UpdateBackground(_borderView.Background);
			}
		}
	}
}
