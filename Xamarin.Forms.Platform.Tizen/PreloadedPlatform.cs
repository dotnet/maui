using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class PreloadedPlatform : DefaultPlatform
	{
		static PreloadedPlatform s_precreated;

		public PreloadedPlatform(EvasObject parent) : base(parent)
		{
			Parent = parent;
			s_precreated = this;
		}

		EvasObject Parent { get; }
		
		public static ITizenPlatform GetInstalce(EvasObject parent)
		{
			var instance = s_precreated;
			if (instance == null || instance.Parent != parent)
			{
				instance?.Dispose();
				instance = null;
			}
			return instance;
		}
	}
}
