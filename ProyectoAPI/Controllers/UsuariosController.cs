using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAPI.Data;
using ProyectoAPI.Models;

using System;
using System.Linq;

namespace ProyectoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ProyectoDbContext _context;

        public UsuariosController(ProyectoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUsuarios()
        {
            try
            {
                var usuarios = _context.Usuarios.ToList();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al obtener usuarios: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CrearUsuario([FromBody] Usuario usuario)
        {
            Console.WriteLine(">>> Llegó al método CrearUsuario");

            if (usuario == null ||
                string.IsNullOrWhiteSpace(usuario.correo) ||
                string.IsNullOrWhiteSpace(usuario.contrasena))
            {
                return BadRequest("Faltan datos obligatorios.");
            }

            try
            {
                string correoPlano = usuario.correo.Trim().ToLower();
                string contraseñaPlano = usuario.contrasena.Trim();

                if (_context.Usuarios.Any(u => u.correo == correoPlano))
                {
                    return Conflict("El correo ya está registrado.");
                }

                usuario.correo = correoPlano;
                usuario.contrasena = contraseñaPlano;

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                Console.WriteLine(">>> Usuario insertado en la BD");

                // ✅ RESPUESTA CORRECTA PARA EL FRONTEND
                return Ok(new
                {
                    usuario = new
                    {
                        usuario.id,
                        usuario.nombre,
                        usuario.correo
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Error al guardar en la base de datos: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
            
        }


        [HttpGet("test-conn")]
        public IActionResult TestConnection()
        {
            try
            {
                var puedeConectar = _context.Database.CanConnect();
                return Ok(new { conectado = puedeConectar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string correoPlano = login.Correo?.Trim().ToLower();
            string contrasenaPlano = login.Contrasena?.Trim();

            var usuarioExistente = _context.Usuarios
                .FirstOrDefault(u =>
                    u.correo.Trim().ToLower() == correoPlano &&
                    u.contrasena.Trim() == contrasenaPlano);

            if (usuarioExistente == null)
            {
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos" });
            }

            return Ok(new
            {
                usuario = new
                {
                    usuarioExistente.id,
                    usuarioExistente.nombre,
                    usuarioExistente.correo
                }
            });
        }


    }

}
