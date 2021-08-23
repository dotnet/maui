using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class WrapperView
	{
		IShape? _clip;
		Shadow? _shadow;

#if WINDOWS
		public new IShape? Clip
#else
		public IShape? Clip
#endif
		{
			get => _clip;
			set
			{
				if (_clip == value)
					return;

				_clip = value;
				ClipChanged();
			}
		}

#if WINDOWS
		public new Shadow? Shadow
#else
		public Shadow? Shadow
#endif

		{
			get => _shadow;
			set
			{
				_shadow = value;
				ShadowChanged();
			}
		}

		partial void ClipChanged();
		partial void ShadowChanged();
	}
}