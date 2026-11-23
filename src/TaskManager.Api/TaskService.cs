namespace TaskManager.Api;

public record TaskRequest(string? Title, string? Description, string? Status, string? Priority);
public record TaskEntry(Guid Id, string Title, string Description, string Status, string Priority, DateTimeOffset CreatedAt);
public record TaskResult(TaskEntry? Task, string? Error = null);

/// <summary>
/// Demonstrációs, memóriabeli adattároló. Nem perzisztens vagy több példány közötti megosztott adatbázis.
/// </summary>
public sealed class TaskService
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, TaskEntry> _tasks = new();

    public IReadOnlyList<TaskEntry> List()
    {
        lock (_gate) return _tasks.Values.OrderByDescending(t => t.CreatedAt).ToArray();
    }

    public TaskEntry? Get(Guid id)
    {
        lock (_gate) return _tasks.GetValueOrDefault(id);
    }

    public TaskResult Create(TaskRequest request)
    {
        var error = Validate(request);
        if (error is not null) return new(null, error);
        var item = new TaskEntry(Guid.NewGuid(), request.Title!.Trim(), (request.Description ?? "").Trim(),
            NormalizeStatus(request.Status), NormalizePriority(request.Priority), DateTimeOffset.UtcNow);
        lock (_gate) _tasks[item.Id] = item;
        return new(item);
    }

    public TaskResult Update(Guid id, TaskRequest request)
    {
        var error = Validate(request);
        if (error is not null) return new(null, error);
        lock (_gate)
        {
            if (!_tasks.TryGetValue(id, out var existing)) return new(null);
            var updated = existing with
            {
                Title = request.Title!.Trim(),
                Description = (request.Description ?? "").Trim(),
                Status = NormalizeStatus(request.Status),
                Priority = NormalizePriority(request.Priority)
            };
            _tasks[id] = updated;
            return new(updated);
        }
    }

    public bool Delete(Guid id)
    {
        lock (_gate) return _tasks.Remove(id);
    }

    private static string NormalizeStatus(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "todo" : value.Trim().ToLowerInvariant();

    private static string NormalizePriority(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "medium" : value.Trim().ToLowerInvariant();

    private static string? Validate(TaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return "Title is required";
        if (request.Title.Trim().Length > 120) return "Title must be at most 120 characters";
        if ((request.Description ?? "").Length > 500) return "Description must be at most 500 characters";
        if (NormalizeStatus(request.Status) is not ("todo" or "in-progress" or "done"))
            return "Invalid status";
        if (NormalizePriority(request.Priority) is not ("low" or "medium" or "high"))
            return "Invalid priority";
        return null;
    }
}
