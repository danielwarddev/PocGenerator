namespace SpanishLearning.Core.Models;

public class ProgressRecord
{
    public HashSet<int> LessonsCompleted { get; set; } = [];
    public List<QuizScore> QuizScores { get; set; } = [];
}
