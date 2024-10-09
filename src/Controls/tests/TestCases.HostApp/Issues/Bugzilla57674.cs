﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57674, "ListView not honoring INotifyCollectionChanged", PlatformAffected.UWP)]
	public class Bugzilla57674 : TestContentPage
	{
		MyCollection _myCollection;
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			_myCollection = new MyCollection();

			var stackLayout = new StackLayout();
			var button = new Button
			{
				AutomationId = "IssueButton",
				Text = "Add new element to ListView"
			};
			button.Clicked += (object sender, EventArgs e) => _myCollection.AddNewItem();

			stackLayout.Children.Add(button);

			stackLayout.Children.Add(new ListView
			{
				AutomationId = "IssueListView",
				ItemsSource = _myCollection
			});

			Content = stackLayout;
		}
	}

	public class MyCollection : IEnumerable<string>, INotifyCollectionChanged
	{
		readonly List<string> _internalList = new List<string>();
		public MyCollection()
		{
		}

		public IEnumerable<string> GetItems()
		{
			foreach (var item in _internalList)
			{
				yield return item;
			}
		}

		public IEnumerator<string> GetEnumerator()
		{
			return GetItems().GetEnumerator();
		}

		public void AddNewItem()
		{
			int index = _internalList.Count;
			string item = Guid.NewGuid().ToString();
			_internalList.Add(item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
