using System;
using System.Collections.Generic;
using ImageSharingWithSecurity.DAL;
using ImageSharingWithSecurity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

/*
 * Add services to the container.
 */
builder.Services.AddControllersWithViews();

/*
 * Configure cookie policy to allow ADA saved in a cookie.
 */
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

/*
 * Configure logging to go the console (local testing only!).
 */
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


/*
 * Connection string for SQL database; append credentials if present (from user secrets or environment variables).
 */
string dbConnectionString = builder.Configuration["Data:ApplicationDb:ConnectionString"];
if (dbConnectionString == null)
{
    throw new KeyNotFoundException("Missing database connection string in configuration: Data:ApplicationDb:ConnectionString");
}
var connStringBuilder = new SqlConnectionStringBuilder(dbConnectionString);

string database = builder.Configuration["Data:ApplicationDb:Database"];
if (database == null)
{
    throw new KeyNotFoundException("Missing database name in configuration: Data:ApplicationDb:Database");
}
connStringBuilder.InitialCatalog = database;

string dbUser = builder.Configuration["Credentials:ApplicationDb:User"];
if (dbUser == null)
{
    throw new KeyNotFoundException("Missing database username in configuration: Credentials:ApplicationDb:User");
}
connStringBuilder.UserID = dbUser;

string dbPassword = builder.Configuration["Credentials:ApplicationDb:Password"];
if (dbPassword == null)
{
    throw new KeyNotFoundException("Missing database password in configuration: Credentials:ApplicationDb:Password");
}
connStringBuilder.Password = dbPassword;

dbConnectionString = connStringBuilder.ConnectionString;
if (dbConnectionString == null)
{
    throw new KeyNotFoundException("Missing database connection string in configuration: Data:ApplicationDb:ConnectionString");
}

// TODO add database context & enable saving data in the log (not for production use!)
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(dbConnectionString); 
    options.EnableSensitiveDataLogging();
});

// Replacement for database error page
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// TODO add Identity service
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    // app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

/*
 * Everything is configurable.
 */
app.MapDefaultControllerRoute();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}"
// );

/*
 * TODO Seed the database: We need to manually inject the dependencies of the initializer.
 * EF services are scoped to a request, so we must create a temporary scope for its injection.
 * More on dependency injection: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
 * More on DbContext lifetime: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
 */

using (var scope = app.Services.CreateScope()){
    var serviceProvider = scope.ServiceProvider;
    
    var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbInitializer>>();
    
    var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
    
    await new ApplicationDbInitializer(db, logger).SeedDatabase(serviceProvider);
    
}


/*
 * Finally, run the application!
 */

app.Run();