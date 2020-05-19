using ElmSharp;
using ELayout = ElmSharp.Layout;

namespace System.Maui.Platform.Tizen
{
	public class PreloadedWindow : Window
	{
		static PreloadedWindow s_precreated;

		public PreloadedWindow() : base("FormsWindow-pre")
		{
			s_precreated = this;
			Initialize();
		}

		public ELayout BaseLayout
		{
			get;
			protected set;
		}

		protected void Initialize()
		{
			var conformant = new Conformant(this);
			conformant.Show();

			var layout = new ELayout(conformant);
			layout.SetTheme("layout", "application", "default");
			layout.Show();

			BaseLayout = layout;
			conformant.SetContent(BaseLayout);
		}

		public static PreloadedWindow GetInstance()
		{
			var instance = s_precreated;
			s_precreated = null;
			return instance;
		}
	}
}
