using ElmSharp;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native;
using ELayout = ElmSharp.Layout;
using EWindow = ElmSharp.Window;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class PreloadedWindow : EWindow
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

			var layout = new ApplicationLayout(conformant);
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
