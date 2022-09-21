using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppCompras.Models
{
    public class Proveedor
    {
        private int _id;
        private string s_descripcion, s_RFC;
        public Proveedor(){}
        public int Id { get => _id; set => _id = value; }
        public string S_descripcion { get => s_descripcion; set => s_descripcion = value; }
        public string S_RFC { get => s_RFC; set => s_RFC = value; }
    }
}