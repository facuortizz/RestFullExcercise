using Microsoft.EntityFrameworkCore;
using RestApiService.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar SQLite con EF Core (crea el archivo inventory.db)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=inventory.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuración de Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API V1");
    c.RoutePrefix = string.Empty; // Carga Swagger en la raíz
});

// Asegurar que la base de datos y las tablas se creen al arrancar (sin precargar datos)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthorization();
app.MapControllers();

app.Run();