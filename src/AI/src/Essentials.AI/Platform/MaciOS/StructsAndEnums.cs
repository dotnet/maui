namespace Microsoft.Maui.Essentials.AI;

public enum ChatClientError
{
	EmptyMessages = 1,
	InvalidRole = 2,
	InvalidContent = 3,
	Cancelled = 4
}

public enum ChatRoleNative
{
	User = 0,
	Assistant = 1,
	System = 2
}
