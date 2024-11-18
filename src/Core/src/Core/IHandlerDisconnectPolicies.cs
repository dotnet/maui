using System;

namespace Microsoft.Maui;

// TODO .NET9/10 Discuss what to do with this
// - default interface method on iElement?
// - new interface?
internal interface IHandlerDisconnectPolicies
{
	HandlerDisconnectPolicy DisconnectPolicy { get; set; }
}
