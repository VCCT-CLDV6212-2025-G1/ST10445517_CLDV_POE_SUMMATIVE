using ClassLibrary.Data;
using ClassLibrary.Services;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllersWithViews();

//Register ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("SqlDbConnection")));

//Register SQL Login Service
builder.Services.AddScoped<SqlLoginService>();

//Register Session Service
builder.Services.AddDistributedMemoryCache(); // Required for session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//Register table storage with configuration
builder.Services.AddSingleton(new AzureTableStorageService(configuration.GetConnectionString("AzureStorage")));



//Register blob service with configuration
builder.Services.AddSingleton<BlobService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new BlobService(configuration.GetConnectionString("AzureStorage"), configuration, httpClientFactory);
});

builder.Services.AddScoped<QueueService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    // Assuming QueueService constructor takes (IConfiguration, IHttpClientFactory)
    return new QueueService(config, httpClientFactory);
});

builder.Services.AddScoped<CustomerFunctionClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return new CustomerFunctionClient(config, httpClientFactory);
});


builder.Services.AddSingleton<FileService>(sp =>
{
    var connectionString = configuration.GetConnectionString("AzureStorage");
    return new FileService(connectionString, "contractshare");
});

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
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
