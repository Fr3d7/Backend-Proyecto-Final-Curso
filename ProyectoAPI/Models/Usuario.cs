using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace ProyectoAPI.Models;

public class Usuario
{
    public int id { get; set; }

    [Required]
    [JsonPropertyName("nombre")]
    public string nombre { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("correo")]
    public string correo { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("contrasena")] // ← cambia aquí
    public string contrasena { get; set; } = string.Empty;
}
