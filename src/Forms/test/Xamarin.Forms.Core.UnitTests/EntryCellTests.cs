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
		public void EntryCellXAlignBindingMatchesHorizontalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.Alignment = TextAlignment.Center;

			var entryCellHorizontalTextAlignment = new EntryCell() { BindingContext = vm };
			entryCellHorizontalTextAlignment.SetBinding(EntryCell.HorizontalTextAlignmentProperty, new Binding("Alignment"));

			Assert.AreEqual(TextAlignment.Center, entryCellHorizontalTextAlignment.HorizontalTextAlignment);

			vm.Alignment = TextAlignment.End;

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