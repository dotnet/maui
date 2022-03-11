namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSourceMode.xml" path="Type[@FullName='Microsoft.Maui.Controls.RelativeBindingSourceMode']/Docs" />
	public enum RelativeBindingSourceMode
	{
		// 0 reserved for possible future implementation of PreviousData 

		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSourceMode.xml" path="//Member[@MemberName='TemplatedParent']/Docs" />
		TemplatedParent = 1,
		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSourceMode.xml" path="//Member[@MemberName='Self']/Docs" />
		Self = 2,
		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSourceMode.xml" path="//Member[@MemberName='FindAncestor']/Docs" />
		FindAncestor = 3,
		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSourceMode.xml" path="//Member[@MemberName='FindAncestorBindingContext']/Docs" />
		FindAncestorBindingContext = 4,
	}
}
