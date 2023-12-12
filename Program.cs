using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TurboTicketsMVC.Data;
using TurboTicketsMVC.Extensions;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Models.Enums;
using TurboTicketsMVC.Services;
using TurboTicketsMVC.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration) ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//change adddefaultid to addidentity abd add IdentityRole
builder.Services.AddIdentity<TTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                 .AddClaimsPrincipalFactory<TTUserClaimsPrincipalFactory>()
                .AddDefaultUI() // lines added
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

//policy config
//IAuthorization int
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("Admin|PM", policy =>
//    {
//        policy.RequireRole(nameof(TTRoles.Admin), nameof(TTRoles.ProjectManager));
//    });
//});

//Custom service section
builder.Services.AddScoped<ITTCompanyService, TTCompanyService>();
builder.Services.AddScoped<ITTFileService, TTFileService>();
builder.Services.AddScoped<ITTInviteService, TTInviteService>();
builder.Services.AddScoped<ITTNotificationService, TTNotificationService>();
builder.Services.AddScoped<ITTRolesService, TTRolesService>();
builder.Services.AddScoped<ITTProjectService, TTProjectService>();
builder.Services.AddScoped<ITTTicketHistoryService, TTTicketHistoryService>();
builder.Services.AddScoped<ITTTicketService, TTTicketService>();
builder.Services.AddScoped<IEmailSender, EmailService>();


//email config
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddMvc();
var app = builder.Build();
var scope = app.Services.CreateScope();
await DataUtility.ManageDataAsync(scope.ServiceProvider);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");
app.MapRazorPages();

app.Run();
