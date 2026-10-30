using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Gtk.Hosting
{
	/// <summary>
	/// Helpers to patch the stock static property mappers in place with a GTK branch.
	/// </summary>
	/// <remarks>
	/// The GTK branch runs only when the handler's platform view is the expected GTK widget; in every other case (unit tests,
	/// another backend) the previous mapping is called, so patching is invisible outside this backend.
	/// </remarks>
	public static class GtkMapperExtensions
	{
		static readonly object s_gate = new();
		static readonly List<Patch> s_patches = new();

		sealed class Patch
		{
			public Patch(Func<Delegate?> current, Action install)
			{
				Current = current;
				Install = install;
			}

			public Func<Delegate?> Current { get; }

			public Action Install { get; }

			public Delegate? Installed { get; set; }
		}

		static void Register(Func<Delegate?> current, Action install)
		{
			var patch = new Patch(current, install);

			lock (s_gate)
			{
				install();
				patch.Installed = current();
				s_patches.Add(patch);
			}
		}

		/// <summary>
		/// Re-installs every patch whose mapping has since been replaced. Controls re-maps the static mappers each time an app is
		/// built (<c>UseMauiApp</c>), which shadows a GTK branch installed earlier; <c>UseMauiGtkCore</c> calls this after
		/// <c>UseMauiApp</c> so the GTK branches wrap the current mappings again.
		/// </summary>
		public static void ReapplyShadowedPatches()
		{
			lock (s_gate)
			{
				foreach (var patch in s_patches)
				{
					if (ReferenceEquals(patch.Current(), patch.Installed))
						continue;

					patch.Install();
					patch.Installed = patch.Current();
				}
			}
		}

		/// <summary>Patches <paramref name="key"/> so <paramref name="action"/> runs when the platform view is a <typeparamref name="TWidget"/>.</summary>
		/// <typeparam name="TVirtualView">The virtual view type of the mapper.</typeparam>
		/// <typeparam name="THandler">The handler interface of the mapper.</typeparam>
		/// <typeparam name="TWidget">The expected GTK widget type.</typeparam>
		/// <param name="mapper">The stock mapper to patch.</param>
		/// <param name="key">The property name.</param>
		/// <param name="action">The GTK implementation.</param>
		public static void MapGtk<TVirtualView, THandler, TWidget>(
			this IPropertyMapper<TVirtualView, THandler> mapper,
			string key,
			Action<THandler, TVirtualView, TWidget> action)
			where TVirtualView : IElement
			where THandler : IElementHandler
			where TWidget : class
		{
			Register(
				() => mapper.GetProperty(key),
				() => mapper.ModifyMapping(key, (handler, view, previous) =>
				{
					if (handler?.PlatformView is TWidget widget)
						action(handler, view, widget);
					else
						previous?.Invoke(handler!, view);
				}));
		}

		/// <summary>Patches <paramref name="key"/> of a command mapper so <paramref name="action"/> runs when the platform view is a <typeparamref name="TWidget"/>.</summary>
		/// <typeparam name="TVirtualView">The virtual view type of the mapper.</typeparam>
		/// <typeparam name="THandler">The handler interface of the mapper.</typeparam>
		/// <typeparam name="TWidget">The expected GTK widget type.</typeparam>
		/// <param name="mapper">The stock command mapper to patch.</param>
		/// <param name="key">The command name.</param>
		/// <param name="action">The GTK implementation.</param>
		public static void MapGtk<TVirtualView, THandler, TWidget>(
			this CommandMapper<TVirtualView, THandler> mapper,
			string key,
			Action<THandler, TVirtualView, object?, TWidget> action)
			where TVirtualView : IElement
			where THandler : IElementHandler
			where TWidget : class
		{
			Register(
				() => mapper.GetCommand(key),
				() =>
				{
					var previous = mapper.GetCommand(key);

					mapper.Add(key, (handler, view, args) =>
					{
						if (handler?.PlatformView is TWidget widget)
							action(handler, view, args, widget);
						else
							previous?.Invoke(handler!, view, args);
					});
				});
		}
	}
}
