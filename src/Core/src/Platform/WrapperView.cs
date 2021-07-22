using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class WrapperView
	{
		IShape? _clip;
		Shadow? _shadow;

		public IShape? Clip
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

		public Shadow? Shadow
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