using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppCompras.Models
{
    public class Impuesto
    {
        private int _id;
        private string _impuesto;
        private double _tasa, _importe;
        public Impuesto()
        {

        }
        public int Id { get => _id; set => _id = value; }
        public string _Impuesto { get => _impuesto; set => _impuesto = value; }
        public double Tasa { get => _tasa; set => _tasa = value; }
        public double Importe { get => _importe; set => _importe = value; }
    }
}