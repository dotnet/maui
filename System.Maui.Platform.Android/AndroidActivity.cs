using System;
using System.ComponentModel;

namespace System.Maui.Platform.Android
{
	[Obsolete("AndroidActivity is obsolete as of version 1.3.0. Please use FormsApplicationActivity instead.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class AndroidActivity : FormsApplicationActivity
	{
	}
}