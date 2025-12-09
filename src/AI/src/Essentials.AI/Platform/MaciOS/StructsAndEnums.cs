namespace Microsoft.Maui.Essentials.AI;

internal enum ChatClientError : long
{
	EmptyMessages = 1,
	InvalidRole = 2,
	InvalidContent = 3,
	Cancelled = 4
}

internal enum ChatRoleNative : long
{
	User = 1,
	Assistant = 2,
	System = 3,
	Tool = 4,
}

internal enum ResponseUpdateTypeNative : long
{
	Content = 0,
	ToolCall = 1,
	ToolResult = 2
}
