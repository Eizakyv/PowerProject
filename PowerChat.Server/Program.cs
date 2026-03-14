using PowerChat.Server;

// Initializes the application builder.
// This prepares essential configurations and environment variables for the server.
var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5257";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Registers SignalR services in the dependency injection container.
// This enables real-time communication capabilities for the chat application.
builder.Services.AddSignalR();

// Finalizes service configuration and builds the application instance.
var app = builder.Build();

// Enforces HTTPS redirection for all incoming requests.
// This ensures that insecure connections are automatically upgraded to secure ones.
//app.UseHttpsRedirection();

// Defines a root endpoint ("/") for health checks.
// This allows for quick verification that the server is operational via a browser.
app.MapGet("/", () => "PowerChat Server is running!");

// Maps the "/chat" route to the PowerHub class.
// All messages sent to this address are processed by the defined chat logic.
app.MapHub<PowerHub>("/chat");

// Executes the application and starts listening for incoming network requests.
// The program remains active to handle client connections.
app.Run();