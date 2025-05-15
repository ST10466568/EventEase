using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VenueBooking.Data;
using VenueBooking.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
// Register services for role-based authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("BookingSpecialist", policy => policy.RequireRole("Booking Specialist"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});
builder.Services.AddSingleton<BlobStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Initialize roles and seed admin user (within a scope)
using (var scope = app.Services.CreateScope())
{
    await InitializeRoles(scope.ServiceProvider);
}

app.Run();

// Method to initialize roles and seed an admin user
static async Task InitializeRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Admin", "Booking Specialist", "User" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var user = await userManager.FindByEmailAsync("admin@eventease.com");
    if (user == null)
    {
        user = new ApplicationUser { UserName = "admin@eventease.com", Email = "admin@eventease.com" };
        await userManager.CreateAsync(user, "Password123!");
    }
    await userManager.AddToRoleAsync(user, "Admin");

    var user2 = await userManager.FindByEmailAsync("booking@eventease.com");
    if (user2 == null)
    {
        user2 = new ApplicationUser { UserName = "booking@eventease.com", Email = "booking@eventease.com" };
        await userManager.CreateAsync(user2, "Password123!");
    }
    await userManager.AddToRoleAsync(user2, "Booking Specialist");
}
