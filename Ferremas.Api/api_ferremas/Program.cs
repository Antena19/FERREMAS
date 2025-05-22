using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ferremas.Api.Services;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar servicios de productos
builder.Services.AddScoped<Ferremas.Api.Repositories.IProductoRepository, Ferremas.Api.Repositories.ProductoRepository>();
builder.Services.AddScoped<Ferremas.Api.Services.IProductoService, Ferremas.Api.Services.ProductoService>();

// Registrar servicios clientes
builder.Services.AddScoped<Ferremas.Api.Repositories.IClienteRepository, Ferremas.Api.Repositories.ClienteRepository>();
builder.Services.AddScoped<Ferremas.Api.Services.IClienteService, Ferremas.Api.Services.ClienteService>();

// Registrar servicios de usuarios
builder.Services.AddScoped<Ferremas.Api.Repositories.IUsuarioRepository, Ferremas.Api.Repositories.UsuarioRepository>();

// Registrar servicios de pedidos
builder.Services.AddScoped<Ferremas.Api.Repositories.IPedidoRepository, Ferremas.Api.Repositories.PedidoRepository>();
builder.Services.AddScoped<Ferremas.Api.Services.IPedidosService, Ferremas.Api.Services.PedidosService>();

// Registrar servicios de pagos
builder.Services.AddScoped<Ferremas.Api.Repositories.IPagoRepository, Ferremas.Api.Repositories.PagoRepository>();
builder.Services.AddScoped<Ferremas.Api.Services.IPagosService, Ferremas.Api.Services.PagosService>();
builder.Services.AddScoped<Ferremas.Api.Services.MercadoPagoService>();

// Registrar servicio de carrito
builder.Services.AddScoped<Ferremas.Api.Services.ICarritoService, Ferremas.Api.Services.CarritoService>();

// Registrar servicio de autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrar servicio de base de datos
builder.Services.AddScoped<Ferremas.Api.Data.DatabaseConnection>();

// Registrar IDbConnection para Dapper
builder.Services.AddTransient<System.Data.IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();