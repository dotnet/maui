#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface IButtonElement : ICommandElement
	{
		//note to implementor: implement this property publicly
		bool IsPressed { get; }


		//note to implementor: but implement these methods explicitly
		void PropagateUpClicked();
		void PropagateUpPressed();
		void PropagateUpReleased();
		void SetIsPressed(bool isPressed);
	}
}
