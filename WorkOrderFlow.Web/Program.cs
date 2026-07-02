using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using WorkOrderFlow.Web.Data;
using WorkOrderFlow.Web.Services;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<QuotePdfService>();
builder.Services.AddScoped<WorkOrderPdfService>();
builder.Services.AddScoped<WorkOrderWorkflowService>();
builder.Services.AddScoped<InventoryStockService>();
builder.Services.AddScoped<QuoteToWorkOrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    await DemoDataSeeder.SeedAsync(dbContext);
}
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
