using Microsoft.AspNetCore.Mvc;
using Encuesta.Data;
using Microsoft.AspNetCore.Http;

namespace Encuesta.Controllers
{
    public class AdminController : Controller
    {
        private readonly ConexionMySql _db;

        public AdminController(ConexionMySql db)
        {
            _db = db;
        }

        // GET: Admin/Encuestas
        public IActionResult Encuestas(string search = "")
        {
            // Verificar sesión y rol
            var userId = HttpContext.Session.GetInt32("UserId");
            var rol = HttpContext.Session.GetString("Rol");

            if (userId == null || rol != "ADMIN")
            {
                return RedirectToAction("Login", "Login");
            }

            // Obtener encuestas (anónimo) y opcionalmente filtradas por búsqueda
            var encuestas = _db.ObtenerEncuestas(search);

            ViewBag.Search = search;
            ViewBag.Mensaje = TempData["Mensaje"]; // Mensaje de eliminación
            return View(encuestas);
        }

        // POST: Admin/EliminarEncuesta
        [HttpPost]
        public IActionResult EliminarEncuesta(int encuestaId)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "ADMIN")
            {
                return RedirectToAction("Login", "Login");
            }

            bool exito = _db.EliminarEncuesta(encuestaId);
            TempData["Mensaje"] = exito ? "Encuesta eliminada correctamente." : "Error al eliminar encuesta.";
            return RedirectToAction("Encuestas");
        }
    }
}
