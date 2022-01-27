using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/PresentationMode.xml" path="Type[@FullName='Microsoft.Maui.Controls.PresentationMode']/Docs" />
	[Flags]
	public enum PresentationMode
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/PresentationMode.xml" path="//Member[@MemberName='NotAnimated']/Docs" />
		NotAnimated = 1,
		/// <include file="../../../docs/Microsoft.Maui.Controls/PresentationMode.xml" path="//Member[@MemberName='Animated']/Docs" />
		Animated = 1 << 1,
		/// <include file="../../../docs/Microsoft.Maui.Controls/PresentationMode.xml" path="//Member[@MemberName='Modal']/Docs" />
		Modal = 1 << 2,
		/// <include file="../../../docs/Microsoft.Maui.Controls/PresentationMode.xml" path="//Member[@MemberName='ModalAnimated']/Docs" />
		ModalAnimated = PresentationMode.Animated | PresentationMode.Modal,
		/// <include file="../../../docs/Microsoft.Maui.Controls/PresentationMode.xml" path="//Member[@MemberName='ModalNotAnimated']/Docs" />
		ModalNotAnimated = PresentationMode.NotAnimated | PresentationMode.Modal
	}
}
