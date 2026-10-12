using WebEmpresa.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;
});

// HttpClient para consumir la API
builder.Services.AddScoped<HttpClient>(sp =>
{
    return new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7211/")
    };
});

var app = builder.Build();

// --- FORZAR PUERTOS ---
app.Urls.Add("http://localhost:5104");
app.Urls.Add("https://localhost:7235");

app.UseCookiePolicy();

// =========================================================
// --- CORRECCIONES DE SEGURIDAD (AL INICIO DEL PIPELINE) ---
// =========================================================

// 1. Ocultar el header "Server"
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("Server");
    await next();
});

// 2. Cabeceras de seguridad globales obligatorias
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";

    // Control de Caché estricto para evitar que ZAP detecte almacenamiento en caché
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    // Content Security Policy (CSP)
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self' wss: https:; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'self';";

    await next();
});

// =========================================================
// --- FIN DE CORRECCIONES DE SEGURIDAD ---
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();