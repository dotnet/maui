using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView
	{
		IShape? _clip;
		IShadow? _shadow;
		IBorderStroke? _border;

#if WINDOWS
		public new IShape? Clip
#else
		public IShape? Clip
#endif
		{
			get => _clip;
			set
			{
				_clip = value;
				ClipChanged();
			}
		}

#if WINDOWS
		public new IShadow? Shadow
#else
		public IShadow? Shadow
#endif

		{
			get => _shadow;
			set
			{
				_shadow = value;
				ShadowChanged();
			}
		}
		public IBorderStroke? Border
		{
			get => _border;
			set
			{
				_border = value;
				BorderChanged();
			}
		}

		partial void ClipChanged();
		partial void ShadowChanged();
		partial void BorderChanged();
	}
}