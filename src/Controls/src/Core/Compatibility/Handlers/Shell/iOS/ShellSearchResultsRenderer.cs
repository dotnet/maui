#nullable disable
using System;
using System.Collections.Specialized;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellSearchResultsRenderer : UITableViewController, IShellSearchResultsRenderer
	{
		#region IShellSearchResultsRenderer

		SearchHandler IShellSearchResultsRenderer.SearchHandler
		{
			get { return SearchHandler; }
			set
			{
				SearchHandler = value;
				OnSearchHandlerSet();
			}
		}

		UIViewController IShellSearchResultsRenderer.ViewController => this;

		#endregion IShellSearchResultsRenderer

		readonly IShellContext _context;
		DataTemplate _defaultTemplate;

		// If data templates were horses, this is a donkey
		DataTemplate DefaultTemplate
		{
			get
			{
				return _defaultTemplate ?? (_defaultTemplate = new DataTemplate(() =>
					{
						var label = new Label();

						if (RuntimeFeature.IsShellSearchResultsRendererDisplayMemberNameSupported)
						{
#pragma warning disable CS0618
#if NET8_0
#pragma warning disable IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
							label.SetBinding(Label.TextProperty, SearchHandler.DisplayMemberName ?? ".");
#if NET8_0
#pragma warning restore IL2026 // FeatureGuardAttribute is not supported on .NET 8
#endif
#pragma warning restore CS0618
						}
						else
						{
#pragma warning disable CS0618
							if (SearchHandler.DisplayMemberName is not null)
							{
								Application.Current?.FindMauiContext()?.CreateLogger<ShellSearchResultsRenderer>()?.LogError(TrimmerConstants.SearchHandlerDisplayMemberNameNotSupportedWarning);
								throw new InvalidOperationException(TrimmerConstants.SearchHandlerDisplayMemberNameNotSupportedWarning);
							}
#pragma warning restore CS0618

							label.SetBinding(Label.TextProperty, static (object o) => o);
						}

						label.HorizontalTextAlignment = TextAlignment.Center;
						label.VerticalTextAlignment = TextAlignment.Center;

						return label;
					}));
			}
		}

		public event EventHandler<object> ItemSelected;

		public ShellSearchResultsRenderer(IShellContext context)
		{
			_context = context;
		}

		protected UITableViewRowAnimation DeleteRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation InsertRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadSectionsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		private ISearchHandlerController SearchController => SearchHandler;
		private SearchHandler SearchHandler { get; set; }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (SearchHandler != null)
				{
					var listProxy = (INotifyCollectionChanged)SearchController.ListProxy;
					if (listProxy != null)
						listProxy.CollectionChanged -= OnProxyCollectionChanged;
					SearchController.ListProxyChanged -= OnListProxyChanged;
				}

				SearchHandler = null;
			}

		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var proxy = SearchController.ListProxy;
			int row = indexPath.Row;
			var context = proxy[row];

			var template = SearchHandler.ItemTemplate;

			if (template == null)
			{
				template = DefaultTemplate;
			}

			var cellId = ((IDataTemplateController)template.SelectDataTemplate(context, _context.Shell)).IdString;

			var cell = (UIContainerCell)tableView.DequeueReusableCell(cellId);

			if (cell == null)
			{
				var view = (View)template.CreateContent(context, _context.Shell);
				view.BindingContext = context;
				view.Parent = _context.Shell;
				cell = new UIContainerCell(cellId, view);
			}
			else
			{
				cell.View.BindingContext = context;
			}

			return cell;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var item = SearchController.ListProxy[indexPath.Row];
			ItemSelected?.Invoke(this, item);
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return 1;
		}

		public override nint RowsInSection(UITableView tableView, nint section)
		{
			if (SearchController.ListProxy == null)
				return 0;
			return SearchController.ListProxy.Count;
		}

		NSIndexPath[] GetPaths(int section, int index, int count)
		{
			var paths = new NSIndexPath[count];
			for (var i = 0; i < paths.Length; i++)
				paths[i] = NSIndexPath.FromRowSection(index + i, section);

			return paths;
		}

		void OnListProxyChanged(object sender, ListProxyChangedEventArgs e)
		{
			if (e.OldList != null)
			{
				((INotifyCollectionChanged)e.OldList).CollectionChanged -= OnProxyCollectionChanged;
			}
			// Full reset
			TableView.ReloadData();

			if (e.NewList != null)
			{
				((INotifyCollectionChanged)e.NewList).CollectionChanged += OnProxyCollectionChanged;
			}
		}

		void OnProxyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			int section = 0;
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:

					if (e.NewStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;

					TableView.BeginUpdates();
					TableView.InsertRows(GetPaths(section, e.NewStartingIndex, e.NewItems.Count), InsertRowsAnimation);
					TableView.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;
					TableView.BeginUpdates();
					TableView.DeleteRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), DeleteRowsAnimation);

					TableView.EndUpdates();
					break;

				case NotifyCollectionChangedAction.Move:
					if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;
					TableView.BeginUpdates();
					for (var i = 0; i < e.OldItems.Count; i++)
					{
						var oldIndex = e.OldStartingIndex;
						var newIndex = e.NewStartingIndex;

						if (e.NewStartingIndex < e.OldStartingIndex)
						{
							oldIndex += i;
							newIndex += i;
						}

						TableView.MoveRow(NSIndexPath.FromRowSection(oldIndex, section), NSIndexPath.FromRowSection(newIndex, section));
					}
					TableView.EndUpdates();
					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex == -1)
						goto case NotifyCollectionChangedAction.Reset;
					TableView.BeginUpdates();
					TableView.ReloadRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), ReloadRowsAnimation);
					TableView.EndUpdates();
					break;

				case NotifyCollectionChangedAction.Reset:
					TableView.ReloadData();
					return;
			}
		}

		void OnSearchHandlerSet()
		{
			SearchController.ListProxyChanged += OnListProxyChanged;
		}
	}
}