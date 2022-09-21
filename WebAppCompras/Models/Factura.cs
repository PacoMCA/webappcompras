using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebAppCompras.Models
{
    public class Factura
    {
        //cfdi:Comprobante
        private string _fecha, _folio, _serie, _formapago, _moneda, _metodopago, _uuid, _tipoDeComprobante;
        private double _subtotal, _total, _descuento;
        //cfdi:Emisor
        private string _rfcE, _nombreE, _regimen;
        //cfdi:Receptor
        private string _rfcR, _nombreR, _usoCfdi;
        //cfdi:Conceptos
        //private string _claveProdServ, _claveUnidad, _unidad, _descripcion;
        //private double _descuento, _cantidad, _valorunitario, _importe, _descuentoProducto;
        //cfdi:Impuestos
        private double _impuesto, _tasaOCuota, _importeImpuesto, _totalImpuestosTrasladados;

        private List<Producto> _productos;

        public Factura(){}

        public string Fecha { get => _fecha; set => _fecha = value; }
        public string Folio { get => _folio; set => _folio = value; }
        public string Serie { get => _serie; set => _serie = value; }
        public string Formapago { get => _formapago; set => _formapago = value; }
        public string Moneda { get => _moneda; set => _moneda = value; }
        public string Metodopago { get => _metodopago; set => _metodopago = value; }
        public string Uuid { get => _uuid; set => _uuid = value; }
        public string TipoDeComprobante { get => _tipoDeComprobante; set => _tipoDeComprobante = value; }

        [DataType(DataType.Currency)]
        public double Subtotal { get => _subtotal; set => _subtotal = value; }

        [DataType(DataType.Currency)]
        public double Total { get => _total; set => _total = value; }

        public string RfcE { get => _rfcE; set => _rfcE = value; }
        public string NombreE { get => _nombreE; set => _nombreE = value; }
        public string Regimen { get => _regimen; set => _regimen = value; }
        public string RfcR { get => _rfcR; set => _rfcR = value; }
        public string NombreR { get => _nombreR; set => _nombreR = value; }
        public string UsoCfdi { get => _usoCfdi; set => _usoCfdi = value; }
        public double Impuesto { get => _impuesto; set => _impuesto = value; }
        public double TasaOCuota { get => _tasaOCuota; set => _tasaOCuota = value; }
        public double ImporteImpuesto { get => _importeImpuesto; set => _importeImpuesto = value; }
        public double TotalImpuestosTrasladados { get => _totalImpuestosTrasladados; set => _totalImpuestosTrasladados = value; }
        public List<Producto> Productos { get => _productos; set => _productos = value; }
        public double Descuento { get => _descuento; set => _descuento = value; }
    }
}