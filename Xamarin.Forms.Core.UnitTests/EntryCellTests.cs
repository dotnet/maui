using System.ComponentModel;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class EntryCellTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[Test]
		public void ChangingHorizontalTextAlignmentFiresXAlignChanged()
		{
			var entryCell = new EntryCell { HorizontalTextAlignment = TextAlignment.Center };

			var xAlignFired = false;
			var horizontalTextAlignmentFired = false;

			entryCell.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "XAlign")
				{
					xAlignFired = true;
				}
				else if (args.PropertyName == EntryCell.HorizontalTextAlignmentProperty.PropertyName)
				{
					horizontalTextAlignmentFired = true;
				}
			};

			entryCell.HorizontalTextAlignment = TextAlignment.End;

			Assert.True(xAlignFired);
			Assert.True(horizontalTextAlignmentFired);
		}

		[Test]
		public void EntryCellXAlignBindingMatchesHorizontalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.Alignment = TextAlignment.Center;

			var entryCellXAlign = new EntryCell() { BindingContext = vm };
			entryCellXAlign.SetBinding(EntryCell.XAlignProperty, new Binding("Alignment"));

			var entryCellHorizontalTextAlignment = new EntryCell() { BindingContext = vm };
			entryCellHorizontalTextAlignment.SetBinding(EntryCell.HorizontalTextAlignmentProperty, new Binding("Alignment"));

			Assert.AreEqual(TextAlignment.Center, entryCellXAlign.XAlign);
			Assert.AreEqual(TextAlignment.Center, entryCellHorizontalTextAlignment.HorizontalTextAlignment);

			vm.Alignment = TextAlignment.End;

			Assert.AreEqual(TextAlignment.End, entryCellXAlign.XAlign);
			Assert.AreEqual(TextAlignment.End, entryCellHorizontalTextAlignment.HorizontalTextAlignment);
		}

		sealed class ViewModel : INotifyPropertyChanged
		{
			TextAlignment alignment;

			public TextAlignment Alignment
			{
				get { return alignment; }
				set
				{
					alignment = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}