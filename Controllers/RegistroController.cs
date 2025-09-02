using Microsoft.AspNetCore.Mvc;
using Encuesta.Data;

namespace Encuesta.Controllers
{
    public class RegistroController : Controller
    {
        private readonly ConexionMySql db = new ConexionMySql();

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Facultades = db.ObtenerFacultades();
            ViewBag.Ciudades = db.ObtenerCiudades();
            return View("~/Views/Register.cshtml");
        }

        [HttpPost]
        public IActionResult ComprobarRegistro(
            string nombre, string apellido, string facultad, string ciudad,
            string sexo, string usuario, string password, string confirmarPassword)
        {
            // 1. Validar que las contraseñas coincidan
            if (password != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                ViewBag.Facultades = db.ObtenerFacultades();
                ViewBag.Ciudades = db.ObtenerCiudades();
                return View("~/Views/Register.cshtml");
            }

            // 2. Llamar al método para registrar el usuario
            bool exito = db.RegistrarUsuario(nombre, apellido, facultad, ciudad, sexo, usuario, password);

            if (!exito)
            {
                ViewBag.Error = "Ocurrió un error al registrar el usuario";
                ViewBag.Facultades = db.ObtenerFacultades();
                ViewBag.Ciudades = db.ObtenerCiudades();
                return View("~/Views/Register.cshtml");
            }

            // 3. Redirigir al login después del registro
            return RedirectToAction("Login","Login");
        }
    }
}
