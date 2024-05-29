using TestResearchProject.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<HelperMethods>();
builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddAuthentication().AddCookie("MySecurity", configure =>
{
    configure.Cookie.Name = "MySecurity";
    //Specifying the path for the login
    configure.LoginPath = "/Login/Login";
    //Specifying the path for the access denied page
    configure.AccessDeniedPath = "/Login/Unauthorized";
    //Configuring the cookie expiry time
    configure.ExpireTimeSpan = TimeSpan.FromSeconds(300);
});

builder.Services.AddAuthorization(option =>
{
    
    //option.AddPolicy("ConfirmedEmployee", policy =>
    //{
    //    policy.RequireClaim("Status", "Confirmed");
    //});
});



//builder.Services.AddAuthentication().AddBearerToken();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");
app.UseSession();
app.Run();
