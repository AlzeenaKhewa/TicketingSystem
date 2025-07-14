using TicketingSystem.Data;
using TicketingSystem.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register your repositories for Dependency Injection
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map your controllers and the SignalR hub
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chathub");

app.Run();