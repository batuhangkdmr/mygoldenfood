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
using MyGoldenFood.Hubs;

var builder = WebApplication.CreateBuilder(args);

// **Localization (Çoklu Dil) Servisini Ekle**
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// **Desteklenen Kültürler (Diller)**
var supportedCultures = new[]
{
    new CultureInfo("tr"),  // Türkçe (Varsayılan)
    new CultureInfo("en"),  // İngilizce
    new CultureInfo("de"),  // Almanca
    new CultureInfo("fr"),  // Fransızca
    new CultureInfo("ru"),  // Rusça
    new CultureInfo("ja"),  // Japonca
    new CultureInfo("ko"),  // Korece
    new CultureInfo("ar")   // Arapça
};

// **Localization Middleware Ayarları**
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
};

// **Eğer çerez veya tarayıcı ayarı yoksa, başlangıç dilini zorla**
localizationOptions.ApplyCurrentCultureToResponseHeaders = true;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("tr");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddSingleton(localizationOptions);
builder.Services.AddSingleton<DeepLTranslationService>();

// **Localization Servisini Ekleyelim**
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// **Database Connection**
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// **Authentication**
builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Admin/Index";
        options.AccessDeniedPath = "/Admin/Index";
    });

// **Cloudinary Configuration**
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

// **Cloudinary ve Çeviri Servislerini Ekleyelim**
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<DeepLTranslationService>();

// **Mail Ayarlarını appsettings.json'dan Oku**
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

// **Mail Servisini Ekleyelim**
builder.Services.AddScoped<MailService>();

// **Memory Cache**
builder.Services.AddMemoryCache();
builder.Services.AddScoped<LocalizationCacheService>();
builder.Services.AddScoped<TranslationService>();

// **Rate Limiting**
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

// **SignalR Servisini Ekleyelim**
builder.Services.AddSignalR();

var app = builder.Build();

// **Localization Middleware'i ekleyelim**
app.UseRequestLocalization(localizationOptions);

// **Hata Yönetimi**
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

app.UseRouting(); // SignalR'dan önce çağrılmalı!

// **SignalR Hub'ı Ekleyelim**
app.MapHub<ProductHub>("/productHub");
app.MapHub<FaydalariHub>("/faydalariHub");
app.MapHub<MyGoldenFood.Hubs.TariflerHub>("/tariflerHub");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// **Varsayılan Route**
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
