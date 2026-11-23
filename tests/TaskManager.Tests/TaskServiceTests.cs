using TaskManager.Api;
using Xunit;

namespace TaskManager.Tests;

public sealed class TaskServiceTests
{
    private readonly TaskService _service = new();

    [Fact]
    public void Create_ReturnsTaskWithDefaults()
    {
        var result = _service.Create(new("  Első feladat  ", null, null, null));
        Assert.Null(result.Error);
        Assert.NotNull(result.Task);
        Assert.Equal("Első feladat", result.Task.Title);
        Assert.Equal("todo", result.Task.Status);
        Assert.Equal("medium", result.Task.Priority);
    }

    [Fact]
    public void Create_RejectsEmptyTitle()
    {
        var result = _service.Create(new(" ", null, null, null));
        Assert.Equal("Title is required", result.Error);
        Assert.Empty(_service.List());
    }

    [Fact]
    public void Create_RejectsLongTitle()
    {
        var result = _service.Create(new(new string('A', 121), null, null, null));
        Assert.NotNull(result.Error);
    }

    [Theory]
    [InlineData("invalid", "low")]
    [InlineData("todo", "critical")]
    public void Create_RejectsInvalidValues(string status, string priority)
    {
        var result = _service.Create(new("Sample", null, status, priority));
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Update_PreservesIdAndCreationDate()
    {
        var created = _service.Create(new("Original", "", "todo", "low")).Task!;
        var updated = _service.Update(created.Id, new("New", "desc", "done", "high")).Task!;
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(created.CreatedAt, updated.CreatedAt);
        Assert.Equal("done", updated.Status);
        Assert.Equal("high", updated.Priority);
        Assert.Equal("New", _service.Get(created.Id)!.Title);
    }

    [Fact]
    public void Delete_RemovesTask()
    {
        var created = _service.Create(new("Delete me", null, null, null)).Task!;
        Assert.True(_service.Delete(created.Id));
        Assert.Null(_service.Get(created.Id));
        Assert.False(_service.Delete(created.Id));
    }

    [Fact]
    public void Update_UnknownId_ReturnsNoTask()
    {
        var result = _service.Update(Guid.NewGuid(), new("Test", "", "todo", "low"));
        Assert.Null(result.Task);
        Assert.Null(result.Error);
    }

    [Fact]
    public void List_ContainsAllAddedTasks()
    {
        _service.Create(new("One", null, null, null));
        _service.Create(new("Two", null, null, null));
        Assert.Equal(2, _service.List().Count);
    }
}
