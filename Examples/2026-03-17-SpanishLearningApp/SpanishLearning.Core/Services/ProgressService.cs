using System.Text.Json;
using Microsoft.JSInterop;
using SpanishLearning.Core.Models;

namespace SpanishLearning.Core.Services;

public class ProgressService(IJSRuntime js)
{
    private const string StorageKey = "spanish-learning-progress";

    public async Task<ProgressRecord> GetProgress()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrEmpty(json))
                return new ProgressRecord();
            return JsonSerializer.Deserialize<ProgressRecord>(json) ?? new ProgressRecord();
        }
        catch (InvalidOperationException)
        {
            // JS interop is unavailable during static prerendering; return empty record.
            return new ProgressRecord();
        }
    }

    public async Task RecordLessonComplete(int lessonId)
    {
        var record = await GetProgress();
        record.LessonsCompleted.Add(lessonId);
        await Save(record);
    }

    public async Task RecordQuizScore(QuizScore score)
    {
        var record = await GetProgress();
        record.QuizScores.Add(score);
        await Save(record);
    }

    private async Task Save(ProgressRecord record)
    {
        var json = JsonSerializer.Serialize(record);
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}
