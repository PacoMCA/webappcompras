using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppCompras.Models
{
    public class Producto2
    {
        private int _id;
        private string _unidad, _descripcion;
        public Producto2()
        {
        }
        public int Id { get => _id; set => _id = value; }
        public string Unidad { get => _unidad; set => _unidad = value; }
        public string Descripcion { get => _descripcion; set => _descripcion = value; }
    }
    public class Producto
    {
        private int _id;
        private string _claveProdServ, _claveUnidad, _unidad, _descripcion, _strImpuestos;
        private double _descuento, _cantidad, _valorunitario, _importe, _descuentoProducto;
        private double _impuesto, _tasaOCuota, _importeImpuesto;
        private double _subtotal, _total;
        private List<Impuesto> _impuestos;

        public Producto(){}


        public int Id { get => _id; set => _id = value; }
        public string ClaveProdServ { get => _claveProdServ; set => _claveProdServ = value; }
        public string ClaveUnidad { get => _claveUnidad; set => _claveUnidad = value; }
        public string Unidad { get => _unidad; set => _unidad = value; }
        public string Descripcion { get => _descripcion; set => _descripcion = value; }
        public string StrImpuestos { get => _strImpuestos; set => _strImpuestos = value; }
        public double Descuento { get => _descuento; set => _descuento = value; }
        public double Cantidad { get => _cantidad; set => _cantidad = value; }
        public double Valorunitario { get => _valorunitario; set => _valorunitario = value; }
        public double Importe { get => _importe; set => _importe = value; }
        public double DescuentoProducto { get => _descuentoProducto; set => _descuentoProducto = value; }
        public double Impuesto { get => _impuesto; set => _impuesto = value; }
        public double TasaOCuota { get => _tasaOCuota; set => _tasaOCuota = value; }
        public double ImporteImpuesto { get => _importeImpuesto; set => _importeImpuesto = value; }

        public double Subtotal { get => _subtotal; set => _subtotal = value; }
        public double Total { get => _total; set => _total = value; }
        public List<Impuesto> Impuestos { get => _impuestos; set => _impuestos = value; }
    }
}