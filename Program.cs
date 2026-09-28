using WebEmpresa.Components;

var builder = WebApplication.CreateBuilder(args);

// Blazor Web App + Interactive Server
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient para consumir la API
builder.Services.AddScoped<HttpClient>(sp =>
{
    return new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7211/")
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();