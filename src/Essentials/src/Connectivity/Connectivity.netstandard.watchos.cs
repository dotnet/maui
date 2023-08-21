// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Networking
{
	partial class ConnectivityImplementation : IConnectivity
	{
		public NetworkAccess NetworkAccess =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public IEnumerable<ConnectionProfile> ConnectionProfiles =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void StartListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void StopListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
