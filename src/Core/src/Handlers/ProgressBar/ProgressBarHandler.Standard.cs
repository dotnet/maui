// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapProgress(IProgressBarHandler handler, IProgress progress) { }
		public static void MapProgressColor(IProgressBarHandler handler, IProgress progress) { }
	}
}