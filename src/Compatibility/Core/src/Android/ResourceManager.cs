using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using AndroidAppCompat = AndroidX.AppCompat.Content.Res.AppCompatResources;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class ResourceManager
	{
		const string _drawableDefType = "drawable";

		readonly static Lazy<ImageCache> _lruCache = new Lazy<ImageCache>(() => new ImageCache());
		static ImageCache GetCache() => _lruCache.Value;

		static Assembly _assembly;
		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Resource.designer.cs is in the root application assembly, which should be preserved.")]
		[UnconditionalSuppressMessage("Trimming", "IL2073", Justification = "Resource.designer.cs may be linked away, so don't worry if there are missing things.")]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
		static Type FindType(string name, string altName)
		{
			if (_assembly != null)
			{
				foreach (var type in _assembly.GetTypes())
				{
					if (type.Name == name || type.Name == altName)
						return type;
				}
			}
			return null;
		}
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
		static Type _drawableClass;
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
		static Type _resourceClass;
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
		static Type _styleClass;
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
		static Type _layoutClass;

		public static Type DrawableClass
		{
			[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			get
			{
				if (_drawableClass == null)
					_drawableClass = FindType("Drawable", "Resource_Drawable");
				return _drawableClass;
			}
			[param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			set
			{
				_drawableClass = value;
			}
		}

		public static Type ResourceClass
		{
			[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			get
			{
				if (_resourceClass == null)
					_resourceClass = FindType("Id", "Resource_Id");
				return _resourceClass;
			}
			[param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			set
			{
				_resourceClass = value;
			}
		}

		public static Type StyleClass
		{
			[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			get
			{
				if (_styleClass == null)
					_styleClass = FindType("Style", "Resource_Style");
				return _styleClass;
			}
			[param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			set
			{
				_styleClass = value;
			}
		}

		public static Type LayoutClass
		{
			[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			get
			{
				if (_layoutClass == null)
					_layoutClass = FindType("Layout", "Resource_Layout");
				return _layoutClass;
			}
			[param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
			set
			{
				_layoutClass = value;
			}
		}

		internal static async Task<Drawable> GetFormsDrawableAsync(this Context context, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (imageSource == null || imageSource.IsEmpty)
				return null;

			// try take a shortcut for files
			if (imageSource is FileImageSource fileImageSource)
			{
				var file = fileImageSource.File;
				var id = IdFromTitle(file, DrawableClass, "drawable", context);

				// try the drawables via id
				if (id != 0)
				{
					var drawable = AndroidAppCompat.GetDrawable(context, id);
					if (drawable != null)
						return drawable;
				}

				// try a direct file on the file system
				if (File.Exists(file))
				{
					using (var bitmap = await BitmapFactory.DecodeFileAsync(file).ConfigureAwait(false))
					{
						if (bitmap != null)
							return new BitmapDrawable(context.Resources, bitmap);
					}
				}

				// try the bitmap resources via id
				if (id != 0)
				{
					using (var bitmap = await BitmapFactory.DecodeResourceAsync(context.Resources, id).ConfigureAwait(false))
					{
						if (bitmap != null)
							return new BitmapDrawable(context.Resources, bitmap);
					}
				}
			}

			// fall back to the handler
			using (var bitmap = await context.GetFormsBitmapAsync(imageSource, cancellationToken).ConfigureAwait(false))
			{
				if (bitmap != null)
					return new BitmapDrawable(context.Resources, bitmap);
			}

			return null;
		}

		internal static async Task<Bitmap> GetFormsBitmapAsync(this Context context, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (imageSource == null || imageSource.IsEmpty)
				return null;

			var handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
			if (handler == null)
				return null;

			try
			{
				return await handler.LoadImageAsync(imageSource, context, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				Application.Current?.FindMauiContext()?.CreateLogger<ImageRenderer>()?.LogWarning("Image load cancelled");
#pragma warning restore CS0618 // Type or member is obsolete
			}
			catch (Exception ex)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				Application.Current?.FindMauiContext()?.CreateLogger<ImageRenderer>()?.LogWarning(ex, "Image load failed");
#pragma warning restore CS0618 // Type or member is obsolete
			}

			return null;
		}

		static bool IsDrawableSourceValid(this IVisualElementRenderer renderer, BindableObject bindable, out BindableObject element)
		{
			if ((renderer is IDisposedState disposed && disposed.IsDisposed) || (renderer != null && renderer.View == null))
				element = null;
			else if (bindable != null)
				element = bindable;
			else
				element = renderer.Element;

			return element != null;
		}

		internal static Task ApplyDrawableAsync(this IVisualElementRenderer renderer,
										  BindableProperty imageSourceProperty,
										  Context context,
										  Action<Drawable> onSet,
										  Action<bool> onLoading = null,
										  CancellationToken cancellationToken = default(CancellationToken))
		{
			return renderer.ApplyDrawableAsync(null, imageSourceProperty, context, onSet, onLoading, cancellationToken);
		}


		internal static async Task ApplyDrawableAsync(this IVisualElementRenderer renderer,
												BindableObject bindable,
												BindableProperty imageSourceProperty,
												Context context,
												Action<Drawable> onSet,
												Action<bool> onLoading = null,
												CancellationToken cancellationToken = default(CancellationToken))
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
			_ = imageSourceProperty ?? throw new ArgumentNullException(nameof(imageSourceProperty));
			_ = onSet ?? throw new ArgumentNullException(nameof(onSet));

			// make sure things are good before we start
			BindableObject element = null;

			if (!renderer.IsDrawableSourceValid(bindable, out element))
				return;

			onLoading?.Invoke(true);
			if (element.GetValue(imageSourceProperty) is ImageSource initialSource && !initialSource.IsEmpty)
			{
				try
				{
					string cacheKey = String.Empty;

					// Todo improve for other sources
					// volley the requests better up front so that if the same request comes in it isn't requeued
					if (initialSource is UriImageSource uri && uri.CachingEnabled)
					{
						cacheKey = Crc64.ComputeHashString(uri.Uri.ToString());
						var cacheObject = await GetCache().GetAsync(cacheKey, uri.CacheValidity, async () =>
						{
							var drawable = await context.GetFormsDrawableAsync(initialSource, cancellationToken);
							return drawable;
						});

						Drawable returnValue = null;
						if (cacheObject is Bitmap bitmap)
							returnValue = new BitmapDrawable(context.Resources, bitmap);
						else
							returnValue = cacheObject as Drawable;

						if (!renderer.IsDrawableSourceValid(bindable, out element))
							return;

						// only set if we are still on the same image
						if (element.GetValue(imageSourceProperty) == initialSource)
						{
							using (returnValue)
								onSet(returnValue);
						}
					}
					else
					{

						using (var drawable = await context.GetFormsDrawableAsync(initialSource, cancellationToken))
						{
							if (!renderer.IsDrawableSourceValid(bindable, out element))
								return;

							// only set if we are still on the same image
							if (element.GetValue(imageSourceProperty) == initialSource)
							{
								onSet(drawable);
							}
						}
					}
				}
				finally
				{
					if (element != null && onLoading != null)
					{
						// only mark as finished if we are still on the same image
						if (element.GetValue(imageSourceProperty) == initialSource)
							onLoading.Invoke(false);
					}
				}
			}
			else
			{
				onSet(null);
				onLoading?.Invoke(false);
			}
		}

		internal static async Task ApplyDrawableAsync(this Context context,
												BindableObject bindable,
												BindableProperty imageSourceProperty,
												Action<Drawable> onSet,
												Action<bool> onLoading = null,
												CancellationToken cancellationToken = default(CancellationToken))
		{

			await ApplyDrawableAsync(null, bindable, imageSourceProperty, context, onSet, onLoading, cancellationToken);
		}

		public static Bitmap GetBitmap(this Resources resource, FileImageSource fileImageSource)
		{
			var file = fileImageSource.File;

			var bitmap = GetBitmap(resource, file);
			if (bitmap != null)
				return bitmap;

			return BitmapFactory.DecodeFile(file);
		}

		public static Bitmap GetBitmap(this Resources resource, string name)
		{
			return BitmapFactory.DecodeResource(resource, IdFromTitle(name, DrawableClass, _drawableDefType, resource));
		}

		public static Bitmap GetBitmap(this Resources resource, string name, Context context)
		{
			return BitmapFactory.DecodeResource(resource, IdFromTitle(name, DrawableClass, _drawableDefType, resource, context.PackageName));
		}

		public static Task<Bitmap> GetBitmapAsync(this Resources resource, string name)
		{
			return BitmapFactory.DecodeResourceAsync(resource, IdFromTitle(name, DrawableClass, _drawableDefType, resource));
		}

		public static Task<Bitmap> GetBitmapAsync(this Resources resource, string name, Context context)
		{
			return BitmapFactory.DecodeResourceAsync(resource, IdFromTitle(name, DrawableClass, _drawableDefType, resource, context.PackageName));
		}

		public static Drawable GetDrawable(this Context context, string name)
		{
			int id = IdFromTitle(name, DrawableClass, _drawableDefType, context);

			if (id == 0)
			{
				Application.Current?.FindMauiContext()?.CreateLogger(nameof(ResourceManager)).LogWarning("Could not load image named: {name}", name);
				return null;
			}

			return AndroidAppCompat.GetDrawable(context, id);
		}

		public static int GetDrawableId(this Context context, string title)
		{
			return IdFromTitle(title, DrawableClass, _drawableDefType, context);
		}

		public static int GetResource(this Context context, string title)
		{
			return IdFromTitle(title, ResourceClass, "id", context);
		}

		public static int GetLayout(this Context context, string name)
		{
			return IdFromTitle(name, LayoutClass, "layout", context);
		}

		public static int GetStyle(this Context context, string name)
		{
			return IdFromTitle(name, StyleClass, "style", context);
		}

		public static void Init(Assembly mainAssembly)
		{
			_assembly = mainAssembly;
		}

		static int IdFromTitle(string title, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type resourceType, string defType, Resources resource)
		{
#pragma warning disable CS0612 // Type or member is obsolete
			return IdFromTitle(title, resourceType, defType, resource, Platform.GetPackageName());
#pragma warning disable CS0612 // Type or member is obsolete
		}

		static int IdFromTitle(string title, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type resourceType, string defType, Context context)
		{
			return IdFromTitle(title, resourceType, defType, context?.Resources, context?.PackageName);
		}

		static int IdFromTitle(string title, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type resourceType, string defType, Resources resource, string packageName)
		{
			int id = 0;
			if (title == null)
				return id;

			string name;

			if (defType == "style" || (resourceType != null && resourceType == StyleClass))
				name = title;
			else
				name = title.ToLower();

			if (defType == _drawableDefType || (resourceType != null && resourceType == DrawableClass))
				name = IOPath.GetFileNameWithoutExtension(name);

			if ((id = SearchByIdentifier(name, defType, resource, packageName)) > 0)
				return id;

			// When searching by reflection you would use a "_" instead of a "."
			// So this accounts for cases where users were searching with an "_"
			if ((id = SearchByIdentifier(name.Replace("_", ".", StringComparison.Ordinal), defType, resource, packageName)) > 0)
				return id;

			int SearchByIdentifier(string n, string d, Resources r, string p)
			{
				int returnValue = 0;

				if (p != null)
					returnValue = r.GetIdentifier(n, d, p);

				if (returnValue == 0)
					returnValue = r.GetIdentifier(n, d, null);

				return returnValue;
			}

			return GetId(resourceType, name);
		}

		static int GetId([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string memberName)
		{
			// This may legitimately be null in designer scenarios
			if (type == null)
				return 0;

			object value = null;
			var fields = type.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				if (field.Name == memberName)
				{
					value = field.GetValue(type);
					break;
				}
			}

			if (value == null)
			{
				var properties = type.GetProperties();
				for (int i = 0; i < properties.Length; i++)
				{
					var prop = properties[i];
					if (prop.Name == memberName)
					{
						value = prop.GetValue(type);
						break;
					}
				}
			}

			if (value is int result)
				return result;
			return 0;
		}
	}
}
