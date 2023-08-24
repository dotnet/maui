using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Embedding
{
	public static class AppHostBuilderExtensions
	{
		public static MauiAppBuilder UseMauiEmbedding<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
			where TApp : class, IApplication
				=> builder.UseMauiApp<TApp>();

		public static MauiAppBuilder UseMauiEmbedding<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
			where TApp : class, IApplication
				=> builder.UseMauiApp(implementationFactory);
	}
}
