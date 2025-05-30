using Ferremas.Api.Modelos;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Ferremas.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly string _connectionString;
        private readonly IAuthService _authService;

        public AdminService(IConfiguration configuration, IAuthService authService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _authService = authService;
        }

        // Gestión de usuarios por rol
        public async Task<IEnumerable<Usuario>> GetAllUsuarios()
        {
            var usuarios = new List<Usuario>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("SELECT * FROM usuarios", connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(new Usuario
                    {
                        Id = reader.GetInt32("id"),
                        Nombre = reader.GetString("nombre"),
                        Apellido = reader.GetString("apellido"),
                        Email = reader.GetString("email"),
                        Rut = reader.GetString("rut"),
                        Telefono = reader.GetString("telefono"),
                        Rol = reader.GetString("rol"),
                        FechaRegistro = reader.GetDateTime("fecha_registro"),
                        UltimoAcceso = reader.GetDateTime("ultimo_acceso"),
                        Activo = reader.GetBoolean("activo")
                    });
                }
            }
            return usuarios;
        }

        public async Task<IEnumerable<Cliente>> GetAllClientes()
        {
            var clientes = new List<Cliente>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT c.*, d.* 
                    FROM clientes c 
                    LEFT JOIN direcciones d ON c.id = d.cliente_id", connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var cliente = new Cliente
                    {
                        Id = reader.GetInt32("id"),
                        Nombre = reader.GetString("nombre"),
                        Apellido = reader.GetString("apellido"),
                        Rut = reader.GetString("rut"),
                        CorreoElectronico = reader.GetString("correo_electronico"),
                        Telefono = reader.GetString("telefono"),
                        FechaRegistro = reader.GetDateTime("fecha_registro"),
                        TipoCliente = reader.GetString("tipo_cliente"),
                        Estado = reader.GetString("estado"),
                        Newsletter = reader.GetBoolean("newsletter"),
                        UltimaCompra = reader.IsDBNull(reader.GetOrdinal("ultima_compra")) ? null : reader.GetDateTime("ultima_compra"),
                        TotalCompras = reader.GetDecimal("total_compras"),
                        NumeroCompras = reader.GetInt32("numero_compras"),
                        Direcciones = new List<Direccion>()
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("direccion_id")))
                    {
                        cliente.Direcciones.Add(new Direccion
                        {
                            Id = reader.GetInt32("direccion_id"),
                            UsuarioId = cliente.Id,
                            Calle = reader.GetString("calle"),
                            Numero = reader.GetString("numero"),
                            Comuna = reader.GetString("comuna"),
                            Region = reader.GetString("region"),
                            CodigoPostal = reader.GetString("codigo_postal"),
                            EsPrincipal = reader.GetBoolean("es_principal")
                        });
                    }

                    clientes.Add(cliente);
                }
            }
            return clientes;
        }

        public async Task<IEnumerable<Vendedor>> GetAllVendedores()
        {
            var vendedores = new List<Vendedor>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT v.*, u.*, s.* 
                    FROM vendedores v 
                    INNER JOIN usuarios u ON v.usuario_id = u.id
                    INNER JOIN sucursales s ON v.sucursal_id = s.id", connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    vendedores.Add(new Vendedor
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        SucursalId = reader.GetInt32("sucursal_id"),
                        Usuario = new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Email = reader.GetString("email"),
                            Rut = reader.GetString("rut"),
                            Telefono = reader.GetString("telefono"),
                            Rol = reader.GetString("rol")
                        },
                        Sucursal = new Sucursal
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Direccion = reader.GetString("direccion")
                        }
                    });
                }
            }
            return vendedores;
        }

        public async Task<IEnumerable<Bodeguero>> GetAllBodegueros()
        {
            var bodegueros = new List<Bodeguero>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT b.*, u.*, s.* 
                    FROM bodegueros b 
                    INNER JOIN usuarios u ON b.usuario_id = u.id
                    INNER JOIN sucursales s ON b.sucursal_id = s.id", connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    bodegueros.Add(new Bodeguero
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        SucursalId = reader.GetInt32("sucursal_id"),
                        Usuario = new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Email = reader.GetString("email"),
                            Rut = reader.GetString("rut"),
                            Telefono = reader.GetString("telefono"),
                            Rol = reader.GetString("rol")
                        },
                        Sucursal = new Sucursal
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Direccion = reader.GetString("direccion")
                        }
                    });
                }
            }
            return bodegueros;
        }

        public async Task<IEnumerable<Contador>> GetAllContadores()
        {
            var contadores = new List<Contador>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT c.*, u.* 
                    FROM contadores c 
                    INNER JOIN usuarios u ON c.usuario_id = u.id", connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    contadores.Add(new Contador
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        Usuario = new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Email = reader.GetString("email"),
                            Rut = reader.GetString("rut"),
                            Telefono = reader.GetString("telefono"),
                            Rol = reader.GetString("rol")
                        }
                    });
                }
            }
            return contadores;
        }

        // Obtener usuario específico
        public async Task<Usuario> GetUsuarioById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("SELECT * FROM usuarios WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Usuario
                    {
                        Id = reader.GetInt32("id"),
                        Nombre = reader.GetString("nombre"),
                        Apellido = reader.GetString("apellido"),
                        Email = reader.GetString("email"),
                        Rut = reader.GetString("rut"),
                        Telefono = reader.GetString("telefono"),
                        Rol = reader.GetString("rol"),
                        FechaRegistro = reader.GetDateTime("fecha_registro"),
                        UltimoAcceso = reader.GetDateTime("ultimo_acceso"),
                        Activo = reader.GetBoolean("activo")
                    };
                }
            }
            return null;
        }

        public async Task<Cliente> GetClienteById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    SELECT c.*, d.* 
                    FROM clientes c 
                    LEFT JOIN direcciones d ON c.id = d.cliente_id 
                    WHERE c.id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var cliente = new Cliente
                    {
                        Id = reader.GetInt32("id"),
                        Nombre = reader.GetString("nombre"),
                        Apellido = reader.GetString("apellido"),
                        Rut = reader.GetString("rut"),
                        CorreoElectronico = reader.GetString("correo_electronico"),
                        Telefono = reader.GetString("telefono"),
                        FechaRegistro = reader.GetDateTime("fecha_registro"),
                        TipoCliente = reader.GetString("tipo_cliente"),
                        Estado = reader.GetString("estado"),
                        Newsletter = reader.GetBoolean("newsletter"),
                        UltimaCompra = reader.IsDBNull(reader.GetOrdinal("ultima_compra")) ? null : reader.GetDateTime("ultima_compra"),
                        TotalCompras = reader.GetDecimal("total_compras"),
                        NumeroCompras = reader.GetInt32("numero_compras"),
                        Direcciones = new List<Direccion>()
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("direccion_id")))
                    {
                        cliente.Direcciones.Add(new Direccion
                        {
                            Id = reader.GetInt32("direccion_id"),
                            UsuarioId = cliente.Id,
                            Calle = reader.GetString("calle"),
                            Numero = reader.GetString("numero"),
                            Comuna = reader.GetString("comuna"),
                            Region = reader.GetString("region"),
                            CodigoPostal = reader.GetString("codigo_postal"),
                            EsPrincipal = reader.GetBoolean("es_principal")
                        });
                    }

                    return cliente;
                }
            }
            return null;
        }

        public async Task<Vendedor> GetVendedorById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("SELECT * FROM vendedores WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Vendedor
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        SucursalId = reader.GetInt32("sucursal_id"),
                        Usuario = new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Email = reader.GetString("email"),
                            Rut = reader.GetString("rut"),
                            Telefono = reader.GetString("telefono"),
                            Rol = reader.GetString("rol")
                        },
                        Sucursal = new Sucursal
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Direccion = reader.GetString("direccion")
                        }
                    };
                }
            }
            return null;
        }

        public async Task<Bodeguero> GetBodegueroById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("SELECT * FROM bodegueros WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Bodeguero
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        SucursalId = reader.GetInt32("sucursal_id"),
                        Usuario = new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Email = reader.GetString("email"),
                            Rut = reader.GetString("rut"),
                            Telefono = reader.GetString("telefono"),
                            Rol = reader.GetString("rol")
                        },
                        Sucursal = new Sucursal
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Direccion = reader.GetString("direccion")
                        }
                    };
                }
            }
            return null;
        }

        public async Task<Contador> GetContadorById(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("SELECT * FROM contadores WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Contador
                    {
                        Id = reader.GetInt32("id"),
                        UsuarioId = reader.GetInt32("usuario_id"),
                        Usuario = new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Email = reader.GetString("email"),
                            Rut = reader.GetString("rut"),
                            Telefono = reader.GetString("telefono"),
                            Rol = reader.GetString("rol")
                        }
                    };
                }
            }
            return null;
        }

        // Crear usuarios
        public async Task<Usuario> CreateUsuario(Usuario usuario, string rol)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Hash de la contraseña
                    usuario.Password = HashPassword(usuario.Password);

                    // Insertar usuario
                    using var command = new MySqlCommand(@"
                        INSERT INTO usuarios (nombre, apellido, email, password, rut, telefono, rol, fecha_registro, ultimo_acceso, activo)
                        VALUES (@nombre, @apellido, @email, @password, @rut, @telefono, @rol, NOW(), NOW(), 1);
                        SELECT LAST_INSERT_ID();", connection, transaction);

                    command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    command.Parameters.AddWithValue("@apellido", usuario.Apellido);
                    command.Parameters.AddWithValue("@email", usuario.Email);
                    command.Parameters.AddWithValue("@password", usuario.Password);
                    command.Parameters.AddWithValue("@rut", usuario.Rut);
                    command.Parameters.AddWithValue("@telefono", usuario.Telefono);
                    command.Parameters.AddWithValue("@rol", rol);

                    usuario.Id = Convert.ToInt32(await command.ExecuteScalarAsync());

                    // Crear registro en la tabla específica según el rol
                    switch (rol.ToLower())
                    {
                        case "contador":
                            using (var contadorCommand = new MySqlCommand(@"
                                INSERT INTO contadores (usuario_id, sucursal_id)
                                VALUES (@usuarioId, 1);", connection, transaction))
                            {
                                contadorCommand.Parameters.AddWithValue("@usuarioId", usuario.Id);
                                await contadorCommand.ExecuteNonQueryAsync();
                            }
                            break;

                        case "vendedor":
                            using (var vendedorCommand = new MySqlCommand(@"
                                INSERT INTO vendedores (usuario_id, sucursal_id)
                                VALUES (@usuarioId, 1);", connection, transaction))
                            {
                                vendedorCommand.Parameters.AddWithValue("@usuarioId", usuario.Id);
                                await vendedorCommand.ExecuteNonQueryAsync();
                            }
                            break;

                        case "bodeguero":
                            using (var bodegueroCommand = new MySqlCommand(@"
                                INSERT INTO bodegueros (usuario_id, sucursal_id)
                                VALUES (@usuarioId, 1);", connection, transaction))
                            {
                                bodegueroCommand.Parameters.AddWithValue("@usuarioId", usuario.Id);
                                await bodegueroCommand.ExecuteNonQueryAsync();
                            }
                            break;
                    }

                    await transaction.CommitAsync();
                    return usuario;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<Cliente> CreateCliente(Cliente cliente)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    using var command = new MySqlCommand(@"
                        INSERT INTO clientes (
                            nombre, apellido, rut, correo_electronico, telefono, 
                            fecha_registro, tipo_cliente, estado, newsletter
                        ) VALUES (
                            @nombre, @apellido, @rut, @correoElectronico, @telefono,
                            NOW(), @tipoCliente, 'activo', @newsletter
                        );
                        SELECT LAST_INSERT_ID();", connection, transaction);

                    command.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("@apellido", cliente.Apellido);
                    command.Parameters.AddWithValue("@rut", cliente.Rut);
                    command.Parameters.AddWithValue("@correoElectronico", cliente.CorreoElectronico);
                    command.Parameters.AddWithValue("@telefono", cliente.Telefono);
                    command.Parameters.AddWithValue("@tipoCliente", cliente.TipoCliente);
                    command.Parameters.AddWithValue("@newsletter", cliente.Newsletter);

                    cliente.Id = Convert.ToInt32(await command.ExecuteScalarAsync());

                    if (cliente.Direcciones != null && cliente.Direcciones.Any())
                    {
                        foreach (var direccion in cliente.Direcciones)
                        {
                            using var direccionCommand = new MySqlCommand(@"
                                INSERT INTO direcciones (
                                    cliente_id, calle, numero, comuna, region, codigo_postal, es_principal
                                ) VALUES (
                                    @clienteId, @calle, @numero, @comuna, @region, @codigoPostal, @esPrincipal
                                );", connection, transaction);

                            direccionCommand.Parameters.AddWithValue("@clienteId", cliente.Id);
                            direccionCommand.Parameters.AddWithValue("@calle", direccion.Calle);
                            direccionCommand.Parameters.AddWithValue("@numero", direccion.Numero);
                            direccionCommand.Parameters.AddWithValue("@comuna", direccion.Comuna);
                            direccionCommand.Parameters.AddWithValue("@region", direccion.Region);
                            direccionCommand.Parameters.AddWithValue("@codigoPostal", direccion.CodigoPostal);
                            direccionCommand.Parameters.AddWithValue("@esPrincipal", direccion.EsPrincipal);

                            await direccionCommand.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return cliente;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Vendedor> CreateVendedor(Vendedor vendedor)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    INSERT INTO usuarios (nombre, apellido, email, password, rut, telefono, rol, fecha_registro, ultimo_acceso, activo)
                    VALUES (@nombre, @apellido, @email, @password, @rut, @telefono, @rol, NOW(), NOW(), 1);
                    SELECT LAST_INSERT_ID();", connection);

                command.Parameters.AddWithValue("@nombre", vendedor.Usuario.Nombre);
                command.Parameters.AddWithValue("@apellido", vendedor.Usuario.Apellido);
                command.Parameters.AddWithValue("@email", vendedor.Usuario.Email);
                command.Parameters.AddWithValue("@password", vendedor.Usuario.Password);
                command.Parameters.AddWithValue("@rut", vendedor.Usuario.Rut);
                command.Parameters.AddWithValue("@telefono", vendedor.Usuario.Telefono);
                command.Parameters.AddWithValue("@rol", "Vendedor");

                vendedor.Usuario.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                vendedor.UsuarioId = vendedor.Usuario.Id;

                using var insertVendedorCommand = new MySqlCommand(@"
                    INSERT INTO vendedores (usuario_id, sucursal_id)
                    VALUES (@usuario_id, @sucursal_id);
                    SELECT LAST_INSERT_ID();", connection);

                insertVendedorCommand.Parameters.AddWithValue("@usuario_id", vendedor.Usuario.Id);
                insertVendedorCommand.Parameters.AddWithValue("@sucursal_id", vendedor.SucursalId);

                vendedor.Id = Convert.ToInt32(await insertVendedorCommand.ExecuteScalarAsync());

                return vendedor;
            }
        }

        public async Task<Bodeguero> CreateBodeguero(Bodeguero bodeguero)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    INSERT INTO usuarios (nombre, apellido, email, password, rut, telefono, rol, fecha_registro, ultimo_acceso, activo)
                    VALUES (@nombre, @apellido, @email, @password, @rut, @telefono, @rol, NOW(), NOW(), 1);
                    SELECT LAST_INSERT_ID();", connection);

                command.Parameters.AddWithValue("@nombre", bodeguero.Usuario.Nombre);
                command.Parameters.AddWithValue("@apellido", bodeguero.Usuario.Apellido);
                command.Parameters.AddWithValue("@email", bodeguero.Usuario.Email);
                command.Parameters.AddWithValue("@password", bodeguero.Usuario.Password);
                command.Parameters.AddWithValue("@rut", bodeguero.Usuario.Rut);
                command.Parameters.AddWithValue("@telefono", bodeguero.Usuario.Telefono);
                command.Parameters.AddWithValue("@rol", "Bodeguero");

                bodeguero.Usuario.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                bodeguero.UsuarioId = bodeguero.Usuario.Id;

                using var insertBodegueroCommand = new MySqlCommand(@"
                    INSERT INTO bodegueros (usuario_id, sucursal_id)
                    VALUES (@usuario_id, @sucursal_id);
                    SELECT LAST_INSERT_ID();", connection);

                insertBodegueroCommand.Parameters.AddWithValue("@usuario_id", bodeguero.Usuario.Id);
                insertBodegueroCommand.Parameters.AddWithValue("@sucursal_id", bodeguero.SucursalId);

                bodeguero.Id = Convert.ToInt32(await insertBodegueroCommand.ExecuteScalarAsync());

                return bodeguero;
            }
        }

        public async Task<Contador> CreateContador(Contador contador)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    INSERT INTO usuarios (nombre, apellido, email, password, rut, telefono, rol, fecha_registro, ultimo_acceso, activo)
                    VALUES (@nombre, @apellido, @email, @password, @rut, @telefono, @rol, NOW(), NOW(), 1);
                    SELECT LAST_INSERT_ID();", connection);

                command.Parameters.AddWithValue("@nombre", contador.Usuario.Nombre);
                command.Parameters.AddWithValue("@apellido", contador.Usuario.Apellido);
                command.Parameters.AddWithValue("@email", contador.Usuario.Email);
                command.Parameters.AddWithValue("@password", contador.Usuario.Password);
                command.Parameters.AddWithValue("@rut", contador.Usuario.Rut);
                command.Parameters.AddWithValue("@telefono", contador.Usuario.Telefono);
                command.Parameters.AddWithValue("@rol", "Contador");

                contador.Usuario.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                contador.UsuarioId = contador.Usuario.Id;

                using var insertContadorCommand = new MySqlCommand(@"
                    INSERT INTO contadores (usuario_id)
                    VALUES (@usuario_id);
                    SELECT LAST_INSERT_ID();", connection);

                insertContadorCommand.Parameters.AddWithValue("@usuario_id", contador.Usuario.Id);

                contador.Id = Convert.ToInt32(await insertContadorCommand.ExecuteScalarAsync());

                return contador;
            }
        }

        // Actualizar usuarios
        public async Task<Usuario> UpdateUsuario(Usuario usuario)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE usuarios 
                    SET nombre = @nombre, apellido = @apellido, email = @email, 
                        rut = @rut, telefono = @telefono
                    WHERE id = @id", connection);

                command.Parameters.AddWithValue("@id", usuario.Id);
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@apellido", usuario.Apellido);
                command.Parameters.AddWithValue("@email", usuario.Email);
                command.Parameters.AddWithValue("@rut", usuario.Rut);
                command.Parameters.AddWithValue("@telefono", usuario.Telefono);

                await command.ExecuteNonQueryAsync();
                return usuario;
            }
        }

        public async Task<Cliente> UpdateCliente(Cliente cliente)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    using var command = new MySqlCommand(@"
                        UPDATE clientes 
                        SET nombre = @nombre, 
                            apellido = @apellido, 
                            correo_electronico = @correoElectronico,
                            telefono = @telefono, 
                            tipo_cliente = @tipoCliente, 
                            estado = @estado, 
                            newsletter = @newsletter
                        WHERE id = @id", connection, transaction);

                    command.Parameters.AddWithValue("@id", cliente.Id);
                    command.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("@apellido", cliente.Apellido);
                    command.Parameters.AddWithValue("@correoElectronico", cliente.CorreoElectronico);
                    command.Parameters.AddWithValue("@telefono", cliente.Telefono);
                    command.Parameters.AddWithValue("@tipoCliente", cliente.TipoCliente);
                    command.Parameters.AddWithValue("@estado", cliente.Estado);
                    command.Parameters.AddWithValue("@newsletter", cliente.Newsletter);

                    await command.ExecuteNonQueryAsync();

                    if (cliente.Direcciones != null && cliente.Direcciones.Any())
                    {
                        // Primero eliminamos las direcciones existentes
                        using var deleteCommand = new MySqlCommand("DELETE FROM direcciones WHERE cliente_id = @clienteId", connection, transaction);
                        deleteCommand.Parameters.AddWithValue("@clienteId", cliente.Id);
                        await deleteCommand.ExecuteNonQueryAsync();

                        // Luego insertamos las nuevas
                        foreach (var direccion in cliente.Direcciones)
                        {
                            using var direccionCommand = new MySqlCommand(@"
                                INSERT INTO direcciones (
                                    cliente_id, calle, numero, comuna, region, codigo_postal, es_principal
                                ) VALUES (
                                    @clienteId, @calle, @numero, @comuna, @region, @codigoPostal, @esPrincipal
                                );", connection, transaction);

                            direccionCommand.Parameters.AddWithValue("@clienteId", cliente.Id);
                            direccionCommand.Parameters.AddWithValue("@calle", direccion.Calle);
                            direccionCommand.Parameters.AddWithValue("@numero", direccion.Numero);
                            direccionCommand.Parameters.AddWithValue("@comuna", direccion.Comuna);
                            direccionCommand.Parameters.AddWithValue("@region", direccion.Region);
                            direccionCommand.Parameters.AddWithValue("@codigoPostal", direccion.CodigoPostal);
                            direccionCommand.Parameters.AddWithValue("@esPrincipal", direccion.EsPrincipal);

                            await direccionCommand.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return cliente;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Vendedor> UpdateVendedor(Vendedor vendedor)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE vendedores 
                    SET sucursal_id = @sucursal_id
                    WHERE id = @id", connection);

                command.Parameters.AddWithValue("@id", vendedor.Id);
                command.Parameters.AddWithValue("@sucursal_id", vendedor.SucursalId);

                await command.ExecuteNonQueryAsync();
                return vendedor;
            }
        }

        public async Task<Bodeguero> UpdateBodeguero(Bodeguero bodeguero)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE bodegueros 
                    SET sucursal_id = @sucursal_id
                    WHERE id = @id", connection);

                command.Parameters.AddWithValue("@id", bodeguero.Id);
                command.Parameters.AddWithValue("@sucursal_id", bodeguero.SucursalId);

                await command.ExecuteNonQueryAsync();
                return bodeguero;
            }
        }

        public async Task<Contador> UpdateContador(Contador contador)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand(@"
                    UPDATE contadores 
                    SET usuario_id = @usuario_id
                    WHERE id = @id", connection);

                command.Parameters.AddWithValue("@id", contador.Id);
                command.Parameters.AddWithValue("@usuario_id", contador.UsuarioId);

                await command.ExecuteNonQueryAsync();
                return contador;
            }
        }

        // Eliminar usuarios
        public async Task<bool> DeleteUsuario(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("DELETE FROM usuarios WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> DeleteCliente(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Primero eliminamos las direcciones
                    using var deleteDireccionesCommand = new MySqlCommand("DELETE FROM direcciones WHERE cliente_id = @id", connection, transaction);
                    deleteDireccionesCommand.Parameters.AddWithValue("@id", id);
                    await deleteDireccionesCommand.ExecuteNonQueryAsync();

                    // Luego eliminamos el cliente
                    using var deleteClienteCommand = new MySqlCommand("DELETE FROM clientes WHERE id = @id", connection, transaction);
                    deleteClienteCommand.Parameters.AddWithValue("@id", id);
                    var result = await deleteClienteCommand.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                    return result > 0;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteVendedor(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("DELETE FROM vendedores WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> DeleteBodeguero(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("DELETE FROM bodegueros WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> DeleteContador(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("DELETE FROM contadores WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        // Cambiar estado de usuarios
        public async Task<bool> ActivarUsuario(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("UPDATE usuarios SET activo = 1 WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> DesactivarUsuario(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new MySqlCommand("UPDATE usuarios SET activo = 0 WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                return await command.ExecuteNonQueryAsync() > 0;
            }
        }
    }
} 