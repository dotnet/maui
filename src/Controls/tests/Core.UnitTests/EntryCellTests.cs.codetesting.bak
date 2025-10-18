using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class EntryCellTests : BaseTestFixture
	{
		[Fact]
		public void EntryCellXAlignBindingMatchesHorizontalTextAlignmentBinding()
		{
			var vm = new ViewModel();
			vm.Alignment = TextAlignment.Center;

			var entryCellHorizontalTextAlignment = new EntryCell() { BindingContext = vm };
			entryCellHorizontalTextAlignment.SetBinding(EntryCell.HorizontalTextAlignmentProperty, new Binding("Alignment"));

			Assert.Equal(TextAlignment.Center, entryCellHorizontalTextAlignment.HorizontalTextAlignment);

			vm.Alignment = TextAlignment.End;

			Assert.Equal(TextAlignment.End, entryCellHorizontalTextAlignment.HorizontalTextAlignment);
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