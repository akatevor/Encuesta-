using Microsoft.AspNetCore.Mvc;
using Encuesta.Data;
using Microsoft.AspNetCore.Http;

namespace Encuesta.Controllers
{
    public class LoginController : Controller
    {
        private readonly ConexionMySql _db;

        public LoginController(ConexionMySql db)
        {
            _db = db;
        }

        // GET: Login
        public IActionResult Login()
        {
            // Limpiar sesión por seguridad
            HttpContext.Session.Clear();
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Verificar credenciales directamente
            var usuario = _db.ObtenerUsuario(username, password);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            // Guardar en sesión
           // Guardar sesión
HttpContext.Session.SetInt32("UserId", usuario.Id);
HttpContext.Session.SetString("Username", usuario.Username);
HttpContext.Session.SetString("Rol", usuario.Rol);

// Redirigir según rol
if (usuario.Rol == "ADMIN")
{
    return RedirectToAction("Encuestas", "Admin");
}
else
{
    // Usuario normal
    if (_db.EncuestaCompletada(usuario.Id))
    {
        return RedirectToAction("Completada", "Encuesta");
    }
    else
    {
        return RedirectToAction("Index", "Encuesta");
    }
}

        }

        // Cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}
