using ExamLeadPortal.Repositories;
using ExamLeadPortal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IRawLeadRepository, RawLeadRepository>();
builder.Services.AddScoped<IAcceptedLeadRepository, CsvAcceptedLeadRepository>();

builder.Services.AddScoped<ILeadQueryService, LeadQueryService>();
builder.Services.AddScoped<ILeadAcceptanceService, LeadAcceptanceService>();

builder.Services.AddScoped<ILeadCorrectionRepository, JsonLeadCorrectionRepository>();
builder.Services.AddScoped<ILeadCorrectionService, LeadCorrectionService>();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
