namespace AllTheLists.Models.Learning;

public class Lesson
{
    public int LessonNumber { get; set; }
    public string Icon { get; set; }
    public string Title { get; set; } = "Hello & Thank you";
    public string SubTitle { get; set; } = "안녕하세요 & 감사합니다";
    public string Type { get; set; } = "Expression";
    public bool IsCompleted { get; set; } = false; // really this user activity and would normally be tracked in another class
    public string UserGrade { get; set; } = "A"; // really this user activity and would normally be tracked in another class
}
