using System;

namespace Microsoft.Maui;

// TODO .NET9/10 Make this public at some point. 
internal interface IHandlerBehaviors
{
    HandlerDisconnectPolicy DisconnectPolicy { get; set;}
}
