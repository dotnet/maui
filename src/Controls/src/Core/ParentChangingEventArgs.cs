#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public class ParentChangingEventArgs
	{
		public Element NewParent { get; }
		public Element OldParent { get; }

		public ParentChangingEventArgs(Element oldParent, Element newParent)
		{
			NewParent = newParent;
			OldParent = oldParent;
		}
	}
}
