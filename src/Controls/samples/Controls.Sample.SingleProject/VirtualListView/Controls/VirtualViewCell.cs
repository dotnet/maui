using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public class VirtualViewCell : Grid, IPositionInfo
	{
		public void SetPositionInfo(PositionInfo info)
		{
			PositionKind = info.Kind;

			if (info.Kind == PositionKind.Item)
			{
				IsLastItemInSection = info.ItemIndex >= info.ItemsInSection - 1;
				IsNotLastItemInSection = !IsLastItemInSection;
				IsFirstItemInSection = info.ItemIndex == 0;
				IsNotFirstItemInSection = !IsFirstItemInSection;
				ItemIndex = info.ItemIndex;
				SectionIndex = info.SectionIndex;
				IsSelected = info.IsSelected;
			}
			else
			{
				IsLastItemInSection = false;
				IsNotLastItemInSection = false;
				IsFirstItemInSection = false;
				IsNotFirstItemInSection = false;
				ItemIndex = -1;
				SectionIndex = -1;
				IsSelected = false;
			}

			IsItem = info.Kind == PositionKind.Item;
			IsGlobalHeader = info.Kind == PositionKind.Header;
			IsGlobalFooter = info.Kind == PositionKind.Footer;
			IsSectionHeader = info.Kind == PositionKind.SectionHeader;
			IsSectionFooter = info.Kind == PositionKind.SectionFooter;
		}

		public static readonly BindableProperty SectionIndexProperty =
			BindableProperty.Create(nameof(SectionIndex), typeof(int), typeof(VirtualViewCell), -1);

		public int SectionIndex
		{
			get => (int)GetValue(SectionIndexProperty);
			set => SetValue(SectionIndexProperty, value);
		}

		public static readonly BindableProperty ItemIndexProperty =
			BindableProperty.Create(nameof(ItemIndex), typeof(int), typeof(VirtualViewCell), -1);

		public int ItemIndex
		{
			get => (int)GetValue(ItemIndexProperty);
			set => SetValue(ItemIndexProperty, value);
		}

		public static readonly BindableProperty IsGlobalHeaderProperty =
			BindableProperty.Create(nameof(IsGlobalHeader), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsGlobalHeader
		{
			get => (bool)GetValue(IsGlobalHeaderProperty);
			set => SetValue(IsGlobalHeaderProperty, value);
		}


		public static readonly BindableProperty IsGlobalFooterProperty =
			BindableProperty.Create(nameof(IsGlobalFooter), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsGlobalFooter
		{
			get => (bool)GetValue(IsGlobalFooterProperty);
			set => SetValue(IsGlobalFooterProperty, value);
		}

		public static readonly BindableProperty IsSectionHeaderProperty =
			BindableProperty.Create(nameof(IsSectionHeader), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsSectionHeader
		{
			get => (bool)GetValue(IsSectionHeaderProperty);
			set => SetValue(IsSectionHeaderProperty, value);
		}


		public static readonly BindableProperty IsSectionFooterProperty =
			BindableProperty.Create(nameof(IsSectionFooter), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsSectionFooter
		{
			get => (bool)GetValue(IsSectionFooterProperty);
			set => SetValue(IsSectionFooterProperty, value);
		}


		public static readonly BindableProperty IsItemProperty =
			BindableProperty.Create(nameof(IsItem), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsItem
		{
			get => (bool)GetValue(IsItemProperty);
			set => SetValue(IsItemProperty, value);
		}


		public static readonly BindableProperty IsLastItemInSectionProperty =
			BindableProperty.Create(nameof(IsLastItemInSection), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsLastItemInSection
		{
			get => (bool)GetValue(IsLastItemInSectionProperty);
			set => SetValue(IsLastItemInSectionProperty, value);
		}

		public static readonly BindableProperty IsNotLastItemInSectionProperty =
			BindableProperty.Create(nameof(IsNotLastItemInSection), typeof(bool), typeof(VirtualViewCell), true);

		public bool IsNotLastItemInSection
		{
			get => (bool)GetValue(IsNotLastItemInSectionProperty);
			set => SetValue(IsNotLastItemInSectionProperty, value);
		}


		public static readonly BindableProperty IsFirstItemInSectionProperty =
			BindableProperty.Create(nameof(IsFirstItemInSection), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsFirstItemInSection
		{
			get => (bool)GetValue(IsFirstItemInSectionProperty);
			set => SetValue(IsFirstItemInSectionProperty, value);
		}


		public static readonly BindableProperty IsNotFirstItemInSectionProperty =
			BindableProperty.Create(nameof(IsNotFirstItemInSection), typeof(bool), typeof(VirtualViewCell), true);

		public bool IsNotFirstItemInSection
		{
			get => (bool)GetValue(IsNotFirstItemInSectionProperty);
			set => SetValue(IsNotFirstItemInSectionProperty, value);
		}


		public static readonly BindableProperty PositionKindProperty =
			BindableProperty.Create(nameof(PositionKind), typeof(PositionKind), typeof(VirtualViewCell), PositionKind.Item);

		public PositionKind PositionKind
		{
			get => (PositionKind)GetValue(PositionKindProperty);
			set => SetValue(PositionKindProperty, value);
		}

		public static readonly BindableProperty IsSelectedProperty =
			BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(VirtualViewCell), false);

		public bool IsSelected
		{
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public IView CreateView(PositionInfo positionInfo)
		{
			SetPositionInfo(positionInfo);
			return this;
		}
	}
}
