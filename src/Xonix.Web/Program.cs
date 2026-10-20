using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Xonix.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Home>("#app");

await builder.Build().RunAsync();
