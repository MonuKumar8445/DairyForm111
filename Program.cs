using BAL;
using DAL.Data.Store;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model.EntityModels;
using Model.ViewModels;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextPool<ApplicationDbContext>(options => options.
UseSqlServer(builder.Configuration.GetConnectionString("Conn"), b => b.MigrationsAssembly("MilkMan")));
 builder.Services.AddIdentity<MilkMan , IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = false;
})
       .AddEntityFrameworkStores<ApplicationDbContext>()
       .AddDefaultTokenProviders();
builder.Services.AddTransient<IMilkMan, MilkManService>();
builder.Services.Configure<SMTPConfigModel>(builder.Configuration.GetSection("SMTPConfig"));

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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
    pattern: "{controller=MilkMan}/{action=Welcome}/{id?}");

app.Run();
