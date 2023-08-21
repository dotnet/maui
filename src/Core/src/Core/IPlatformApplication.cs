// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// IPlatformApplication.
	/// Hosts the platform application.
	/// </summary>
	public interface IPlatformApplication
	{

#if !NETSTANDARD2_0
		/// <summary>
		/// Gets the current IPlatformApplication.
		/// This must be set in each implementation manually, as we can't
		/// have a true static be used in the implementation.
		/// </summary>
		public static IPlatformApplication? Current { get; set; }
#endif

		/// <summary>
		/// Gets the Service Provider.
		/// <see cref="IServiceProvider"/>.
		/// </summary>
		public IServiceProvider Services { get; }

		/// <summary>
		/// Gets the Application.
		/// <see cref="IApplication"/>.
		/// </summary>
		public IApplication Application { get; }
	}
}
