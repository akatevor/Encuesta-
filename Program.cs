using Encuesta.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar ConexionMySql como servicio scoped
builder.Services.AddScoped<ConexionMySql>();

// Configurar sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Asegúrate de servir archivos estáticos si los necesitas

app.UseRouting();

// Debe ir antes de UseAuthorization
app.UseSession();

app.UseAuthorization();

// Estas líneas parecen incorrectas - probablemente sobrantes
// app.MapStaticAssets();
// .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();