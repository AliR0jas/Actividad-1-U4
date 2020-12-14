using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Actividad_1_U4.Models;

namespace Actividad_1_U4.Repositories
{
    public class Repository<T> where T:class
    {
        public controlusuariosContext Context { get; set; }

        public Repository(controlusuariosContext context)
        {
            Context = context;
        }
        public Usuario ObtenerUsuarioPorId(int id)
        {
            return Context.Usuario.FirstOrDefault(x => x.Id == id);
        }
        public Usuario ObtenerUsuarioPorCorreo(string correo)
        {
            return Context.Usuario.FirstOrDefault(x => x.Correo == correo);
        }
        public Usuario ObtenerUsuario(Usuario id)
        {
            return Context.Find<Usuario>(id);
        }

        public bool Validaciones(Usuario usuario)
        {
            if (string.IsNullOrEmpty(usuario.NombreUsuario))
                throw new Exception("Ingrese el nombre de usuario.");
            if (string.IsNullOrEmpty(usuario.Correo))
                throw new Exception("Ingrese el correo electrónico del usuario.");
            if (string.IsNullOrEmpty(usuario.Contraseña))
                throw new Exception("Ingrese la contraseña del usuario.");
            return true;
        }

        public virtual void Insertar(Usuario usuario)
        {
            if (Validaciones(usuario))
            {
                Context.Add(usuario);
                Context.SaveChanges();
            }
        }
        public virtual void Editar(Usuario usuario)
        {
            if (Validaciones(usuario))
            {
                Context.Update<Usuario>(usuario);
                Context.SaveChanges();
            }
        }
        public virtual void Eliminar(Usuario usuario)
        {
            Context.Remove<Usuario>(usuario);
            Context.SaveChanges();
        }
    }
}
