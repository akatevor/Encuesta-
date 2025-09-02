using Microsoft.AspNetCore.Mvc;
using Encuesta.Data;
using Microsoft.AspNetCore.Http;

namespace Encuesta.Controllers
{
    public class EncuestaController : Controller
    {
        private readonly ConexionMySql _db;

        public EncuestaController(ConexionMySql db)
        {
            _db = db;
        }

        // GET: Encuesta
        public IActionResult Index()
        {
            // Verificar si el usuario está logueado
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");

            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Username = username;
            ViewBag.UserId = userId;

            // Verificar si ya completó la encuesta
            var completada = _db.EncuestaCompletada(userId.Value);
            ViewBag.ReadOnly = completada;

            if (completada)
            {
                ViewBag.Mensaje = "Ya has completado esta encuesta";
                ViewBag.Respuestas = _db.ObtenerRespuestas(userId.Value); // opcional: mostrar respuestas previas
            }
            else
            {
                ViewBag.Respuestas = new string[10];
            }

            return View();
        }

        // POST: Encuesta
        [HttpPost]
        public IActionResult Index(string[] respuestas)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("Username");

            if (userId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.Username = username;
            ViewBag.UserId = userId;

            // Verificar si ya completó la encuesta
            if (_db.EncuestaCompletada(userId.Value))
            {
                ViewBag.ReadOnly = true;
                ViewBag.Mensaje = "Ya has completado esta encuesta";
                ViewBag.Respuestas = _db.ObtenerRespuestas(userId.Value); // opcional
                return View();
            }

            // Guardar las respuestas y marcar encuesta como completada
            var exito = _db.GuardarRespuesta(userId.Value, respuestas);

            if (exito)
            {
                // Redirigir a página de éxito
                return RedirectToAction("Completada", "Encuesta");
            }
            else
            {
                ViewBag.Error = "Error al guardar la encuesta. Inténtalo de nuevo.";
                ViewBag.Respuestas = respuestas;
                return View();
            }
        }

        // GET: Encuesta/Completada
        public IActionResult Completada()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;
            return View();
        }
         public IActionResult Encuesta()
        {
            return View("~/Views/Encuesta/Encuesta.cshtml");
        }
    }
}