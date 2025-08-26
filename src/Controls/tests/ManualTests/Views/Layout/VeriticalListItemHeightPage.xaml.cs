namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VeriticalListItemHeightPage : ContentPage
	{
		public List<string> Items { get; set; }

		public VeriticalListItemHeightPage()
		{
			InitializeComponent();
			Items = new List<string>
		{
		  "If you're visiting this page, you're likely here because you're searching for a random sentence.",
		  "Sometimes a random word just isn't enough, and that is where the random sentence generator comes into play. By inputting the desired number, you can make a list of as many random sentences as you want or need. Producing random sentences can be helpful in a number of different ways.",
		  "For writers, a random sentence can help them get their creative juices flowing. Since the topic of the sentence is completely unknown, it forces the writer to be creative when the sentence appears. There are a number of different ways a writer can use the random sentence for creativity. The most common way to use the sentence is to begin a story. Another option is to include it somewhere in the story. A much more difficult challenge is to use it to end a story. In any of these cases, it forces the writer to think creatively since they have no idea what sentence will appear from the tool.",
		  "For those writers who have writers' block, this can be an excellent way to take a step to crumbling those walls.",
		  "It can also be successfully used as a daily exercise to get writers to begin writing. Being shown a random sentence and using it to complete a paragraph each day can be an excellent way to begin any writing session.",
		  "By taking the writer away from the subject matter that is causing the block, a random sentence may allow them to see the project they're working on in a different light and perspective. Sometimes all it takes is to get that first sentence down to help break the block.",
		  "It can also be a fun way to surprise others. You might choose to share a random sentence on social media just to see what type of reaction it garners from others. It's an unexpected move that might create more conversation than a typical post or tweet.",
		  "Have several random sentences generated and you'll soon be able to see if they can help with your project."
		};
			BindingContext = this;
		}
	}
}