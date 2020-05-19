using System;
using System.Collections.Generic;
using System.Text;
using System.Maui.Internals;

namespace System.Maui.Controls.Effects
{
	[Preserve(AllMembers = true)]
	public class AttachedStateEffectLabel : Label
	{
		// Android renderers don't detach effects when the renderers get disposed
		// so this is a hack setup to detach those effects when testing if dispose is called from a renderer
		// https://github.com/xamarin/System.Maui/issues/2520
	}
}
