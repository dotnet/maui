using System.Maui.Internals;

namespace System.Maui.Platform.Tizen
{
	internal class XamarinLogListener : LogListener
	{
		public XamarinLogListener()
		{
		}

		#region implemented abstract members of LogListener

		public override void Warning(string category, string message)
		{
			Log.Warn("[{0}] {1}", category, message);
		}

		#endregion
	}
}
