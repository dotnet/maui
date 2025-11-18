namespace Microsoft.Maui.Essentials.AI;

public enum ChatClientError : long
{
	EmptyMessages = 1,
	InvalidRole = 2,
	InvalidContent = 3,
	Cancelled = 4
}

public enum ChatRoleNative : long
{
	User = 1,
	Assistant = 2,
	System = 3
}
