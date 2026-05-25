using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeSite.Data;

var builder = WebApplication.CreateBuilder(args);

// Підключення до бази даних
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// 1. ПІДКЛЮЧАЄМО СИСТЕМУ АВТОРИЗАЦІЇ
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
// 2. ДОДАЄМО ПІДТРИМКУ РАЗОР-СТОРІНОК (для готових вікон логіну)
builder.Services.AddRazorPages(); 

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. ВКЛЮЧАЄМО ПЕРЕВІРКУ КОРИСТУВАЧА
app.UseAuthentication(); // Перевіряє, ХТО зайшов (Логін/Пароль)
app.UseAuthorization();  // Перевіряє, ЩО йому дозволено робити

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Recipes}/{action=Index}/{id?}");

// 4. МАРШРУТИ ДЛЯ СТОРІНОК ВХОДУ ТА РЕЄСТРАЦІЇ
app.MapRazorPages(); 

// АВТОМАТИЧНЕ СТВОРЕННЯ АДМІНА 
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    
    
    var adminEmail = "admin@site.com";
    var adminPass = "Admin_1234!"; 

    // Якщо такого користувача ще немає в базі, створюємо його
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(adminUser, adminPass);
    }
}



app.Run();