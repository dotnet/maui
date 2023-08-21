// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformComposeAsync(EmailMessage message) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
