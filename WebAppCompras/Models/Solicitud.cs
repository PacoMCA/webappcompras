using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppCompras.Models
{
    public class Solicitud
    {
        private string _id, _usuario, _dilacion, _tipoSolicitud, _fecha, _empresa, _etapa, _responsable, _proveedor;
        private double _monto;

        public Solicitud()
        {

        }
        public string Id { get => _id; set => _id = value; }
        public string Usuario { get => _usuario; set => _usuario = value; }
        public string Dilacion { get => _dilacion; set => _dilacion = value; }
        public string TipoSolicitud { get => _tipoSolicitud; set => _tipoSolicitud = value; }
        public string Fecha { get => _fecha; set => _fecha = value; }
        public string Empresa { get => _empresa; set => _empresa = value; }
        public string Etapa { get => _etapa; set => _etapa = value; }
        public string Responsable { get => _responsable; set => _responsable = value; }
        public string Proveedor { get => _proveedor; set => _proveedor = value; }
        public double Monto { get => _monto; set => _monto = value; }
    }
}