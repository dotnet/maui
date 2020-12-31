using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	internal class ShellFlyoutItemsManager
	{
		readonly Shell _shell;
		List<List<Element>> _lastGeneratedFlyoutItems;
		public event EventHandler FlyoutItemsChanged;
		IShellController ShellController => _shell;
		ReadOnlyObservableCollectionWithSource<IReadOnlyList<Element>> _flyoutItemsReadonly;

		public IEnumerable FlyoutItems => _flyoutItemsReadonly;
		public ShellFlyoutItemsManager(Shell shell)
		{
			_shell = shell;
			_flyoutItemsReadonly = new ReadOnlyObservableCollectionWithSource<IReadOnlyList<Element>>();
		}


		void SyncFlyoutItemsToReadOnlyCollection()
		{
			var flyoutItems = _flyoutItemsReadonly.List;

			// sync the number of groups
			for (var i = flyoutItems.Count; i < _lastGeneratedFlyoutItems.Count; i++)
				flyoutItems.Add(new ReadOnlyObservableCollectionWithSource<Element>());

			for (var i = _lastGeneratedFlyoutItems.Count; i < flyoutItems.Count; i++)
				flyoutItems.RemoveAt(i);

			for (var i = 0; i < _lastGeneratedFlyoutItems.Count; i++)
			{
				var source = _lastGeneratedFlyoutItems[i];
				var dest = ((ReadOnlyObservableCollectionWithSource<Element>)flyoutItems[i]).List;

				for (var j = dest.Count - 1; j >= 0; j--)
				{
					var item = dest[j];
					if (!source.Contains(item))
						dest.RemoveAt(j);
				}

				for (var j = 0; j < source.Count; j++)
				{
					var item = source[j];
					var destIndex = dest.IndexOf(item);

					if (destIndex == -1)
					{
						if (j < dest.Count)
							dest.Insert(j, item);
						else
							dest.Add(item);
					}
					else
					{
						if (j < dest.Count)
						{
							if(destIndex != j)
								dest.Move(destIndex, j);
						}
						else
							dest.Add(item);
					}
				}
			}
		}

		public void CheckIfFlyoutItemsChanged()
		{
			if (UpdateFlyoutGroupings())
			{
				FlyoutItemsChanged?.Invoke(this, EventArgs.Empty);
				SyncFlyoutItemsToReadOnlyCollection();
			}
		}

		public List<List<Element>> GenerateFlyoutGrouping()
		{
			if (_lastGeneratedFlyoutItems == null)
				UpdateFlyoutGroupings();

			return _lastGeneratedFlyoutItems;
		}

		bool UpdateFlyoutGroupings()
		{
			// The idea here is to create grouping such that the Flyout would
			// render correctly if it renderered each item in the groups in order
			// but put a line between the groups. This is needed because our grouping can
			// actually go 3 layers deep.

			// Ideally this lets us control where lines are drawn in the core code
			// just by changing how we generate these groupings

			var result = new List<List<Element>>();

			var currentGroup = new List<Element>();

			foreach (var shellItem in ShellController.GetItems())
			{
				if (!ShowInFlyoutMenu(shellItem))
					continue;

				if (Routing.IsImplicit(shellItem) || shellItem.FlyoutDisplayOptions == FlyoutDisplayOptions.AsMultipleItems)
				{
					if (shellItem.FlyoutDisplayOptions == FlyoutDisplayOptions.AsMultipleItems)
						IncrementGroup();

					foreach (var shellSection in (shellItem as IShellItemController).GetItems())
					{
						if (!ShowInFlyoutMenu(shellSection))
							continue;

						var shellContents = ((IShellSectionController)shellSection).GetItems();
						if (Routing.IsImplicit(shellSection) || shellSection.FlyoutDisplayOptions == FlyoutDisplayOptions.AsMultipleItems)
						{
							foreach (var shellContent in shellContents)
							{
								if (!ShowInFlyoutMenu(shellContent))
									continue;

								currentGroup.Add(shellContent);
								if (shellContent == shellSection.CurrentItem)
								{
									AddMenuItems(shellContent.MenuItems);
								}
							}

							if (shellSection.FlyoutDisplayOptions == FlyoutDisplayOptions.AsMultipleItems)
								IncrementGroup();
						}
						else
						{
							if (!(shellSection.Parent is TabBar))
							{
								if (Routing.IsImplicit(shellSection) && shellContents.Count == 1)
								{
									if (!ShowInFlyoutMenu(shellContents[0]))
										continue;

									currentGroup.Add(shellContents[0]);
								}
								else
									currentGroup.Add(shellSection);
							}

							// If we have only a single child we will also show the items menu items
							if (shellContents.Count == 1 && shellSection == shellItem.CurrentItem && shellSection.CurrentItem.MenuItems.Count > 0)
							{
								AddMenuItems(shellSection.CurrentItem.MenuItems);
							}
						}
					}

					if (shellItem.FlyoutDisplayOptions == FlyoutDisplayOptions.AsMultipleItems)
						IncrementGroup();
				}
				else
				{
					if (!(shellItem is TabBar))
						currentGroup.Add(shellItem);
				}
			}

			IncrementGroup();

			// If the flyout groupings haven't changed just return
			// the same instance so the caller knows it hasn't changed
			// at a later point this will all get converted to an observable collection
			if (_lastGeneratedFlyoutItems?.Count == result.Count)
			{
				bool hasChanged = false;
				for (var i = 0; i < result.Count && !hasChanged; i++)
				{
					var topLevelNew = result[i];
					var topLevelPrevious = _lastGeneratedFlyoutItems[i];

					if (topLevelNew.Count != topLevelPrevious.Count)
					{
						hasChanged = true;
						break;
					}

					for (var j = 0; j > topLevelNew.Count; j++)
					{
						if (topLevelNew[j] != topLevelPrevious[j])
						{
							hasChanged = true;
							break;
						}
					}

				}

				if (!hasChanged)
					return false;
			}

			_lastGeneratedFlyoutItems = result;
			return true;

			bool ShowInFlyoutMenu(BindableObject bo)
			{
				if (bo is MenuShellItem msi)
					return Shell.GetFlyoutItemIsVisible(msi.MenuItem);

				return Shell.GetFlyoutItemIsVisible(bo);
			}

			void AddMenuItems(MenuItemCollection menuItems)
			{
				foreach (var item in menuItems)
				{
					if (ShowInFlyoutMenu(item))
						currentGroup.Add(item);
				}
			}

			void IncrementGroup()
			{
				if (currentGroup.Count > 0)
				{
					result.Add(currentGroup);
					currentGroup = new List<Element>();
				}
			}
		}

		class ReadOnlyObservableCollectionWithSource<T> : ReadOnlyObservableCollection<T>
		{
			public ReadOnlyObservableCollectionWithSource() : this(new ObservableCollection<T>())
			{
			}

			public ReadOnlyObservableCollectionWithSource(ObservableCollection<T> list) : base(list)
			{
				List = list;
			}

			public ObservableCollection<T> List { get; }
		}
	}
}
