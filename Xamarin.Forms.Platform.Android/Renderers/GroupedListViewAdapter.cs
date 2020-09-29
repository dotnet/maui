using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Widget;
using AListView = Android.Widget.ListView;

namespace Xamarin.Forms.Platform.Android
{

	internal class GroupedListViewAdapter : ListViewAdapter, ISectionIndexer
	{
		class SectionData
		{
			public int Index { get; set; }
			public int Length { get; set; }
			public int Start { get; set; }
			public int End => Start + Length;
		}
		public GroupedListViewAdapter(Context context, AListView realListView, ListView listView) : base(context, realListView, listView)
		{

		}
		bool sectionDataValid = false;

		SectionData[] Sections;
		Java.Lang.Object[] nativeSections;
		public int GetPositionForSection(int sectionIndex)
		{
			ValidateSectionData();
			return Sections[sectionIndex].Start;
		}

		public int GetSectionForPosition(int position)
		{
			ValidateSectionData();
			foreach (var section in Sections)
			{
				if (section.Start >= position && section.End <= position)
					return section.Index;
			}
			return 0;
		}

		public Java.Lang.Object[] GetSections()
		{
			ValidateSectionData();
			return nativeSections;
		}

		void ValidateSectionData()
		{
			if (sectionDataValid)
				return;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			int count = 0;

			var sectionData = new List<SectionData>();
			for (var i = 0; i < templatedItems.Count; i++)
			{
				var groupCount = templatedItems.GetGroup(i).Count;
				sectionData.Add(new SectionData { Index = i, Length = groupCount, Start = count });
				count += groupCount;
			}
			Sections = sectionData.ToArray();

			var shortNames = templatedItems.ShortNames;
			if (shortNames != null)
			{
				nativeSections = shortNames.Select(x => new Java.Lang.String(x)).ToArray();
			}
			sectionDataValid = true;
		}

		protected override void InvalidateCount()
		{
			base.InvalidateCount();
			sectionDataValid = false;
		}
	}
}