using System.Text.Json;
using AwesomeAssertions;
using Microsoft.JSInterop;
using NSubstitute;
using SpanishLearning.Core.Models;
using SpanishLearning.Core.Services;

namespace SpanishLearning.Core.UnitTests;

public class ProgressServiceTests
{
    private readonly IJSRuntime _js = Substitute.For<IJSRuntime>();
    private readonly ProgressService _service;

    public ProgressServiceTests()
    {
        _service = new ProgressService(_js);
    }

    [Fact]
    public async Task When_localStorage_Returns_Null_Then_GetProgress_Should_Return_Empty_Record()
    {
        _js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object?[]?>())
            .Returns(new ValueTask<string?>(result: null));

        var result = await _service.GetProgress();

        result.LessonsCompleted.Should().BeEmpty();
        result.QuizScores.Should().BeEmpty();
    }

    [Fact]
    public async Task When_RecordLessonComplete_Is_Called_Then_Lesson_Id_Is_Added()
    {
        var stored = new ProgressRecord();
        var json = JsonSerializer.Serialize(stored);

        _js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object?[]?>())
            .Returns(new ValueTask<string?>(json));

        await _service.RecordLessonComplete(42);

        await _js.Received().InvokeVoidAsync(
            "localStorage.setItem",
            Arg.Is<object?[]?>(args => args != null && args.Length == 2 && ContainsLessonId((string)args[1]!, 42)));
    }

    [Fact]
    public async Task When_RecordQuizScore_Is_Called_Then_Score_Is_Appended()
    {
        var stored = new ProgressRecord();
        var json = JsonSerializer.Serialize(stored);

        _js.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object?[]?>())
            .Returns(new ValueTask<string?>(json));

        var score = new QuizScore(DateTime.UtcNow, 8, 10);
        await _service.RecordQuizScore(score);

        await _js.Received().InvokeVoidAsync(
            "localStorage.setItem",
            Arg.Is<object?[]?>(args => args != null && args.Length == 2 && ContainsQuizScore((string)args[1]!)));
    }

    private static bool ContainsLessonId(string json, int id)
    {
        var record = JsonSerializer.Deserialize<ProgressRecord>(json);
        return record?.LessonsCompleted.Contains(id) == true;
    }

    private static bool ContainsQuizScore(string json)
    {
        var record = JsonSerializer.Deserialize<ProgressRecord>(json);
        return record?.QuizScores.Count > 0;
    }
}
