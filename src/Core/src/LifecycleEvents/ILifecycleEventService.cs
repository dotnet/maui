// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Maui.LifecycleEvents
{
	public interface ILifecycleEventService
	{
		IEnumerable<TDelegate> GetEventDelegates<TDelegate>(string eventName)
			where TDelegate : Delegate;

		bool ContainsEvent(string eventName);
	}
}