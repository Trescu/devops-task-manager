using TaskManager.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TaskService>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", app = "TaskManager", version = "1.0.0" }));

app.MapGet("/api/tasks", (TaskService service) => Results.Ok(service.List()));

app.MapGet("/api/tasks/{id:guid}", (Guid id, TaskService service) =>
    service.Get(id) is { } task ? Results.Ok(task) : Results.NotFound(new { error = "Task not found" }));

app.MapPost("/api/tasks", (TaskRequest request, TaskService service) =>
{
    var result = service.Create(request);
    return result.Task is { } task
        ? Results.Created($"/api/tasks/{task.Id}", task)
        : Results.BadRequest(new { error = result.Error });
});

app.MapPut("/api/tasks/{id:guid}", (Guid id, TaskRequest request, TaskService service) =>
{
    var result = service.Update(id, request);
    if (result.Error is not null) return Results.BadRequest(new { error = result.Error });
    return result.Task is { } task ? Results.Ok(task) : Results.NotFound(new { error = "Task not found" });
});

app.MapDelete("/api/tasks/{id:guid}", (Guid id, TaskService service) =>
    service.Delete(id) ? Results.NoContent() : Results.NotFound(new { error = "Task not found" }));

app.Run();
