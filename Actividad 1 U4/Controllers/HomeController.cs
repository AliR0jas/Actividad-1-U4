using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Actividad_1_U4.Models;
using Actividad_1_U4.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Actividad_1_U4.Helpers;

namespace Actividad_1_U4.Controllers
{
    public class HomeController : Controller
    {
        public IWebHostEnvironment Environment { get; set; }
        public HomeController(IWebHostEnvironment env)
        {
            Environment = env;
        }
        [Authorize(Roles = "Cliente")]
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Registrar()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Registrar(Usuario u, string contraseña, string contraseña2)
        {
            controlusuariosContext controlusuariosContext = new controlusuariosContext();
            try
            {
                if (controlusuariosContext.Usuario.Any(x => x.Correo == u.Correo))
                {
                    ModelState.AddModelError("", "Cuenta ya registrada con esta dirección de correo.");
                    return View(u);
                }
                else
                {
                    if (contraseña == contraseña2)
                    {
                        Repository<Usuario> repos = new Repository<Usuario>(controlusuariosContext);
                        u.Contraseña = HashingHelpers.GetHash(contraseña);
                        u.Codigo = Helper.GetCodigo();
                        u.Activo = 0;
                        repos.Insertar(u);

                        MailMessage message = new MailMessage();
                        message.From = new MailAddress("sistemascomputacionales7g@gmail.com", "Servicio de streaming.");
                        message.To.Add(u.Correo);
                        message.Subject = "Activar cuenta";
                        string mensaje = System.IO.File.ReadAllText(Environment.WebRootPath + "/Inicio.html");
                        message.IsBodyHtml = true;
                        message.Body = mensaje.Replace("{##Codigo##}", u.Codigo.ToString());

                        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential("sistemascomputacionales7g@gmail.com", "sistemas7g");
                        client.Send(message);


                        List<Claim> informacion = new List<Claim>();
                        informacion.Clear();
                        informacion.Add(new Claim("CorreoActivar", u.Correo));

                        return RedirectToAction("Activar");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Las contraseñas ingresadas no coinciden");
                        return View(u);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(u);
            }
        }
        
        [AllowAnonymous]
        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> IniciarSesion(Usuario u, bool persistente)
        {
            controlusuariosContext controlusuariosContext = new controlusuariosContext();
            Repository<Usuario> usuarioRepository = new Repository<Usuario>(controlusuariosContext);
            var usuario = usuarioRepository.ObtenerUsuarioPorCorreo(u.Correo);

            if (usuario != null && HashingHelpers.GetHash(u.Contraseña) == usuario.Contraseña)
            {
                if (usuario.Activo == 1)
                {
                    List<Claim> informacion = new List<Claim>();
                    informacion.Add(new Claim(ClaimTypes.Name, "Usuario" + usuario.NombreUsuario));
                    informacion.Add(new Claim(ClaimTypes.Role, "Cliente"));
                    informacion.Add(new Claim("Correo", usuario.Correo));
                    informacion.Add(new Claim("Nombre", usuario.NombreUsuario));

                    var claimsIdentity = new ClaimsIdentity(informacion, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    if (persistente == true)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                        new AuthenticationProperties { IsPersistent = true });
                    }
                    else
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal,
                        new AuthenticationProperties { IsPersistent = false });
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "La cuenta registrada con este correo electronico no esta activa");
                    return View(u);
                }

            }
            else
            {
                ModelState.AddModelError("", "El usuario o la contraseña son incorrectas");
                return View(u);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("IniciarSesion");
        }

        [AllowAnonymous]
        public IActionResult Activar()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Activar(int codigo)
        {
            controlusuariosContext context = new controlusuariosContext();
            Repository<Usuario> repository = new Repository<Usuario>(context);
            var usuario = context.Usuario.FirstOrDefault(x => x.Codigo == codigo);

            if (usuario != null && usuario.Activo == 0)
            {
                var code = usuario.Codigo;
                if (codigo == code)
                {
                    usuario.Activo = 1;
                    repository.Editar(usuario);
                    return RedirectToAction("IniciarSesion");
                }
                else
                {
                    ModelState.AddModelError("", "El código ingresado no coincide con el original.");
                    return View((object)codigo);
                }
            }
            else
            {
                ModelState.AddModelError("", "El usuario no existe.");
                return View((object)codigo);
            }
        }
        [Authorize(Roles = "Cliente")]
        public IActionResult CambiarContra()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public IActionResult CambiarContra(string correo, string contraseña, string nuevaContraseña, string confirmarNuevaContraseña)
        {
            controlusuariosContext context = new controlusuariosContext();
            Repository<Usuario> repository = new Repository<Usuario>(context);
            try
            {
                var usuario = repository.ObtenerUsuarioPorCorreo(correo);

                if (usuario.Contraseña != HashingHelpers.GetHash(contraseña))
                {
                    ModelState.AddModelError("", "La contraseña ingresada no es correcta.");
                    return View();
                }
                else
                {
                    if (nuevaContraseña != confirmarNuevaContraseña)
                    {
                        ModelState.AddModelError("", "Las contraseñas no coinciden.");
                        return View();
                    }
                    else if (usuario.Contraseña == HashingHelpers.GetHash(nuevaContraseña))
                    {
                        ModelState.AddModelError("", "La nueva contraseña no puede ser igual a la actual.");
                        return View();
                    }
                    else
                    {
                        usuario.Contraseña = HashingHelpers.GetHash(nuevaContraseña);
                        repository.Editar(usuario);
                        return RedirectToAction("IniciarSesion");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
        [AllowAnonymous]
        public IActionResult RecuperarContra()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult RecuperarContra(string correo)
        {
            try
            {
                controlusuariosContext context = new controlusuariosContext();
                Repository<Usuario> repository = new Repository<Usuario>(context);
                var usuario = repository.ObtenerUsuarioPorCorreo(correo);

                if (usuario != null)
                {
                    var contraTemp = Helper.GetCodigo();
                    MailMessage mensaje = new MailMessage();
                    mensaje.From = new MailAddress("sistemascomputacionales7g@gmail.com", "Servicio de streaming");
                    mensaje.To.Add(correo);
                    mensaje.Subject = "Recupera tu contraseña";
                    string text = System.IO.File.ReadAllText(Environment.WebRootPath + "/Recuperacion.html");
                    mensaje.Body = text.Replace("{##contraTemp##}", contraTemp.ToString());
                    mensaje.IsBodyHtml = true;

                    SmtpClient cliente = new SmtpClient("smtp.gmail.com", 587);
                    cliente.EnableSsl = true;
                    cliente.UseDefaultCredentials = false;
                    cliente.Credentials = new NetworkCredential("sistemascomputacionales7g@gmail.com", "sistemas7g");
                    cliente.Send(mensaje);
                    usuario.Contraseña = HashingHelpers.GetHash(contraTemp.ToString());
                    repository.Editar(usuario);
                    return RedirectToAction("IniciarSesion");
                }
                else
                {
                    ModelState.AddModelError("", "El correo electrónico ingresado no está registrado.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View((object)correo);
            }
        }
        [Authorize(Roles = "Cliente")]
        public IActionResult EliminarCuenta()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public IActionResult EliminarCuenta(string correo, string contra)
        {
            try
            {
                controlusuariosContext context = new controlusuariosContext();
                Repository<Usuario> repository = new Repository<Usuario>(context);
                var usuario = repository.ObtenerUsuarioPorCorreo(correo);
                if (usuario != null)
                {
                    if (HashingHelpers.GetHash(contra) == usuario.Contraseña)
                    {
                        repository.Eliminar(usuario);
                    }
                    else
                    {
                        ModelState.AddModelError("", "La contraseña es incorrecta.");
                        return View();
                    }
                }
                return RedirectToAction("IniciarSesion");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ocurrió un error. Inténtelo más tarde.");
                return View();
            }
        }
        [AllowAnonymous]
        public IActionResult Denegado()
        {
            return View();
        }
    }
}
