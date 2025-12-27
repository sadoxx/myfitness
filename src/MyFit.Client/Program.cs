using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyFit.Client;
using MyFit.Client.Services;
using MudBlazor.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API base address (supports both Docker and local development)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:7001";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Add MudBlazor
builder.Services.AddMudServices();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add custom services
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<StateContainer>();

await builder.Build().RunAsync();
