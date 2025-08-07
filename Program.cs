using TicketingSystem.Data;
using TicketingSystem.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// --- THIS IS THE CRITICAL SECTION ---

// 1. Enforce HTTPS Redirection FIRST.
// This middleware catches any incoming HTTP requests and redirects them to HTTPS
// before they go any further. This is vital.
app.UseHttpsRedirection();

// 2. Serve static files (like your chat.js).
app.UseStaticFiles();

// 3. Set up routing.
app.UseRouting();

// 4. (Optional but good practice) Add Authentication/Authorization here if you have it.
// app.UseAuthentication();
app.UseAuthorization();

// 5. Finally, map the endpoints. The router now knows about both controllers and hubs.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chathub"); // The hub endpoint

// --- END OF CRITICAL SECTION ---

app.Run();