#nullable enable
using System.Collections.Generic;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiPicker : NoCaretField
	{
		readonly HashSet<string> _enableActions;

		public MauiPicker(UIPickerView? uIPickerView)
		{
			UIPickerView = uIPickerView;

			string[] actions = { "copy:", "select:", "selectAll:" };
			_enableActions = new HashSet<string>(actions);
		}

		public UIPickerView? UIPickerView { get; set; }

		public override bool CanPerform(Selector action, NSObject? withSender)
			=> _enableActions.Contains(action.Name);
	}
}
