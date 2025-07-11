// TicketingSystem/Program.cs
using TicketingSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

// 1. Add MVC services
builder.Services.AddControllersWithViews();

// 2. Register our custom repository for Dependency Injection
// This tells the app: "When a controller asks for ITicketRepository, give it a TicketRepository"
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

// === ADD SIGNALR SERVICES ===
builder.Services.AddSignalR();
var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 3. Set the default route to our new Tickets page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=Index}/{id?}");

// === MAP THE SIGNALR HUB ENDPOINT ===
app.MapHub<TicketingSystem.Hubs.ChatHub>("/chatHub");

app.Run();