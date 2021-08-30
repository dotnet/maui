using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class WrapperView
	{
		IShape? _clip;

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

		partial void ClipChanged();
	}
}