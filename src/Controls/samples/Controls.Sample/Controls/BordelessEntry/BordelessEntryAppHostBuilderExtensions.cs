// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Controls
{
	static class BordelessEntryAppHostBuilderExtensions
	{
		public static MauiAppBuilder UseBordelessEntry(this MauiAppBuilder builder, Action<BordelessEntryServiceBuilder> configureDelegate = null)
		{
			builder.Services.TryAddSingleton<IMauiInitializeService, BorderlessEntryInitializer>();

			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<BorderlessEntryRegistration>(new BorderlessEntryRegistration(configureDelegate));
			}

			return builder;
		}
	}
}
