using System.Collections.Generic;
using ObjCRuntime;
using Foundation;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	internal class ReadOnlyMaterialTextField : NoCaretMaterialTextField
	{
		readonly HashSet<string> _enableActions;

		public ReadOnlyMaterialTextField(IMaterialEntryRenderer element, IFontElement fontElement) : base(element, fontElement)
		{
			string[] actions = { "copy:", "select:", "selectAll:" };
			_enableActions = new HashSet<string>(actions);
		}

		public override bool CanPerform(Selector action, NSObject withSender)
			=> _enableActions.Contains(action.Name);
	}
}