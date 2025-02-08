using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// **Localization (Çoklu Dil) Servisini Ekle**
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// **Desteklenen Kültürler (Diller)**
var supportedCultures = new[]
{
    new CultureInfo("tr"),  // Türkçe (Varsayýlan)
    new CultureInfo("en"),  // Ýngilizce
    new CultureInfo("de"),  // Almanca
    new CultureInfo("fr"),  // Fransýzca
    new CultureInfo("ru"),  // Rusça
    new CultureInfo("ja"),  // Japonca
    new CultureInfo("ko")   // Korece
};

// **Localization Middleware Ayarlarý**
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr"), // ? Türkçeyi varsayýlan yap
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
};

// **Eðer çerez veya tarayýcý ayarý yoksa, baþlangýç dilini zorla**
localizationOptions.ApplyCurrentCultureToResponseHeaders = true;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("tr");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddSingleton(localizationOptions); // ? Localization servisini singleton olarak ekleyelim
builder.Services.AddSingleton<DeepLTranslationService>();

// **Localization Servisini Ekleyelim**
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Database Connection (Using appsettings.json)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Admin/Index";
        options.AccessDeniedPath = "/Admin/Index";
    });

// Cloudinary Configuration (Using appsettings.json)
builder.Services.AddSingleton(sp =>
{
    var cloudinaryConfig = builder.Configuration.GetSection("CloudinarySettings");
    var account = new Account(
        cloudinaryConfig["CloudName"],
        cloudinaryConfig["ApiKey"],
        cloudinaryConfig["ApiSecret"]
    );
    return new Cloudinary(account);
});

// Register CloudinaryService
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<DeepLTranslationService>();


// **Mail Ayarlarýný appsettings.json'dan Oku**
var emailSettings = builder.Configuration.GetSection("EmailSettings");
var smtpServer = emailSettings["SmtpServer"];
var smtpPort = int.Parse(emailSettings["Port"]);
var smtpUsername = emailSettings["Username"];
var smtpPassword = emailSettings["Password"];

builder.Services.AddScoped<SmtpClient>(sp =>
{
    var client = new SmtpClient(smtpServer, smtpPort)
    {
        Credentials = new NetworkCredential(smtpUsername, smtpPassword),
        EnableSsl = true
    };
    return client;
});

// Register MailService
builder.Services.AddScoped<MailService>();
// Memory Cache
builder.Services.AddMemoryCache();
builder.Services.AddScoped<LocalizationCacheService>();
builder.Services.AddScoped<TranslationService>();


// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.Identity?.Name ?? "guest",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // 100 requests per minute
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));
});

var app = builder.Build();

// **Localization Middleware'i ekle**
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
