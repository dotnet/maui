namespace Microsoft.Maui.Controls
{
	internal abstract record RadioButtonScopeMessage(Element Scope);

	internal sealed record RadioButtonGroupSelectionChanged(Element Scope, RadioButton RadioButton) 
		: RadioButtonScopeMessage(Scope);

	internal sealed record RadioButtonGroupNameChanged(Element Scope, string OldName) 
		: RadioButtonScopeMessage(Scope);

	internal sealed record RadioButtonValueChanged(Element Scope, RadioButton RadioButton)
		: RadioButtonScopeMessage(Scope);

	internal sealed record RadioButtonGroupValueChanged (Element Scope, object Value, string GroupName) 
		: RadioButtonScopeMessage(Scope);
}