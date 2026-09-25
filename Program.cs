//using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ----------------------------------------------------
        // Database
        // ----------------------------------------------------

        var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // ----------------------------------------------------
        // Identity
        // ----------------------------------------------------

        builder.Services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        // ----------------------------------------------------
        // MVC
        // ----------------------------------------------------

        builder.Services.AddControllersWithViews();
        var app = builder.Build();
        // ----------------------------------------------------
        // Middleware
        // ----------------------------------------------------
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();

        // ----------------------------------------------------
        // Routing
        // ----------------------------------------------------

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // ----------------------------------------------------
        // Seed admin account
        // ----------------------------------------------------

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            await SeedData.InitializeAsync(services);
        }
        app.Run();
    }
}