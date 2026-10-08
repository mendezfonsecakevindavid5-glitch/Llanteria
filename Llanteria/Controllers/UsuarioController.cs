using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace Llanteria.Controllers;

public class UsuarioController : Controller
{
    private readonly UsuarioService ser;
    public UsuarioController(UsuarioService usuarioService)
    {
        ser = usuarioService;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult IniciarAutenticacion(string username, string password)
    {
        // Buscar el usuario solo por nombre o correo
        var user = ser.GetUsuarios()
        .FirstOrDefault(u => u.Username == username || u.Correo == username);

        // Verificar si existe y si la contraseña combinada coincide utilizando el Helper
        if (user == null || !Llanteria.Services.SecurityService.VerificarPassword(password, user.PasswordHash))
        {
            return Json(new { success = false, message = "Nombre de usuario o contraseña incorrectos." });
        }

        if (user.Estado != "Activo")
        {
            return Json(new { success = false, message = "Su cuenta se encuentra: " + user.Estado });
        }

        // Verifica si el correo está vacío, nulo o contiene el correo por defecto de migración
        bool sinCorreoValido = string.IsNullOrWhiteSpace(user.Correo) || user.Correo == "sin_correo@llanteria.com";

        if (sinCorreoValido)
        {
            // PRIMERA VEZ: UltimaConexion es null -> Ingresa por única vez
            if (user.UltimaConexion == null)
            {
                user.UltimaConexion = DateTime.Now; // Se registra el ingreso para invalidar futuros intentos sin correo
                ser.UpdateUsuario(user);

                HttpContext.Session.SetString("UsuarioId", user.Id.ToString());

                return Json(new
                {
                    success = true,
                    esUsuarioLegacy = true,
                    redirectUrl = Url.Action("Index", "Home")
                });
            }

            // SEGUNDA VEZ EN ADELANTE: UltimaConexion ya no es null -> Se bloquea el inicio de sesión
            return Json(new
            {
                success = false,
                message = "Acceso denegado. No tiene un correo electrónico registrado para completar la verificación en dos pasos (2FA). Contacte al administrador para vincular su correo."
            });
        }

        // Flujo Estándar: Generación y envío del código 2FA
        string codigoVerificacion = new Random().Next(100000, 999999).ToString();
        HttpContext.Session.SetString($"2FA_Code_{username}", codigoVerificacion);
        HttpContext.Session.SetString($"2FA_UserPending", username);



        // Lógica de envío de correo omitida para brevedad de servicio SMTP



        return Json(new { success = true, esUsuarioLegacy = false });

    }



    [HttpPost]

    public IActionResult VerificarCodigo(string username, string code)

    {

        string? codigoGuardado = HttpContext.Session.GetString($"2FA_Code_{username}");



        if (!string.IsNullOrEmpty(codigoGuardado) && codigoGuardado == code)

        {

            var user = ser.GetUsuarios().FirstOrDefault(u => u.Username == username);

            if (user != null)

            {

                user.UltimaConexion = DateTime.Now;

                ser.UpdateUsuario(user);

                HttpContext.Session.SetString("UsuarioId", user.Id.ToString());

            }



            HttpContext.Session.Remove($"2FA_Code_{username}");

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });

        }



        return Json(new { success = false, message = "El código ingresado es incorrecto o ha expirado." });

    }



    [HttpPost]

    public IActionResult ReenviarCodigo(string username)

    {

        string nuevoCodigo = new Random().Next(100000, 999999).ToString();

        HttpContext.Session.SetString($"2FA_Code_{username}", nuevoCodigo);



        // Lógica de reenvío por correo SMTP



        return Json(new { success = true });

    }

}