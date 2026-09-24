var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Exposed so tests can use WebApplicationFactory<Program>.
public partial class Program;
