using Tizen.UIExtensions.Common;
using TImage = Tizen.UIExtensions.ElmSharp.Image;

namespace Microsoft.Maui.Platform
{
	public class MauiBackgroundImage : TImage
	{
		WrapperView _parent;
		public MauiBackgroundImage(WrapperView parent) : base(parent)
		{
			Aspect = Tizen.UIExtensions.Common.Aspect.Fill;

			_parent = parent;
			_parent.Children.Add(this);
			_parent.LayoutUpdated += OnLayout;
			Lower();
		}

		void OnLayout(object? sender, LayoutEventArgs e)
		{
			Geometry = _parent.Geometry;
		}

		protected override void OnUnrealize()
		{
			_parent.LayoutUpdated -= OnLayout;
			base.OnUnrealize();
		}
	}
}
