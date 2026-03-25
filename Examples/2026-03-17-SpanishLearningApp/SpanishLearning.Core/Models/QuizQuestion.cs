namespace SpanishLearning.Core.Models;

public record QuizQuestion(int Id, string Prompt, string[] Options, int CorrectIndex, string Explanation);
