#nullable enable
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Bumptech.Glide;
using Java.IO;

namespace Microsoft.Maui.BumptechGlide
{
	// This class name is very specific as it is directly loaded.
	// TODO: make this extensible
	[Register("com.bumptech.glide.GeneratedAppGlideModuleImpl")]
	public class MauiAppGlideModule : GeneratedAppGlideModule
	{
		public MauiAppGlideModule(Context context)
		{
		}

		public override void RegisterComponents(Context context, Glide glide, Registry registry) => registry
			.Prepend(GetClass<InputStream>(), GetClass<InputStream>(), new PassThroughModelLoader<InputStream>.Factory())
			.Append(GetClass<FontImageSourceModel>(), GetClass<FontImageSourceModel>(), new PassThroughModelLoader<FontImageSourceModel>.Factory())
			.Append(GetClass<FontImageSourceModel>(), GetClass<Bitmap>(), new FontImageSourceDecoder());

		public override ICollection<Java.Lang.Class> ExcludedModuleClasses =>
			Array.Empty<Java.Lang.Class>();

		static Java.Lang.Class GetClass<T>() =>
			Java.Lang.Class.FromType(typeof(T));
	}
}