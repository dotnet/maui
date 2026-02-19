using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Views;

public class ChatBubbleTemplateSelector : DataTemplateSelector
{
	public DataTemplate? UserTemplate { get; set; }
	public DataTemplate? AssistantTemplate { get; set; }
	public DataTemplate? ToolCallTemplate { get; set; }
	public DataTemplate? ToolResultTemplate { get; set; }

	protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
	{
		if (item is ChatBubble bubble)
		{
			return bubble.BubbleType switch
			{
				ChatBubbleType.User => UserTemplate,
				ChatBubbleType.Assistant => AssistantTemplate,
				ChatBubbleType.ToolCall => ToolCallTemplate,
				ChatBubbleType.ToolResult => ToolResultTemplate,
				_ => AssistantTemplate
			};
		}
		return AssistantTemplate;
	}
}
