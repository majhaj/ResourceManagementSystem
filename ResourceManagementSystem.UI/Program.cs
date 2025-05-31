using Microsoft.AspNetCore.Components.Web;
using ResourceManagementSystem.UI.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5193/");
});

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BackendApi"));

builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<TaskHubService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
