using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class WrapperView
	{
		IShape? _clip;

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

		partial void ClipChanged();
	}
}