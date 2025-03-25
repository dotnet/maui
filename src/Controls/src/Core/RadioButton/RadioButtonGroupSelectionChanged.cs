#nullable disable
namespace Microsoft.Maui.Controls
{
	internal abstract class RadioButtonScopeMessage
	{
		public Element Scope { get; }

		protected RadioButtonScopeMessage(Element scope) => Scope = scope;
	}

	internal class RadioButtonGroupSelectionChanged : RadioButtonScopeMessage
	{
		public object Value { get; }

		public RadioButtonGroupSelectionChanged(Element scope, object value) : base(scope)
		{
			Value = value;
		}
	}

	internal class RadioButtonGroupNameChanged : RadioButtonScopeMessage
	{
		public string OldName { get; }

		public RadioButtonGroupNameChanged(Element scope, string oldName) : base(scope)
		{
			OldName = oldName;
		}
	}

	internal class RadioButtonValueChanged : RadioButtonScopeMessage
	{
		public RadioButtonValueChanged(Element scope) : base(scope) { }
	}

	internal class RadioButtonGroupValueChanged : RadioButtonScopeMessage
	{
		public object Value { get; }
		public string GroupName { get; }

		public RadioButtonGroupValueChanged(string groupName, Element scope, object value) : base(scope)
		{
			GroupName = groupName;
			Value = value;
		}
	}
}