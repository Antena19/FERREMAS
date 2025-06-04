using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ferremas.Api.Services;
using MySql.Data.MySqlClient;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ferremas API",
        Version = "v1",
        Description = "API para el sistema de Ferremas"
    });

    // Configuración de autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuración de autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Error de autenticación: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validado correctamente");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

// Registrar servicios de base de datos
builder.Services.AddScoped<Ferremas.Api.Data.DatabaseConnection>();
builder.Services.AddTransient<System.Data.IDbConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar servicios de autenticación y autorización
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<Ferremas.Api.Repositories.IUsuarioRepository, Ferremas.Api.Repositories.UsuarioRepository>();

// Registrar servicios de productos
builder.Services.AddScoped<Ferremas.Api.Repositories.IProductoRepository, Ferremas.Api.Repositories.ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

// Registrar servicios de clientes
builder.Services.AddScoped<Ferremas.Api.Repositories.IClienteRepository, Ferremas.Api.Repositories.ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Registrar servicios de pedidos
builder.Services.AddScoped<Ferremas.Api.Repositories.IPedidoRepository, Ferremas.Api.Repositories.PedidoRepository>();
builder.Services.AddScoped<IPedidosService, PedidosService>();

// Registrar servicios de pagos
builder.Services.AddScoped<Ferremas.Api.Repositories.IPagoRepository, Ferremas.Api.Repositories.PagoRepository>();
builder.Services.AddScoped<IPagosService, PagosService>();
builder.Services.AddScoped<MercadoPagoService>();

// Registrar servicios de carrito
builder.Services.AddScoped<ICarritoService, CarritoService>();

// Registrar servicios de roles específicos
builder.Services.AddScoped<IBodegueroService, BodegueroService>();
builder.Services.AddScoped<IVendedorService, VendedorService>();
builder.Services.AddScoped<IContadorService, ContadorService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Servicios de gestión de inventario
builder.Services.AddScoped<IInventarioService, InventarioService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Configuración de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("administrador"));

    options.AddPolicy("RequireVendedorRole", policy =>
        policy.RequireRole("vendedor"));

    // Configurar el mapeo de roles
    options.AddPolicy("RequireRole", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var roleClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            return roleClaim != null && (roleClaim.Value == "administrador" || roleClaim.Value == "vendedor");
        });
    });
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

// Importante: el orden de estos middleware es crucial
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();