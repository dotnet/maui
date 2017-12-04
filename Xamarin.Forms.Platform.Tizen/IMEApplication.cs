using System;
using ElmSharp;
using Tizen.Uix.InputMethod;

namespace Xamarin.Forms.Platform.Tizen
{
	public class IMEApplication : FormsApplication
	{
		public EditorWindow EditorWindow
		{
			get { return MainWindow as EditorWindow; }
		}

		protected IMEApplication()
		{
		}

		protected override void OnPreCreate()
		{
			Application.ClearCurrent();

			/*
			 * Since the IMEWindow class acquires window handle from InputMethod module and
			 * the handle is created internally when calling InputMethodEditor.Create() function,
			 * this needs to be called BEFORE creating new IMEWindow instance.
			 */
			InputMethodEditor.Create();
			MainWindow = InputMethodEditor.GetMainWindow();
			MainWindow.IndicatorMode = IndicatorMode.Hide;
		}

		protected override void OnTerminate()
		{
			InputMethodEditor.Destroy();
			base.OnTerminate();
		}
	}
}
