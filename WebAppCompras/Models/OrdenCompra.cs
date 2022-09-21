using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppCompras.Models
{
    public class OrdenCompra
    {
        private int _id, _idProveedor;
        private string _fechaEntrega, _fechaProgramada, _folioOC, _proveedor, _observaciones, _claveUsuario, _concepto, _penalizacion, _unidad;
        private double _monto, _total, _precio, _cantidad, _iva, _descuento, _porcentajePenalizacion,_retencion;
        private List<Producto> Productos;

        public OrdenCompra(){}
        public int Id { get => _id; set => _id = value; }
        public int IdProveedor { get => _idProveedor; set => _idProveedor = value; }
        public string FechaEntrega { get => _fechaEntrega; set => _fechaEntrega = value; }
        public string FechaProgramada { get => _fechaProgramada; set => _fechaProgramada = value; }
        public string FolioOC { get => _folioOC; set => _folioOC = value; }
        public string Proveedor { get => _proveedor; set => _proveedor = value; }
        public string Observaciones { get => _observaciones; set => _observaciones = value; }
        public string ClaveUsuario { get => _claveUsuario; set => _claveUsuario = value; }
        public string Concepto { get => _concepto; set => _concepto = value; }
        public string Penalizacion { get => _penalizacion; set => _penalizacion = value; }
        public string Unidad { get => _unidad; set => _unidad = value; }
        public double Monto { get => _monto; set => _monto = value; }
        public double Total { get => _total; set => _total = value; }
        public double Precio { get => _precio; set => _precio = value; }
        public double Cantidad { get => _cantidad; set => _cantidad = value; }
        public double Iva { get => _iva; set => _iva = value; }
        public double Descuento { get => _descuento; set => _descuento = value; }
        public double PorcentajePenalizacion { get => _porcentajePenalizacion; set => _porcentajePenalizacion = value; }
        public double Retencion { get => _retencion; set => _retencion = value; }
        public List<Producto> Productos1 { get => Productos; set => Productos = value; }
    }
}