using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Web.Script.Serialization;
using System.Xml;
using WebAppCompras.Models;
using WebAppCompras.db;
using Newtonsoft.Json.Linq;
namespace WebAppCompras.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Compra()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LoadFile(HttpPostedFileBase fileUpload)
        {
            Factura f = new Factura();
            try
            {
                string path = Server.MapPath("~/App_Data/Adjuntos/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Server.MapPath("~/App_Data/Adjuntos/");
                fileUpload.SaveAs(path + Path.GetFileName(fileUpload.FileName));
                //readXml(path + fileUpload.FileName);
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(path + fileUpload.FileName);
                XmlNodeReader nodos = new XmlNodeReader(xDoc);

                
                List<Producto> productos = new List<Producto>();

                Producto p1 = new Producto();
                Impuesto impu = new Impuesto();
                f.Productos = new List<Producto>();
                List<Impuesto> listImpu = new List<Impuesto>();

                while (nodos.Read())
                {
                    if (nodos.Name == "cfdi:Comprobante")
                    {
                        f.Serie = nodos["Serie"].ToString();
                        f.Folio = nodos["Folio"].ToString();
                        f.Fecha = Convert.ToDateTime(nodos["Fecha"]).ToShortDateString();
                        f.Formapago = nodos["FormaPago"].ToString();
                        f.Subtotal = Convert.ToDouble(nodos["SubTotal"]);
                        f.Descuento = Convert.ToDouble(nodos["Descuento"]);
                        f.Total = Convert.ToDouble(nodos["Total"]);
                        f.Moneda = nodos["Moneda"].ToString();
                        f.TipoDeComprobante = nodos["TipoDeComprobante"].ToString();
                        f.Metodopago = nodos["MetodoPago"].ToString();
                    }
                    else if (nodos.Name == "cfdi:Emisor")
                    {
                        f.RfcE = nodos["Rfc"].ToString();
                        f.NombreE = nodos["Nombre"].ToString();
                        f.Regimen = nodos["RegimenFiscal"].ToString();
                    }
                    else if (nodos.Name == "cfdi:Receptor")
                    {
                        f.RfcR = nodos["Rfc"].ToString();
                        f.NombreR = nodos["Nombre"].ToString();
                        f.UsoCfdi = nodos["UsoCFDI"].ToString();
                    }
                    else if (nodos.Name == "cfdi:Concepto")
                    {
                        Producto p = new Producto();
                        p.ClaveProdServ = nodos["ClaveProdServ"];
                        p.Cantidad = Convert.ToDouble(nodos["Cantidad"]);
                        p.ClaveUnidad = nodos["ClaveUnidad"];
                        p.Unidad = nodos["Unidad"];
                        p.Descripcion = nodos["Descripcion"];
                        p.Valorunitario = Convert.ToDouble(nodos["ValorUnitario"]);
                        p.Importe = Convert.ToDouble(nodos["Importe"]);
                        p.DescuentoProducto = Convert.ToDouble(nodos["Descuento"]);
                        p1 = p;
                    }
                    else if (nodos.Name == "cfdi:Traslado")
                    {
                        Impuesto im = new Impuesto();
                        im.Importe = Convert.ToDouble(nodos["Importe"]);
                        im.Tasa = Convert.ToDouble(nodos["TasaOCuota"]) * 100;
                        im._Impuesto = "IVA 16%";
                        if (listImpu.FindAll(x => x._Impuesto == im._Impuesto).Count == 0) {
                            listImpu.Add(im);
                        }
                        p1.Impuestos = listImpu;
                        if (f.Productos.FindAll(x => x.Descripcion == p1.Descripcion).Count == 0)
                        {
                            f.Productos.Add(p1);
                        }
                    }
                    else if (nodos.Name == "cfdi:Retencion")
                    {
                        Impuesto im = new Impuesto();
                        im.Importe = Convert.ToDouble(nodos["Importe"]);
                        im.Tasa = Convert.ToDouble(nodos["TasaOCuota"]) * 100;
                        if (nodos["Impuesto"].ToString() == "002") { im._Impuesto = "IVA Retenido"; }
                        else { im._Impuesto = "ISR Retenido"; }
                        if (listImpu.FindAll(x => x._Impuesto == im._Impuesto).Count == 0) {
                            listImpu.Add(im);
                        }
                    }
                    else if (nodos.Name == "cfdi:Impuestos")
                    {
                        f.TotalImpuestosTrasladados = Convert.ToDouble(nodos["TotalImpuestosTrasladados"]);
                    }
                    else if (nodos.Name == "implocal:RetencionesLocales")
                    {
                        Impuesto im = new Impuesto();
                        im._Impuesto = nodos["ImpLocRetenido"].ToString();
                        im.Tasa = Convert.ToDouble(nodos["TasadeRetencion"]);
                        im.Importe = Convert.ToDouble(nodos["Importe"]);
                        if (listImpu.FindAll(x => x._Impuesto == im._Impuesto).Count == 0){
                            listImpu.Add(im);
                        }
                        p1.Impuestos = listImpu;
                        if (f.Productos.FindAll(x => x.Descripcion == p1.Descripcion).Count == 0)
                        {
                            f.Productos.Add(p1);
                        }
                    }
                    else if (nodos.Name == "tfd:TimbreFiscalDigital")
                    {
                        f.Uuid = nodos["UUID"];
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { Value = false, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Data = f }, JsonRequestBehavior.AllowGet);//return View("Detalle",f);
        }

        public ActionResult generaOC(Factura f, OrdenCompra oc)
        {
            var obj = new JObject();
            dynamic res = new JObject();
            Proveedor proveedor = new Proveedor();
            List<Producto> listProductos = new List<Producto>(); ;

            var rfc = APIToolkit.SelectSP($"select * from cat_proveedores WHERE s_RFC = '" + f.RfcE + "';");
            if (rfc.Count > 0)
            {
                var prove = ((IDictionary<String, Object>)rfc[0]);//Crea un objeto de tipo proveedor
                res = "Ya existe el proveedor";
                proveedor.Id = Convert.ToInt32(prove["id"]);
                proveedor.S_RFC = f.RfcE;
                proveedor.S_descripcion = f.NombreE;
            }
            else
            {//Inserta un nuevo proveedor
                var insertProv = APIToolkit.SelectSP("INSERT INTO cat_proveedores (s_descripcion, s_RFC) values ('"+f.NombreE+"','"+f.RfcE+"')");//sp_post_proveedores
                var ulidP = APIToolkit.SelectSP($"select ISNULL(max(id),0) as id from cat_proveedores");
                var idproved = ((IDictionary<String, Object>)ulidP[0]);
                proveedor.Id = Convert.ToInt32(idproved["id"]);
                proveedor.S_RFC = f.RfcE;
                proveedor.S_descripcion = f.NombreE;
            }

            for (int i = 0; i < f.Productos.Count; i++)
            {
                string nombre = f.Productos[i].Descripcion.Trim();
                var v = APIToolkit.SelectSP($"select * from cat_Producto where s_NombreProducto='" + nombre + "'");
                List<String> ivas = new List<string>();
                if (v.Count > 0) {
                    var pdto = ((IDictionary<String, Object>)v[0]);
                    Producto p = new Producto();
                    Impuesto im = new Impuesto();
                    p.Id = Convert.ToInt32(pdto["id"]);
                    p.Descripcion = nombre;
                    p.Unidad = f.Productos[i].Unidad;
                    p.Cantidad = f.Productos[i].Cantidad;
                    p.Valorunitario = f.Productos[i].Valorunitario;
                    p.Impuestos = f.Productos[i].Impuestos;
                    for (int z = 0; z < p.Impuestos.Count(); z++)
                    {
                        p.StrImpuestos += "|" + p.Impuestos[z]._Impuesto;
                    }
                    p.DescuentoProducto = f.Productos[i].DescuentoProducto;
                    p.Importe = f.Productos[i].Importe;

                    p.Total = p.Importe + p.ImporteImpuesto;
                    listProductos.Add(p);
                }
                else
                {
                    Producto pn = new Producto();
                    var idultimo = APIToolkit.SelectSP($"select ISNULL(max(id),0) as id from cat_Producto");
                    var idp = ((IDictionary<String, Object>)idultimo[0]);
                    int idpW = Convert.ToInt32(idp["id"]);
                    pn.Id = idpW + 1;
                    pn.Descripcion = nombre;
                    pn.Unidad = f.Productos[i].Unidad;
                    pn.Cantidad = f.Productos[i].Cantidad;
                    pn.Valorunitario = f.Productos[i].Valorunitario;
                    pn.Impuestos = f.Productos[i].Impuestos;
                    for (int z = 0; z < pn.Impuestos.Count(); z++){
                        pn.StrImpuestos += "|" + pn.Impuestos[z]._Impuesto;
                    }
                    pn.DescuentoProducto = f.Productos[i].DescuentoProducto;
                    pn.Importe = f.Productos[i].Importe;
                    
                    pn.Total = pn.Importe + pn.ImporteImpuesto;
                    listProductos.Add(pn);
                    var query = "insert into cat_Producto (s_ClaveEmpleado,s_NombreProducto,idCategoriaProducto,s_contenido,s_clavesat,s_Unidad,s_ClavePrincipal,s_Impuestos) values (";
                    query = query + "'ce2222','"+nombre+"',"+ (idpW + 1)+ ",'1','"+ f.Productos[i].ClaveProdServ + "','"+ f.Productos[i].Unidad + "','WS"+ (idpW + 1)+ "','"+ pn.StrImpuestos + "')";
                    var insertProducto = instruccionesSQL.InsertarFila(query);//sp_post_Producto_v2
                }
            }
            //INSERTAR UNA ORDEN DE COMPRA EN LA TABLA "tbl_OrdenCompra"
            var ulidOC = APIToolkit.SelectSP($"select ISNULL(max(id),0) as id from tbl_OrdenCompra");
            var idoc = ((IDictionary<String, Object>)ulidOC[0]);
            int idoc_ = Convert.ToInt32(idoc["id"]) + 1;
            var qoc = "INSERT INTO tbl_OrdenCompra (idsolicitud,s_FolioOrdenCompra,s_Proveedor,s_Unidad,s_ClaveEmpleado,d_FechaEntrega,cancelaciones) VALUES (";
            qoc = qoc + "'ce2222-"+idoc_+"','OC"+idoc_+"','"+proveedor.S_descripcion+"','"+f.Productos[0].Unidad+"',";
            qoc = qoc + "'ce2222','"+oc.FechaEntrega+"',1)";
            var insertOc = instruccionesSQL.InsertarFila(qoc);

            //INSERTAR UNA ORDEN DE COMPRA (MODAL) EN LA TABLA "tbl_OrdenCompra_CondPago"
            var ulidOCD = APIToolkit.SelectSP($"select ISNULL(max(id),0) as id from tbl_OrdenCompra_CondPago");
            var idocD = ((IDictionary<String, Object>)ulidOCD[0]);
            int idocD_ = Convert.ToInt32(idocD["id"]) + 1;
            var qocD = "INSERT INTO tbl_OrdenCompra_CondPago (s_FolioOrdenCompra,n_NumeroPago,d_FechaProgramada,s_Concepto,d_Monto,s_Penalizacion,d_PorcentajePenalizacion,s_ClaveEmpleado,b_FacturaGeneral,d_DescuentoTotal) VALUES (";
            qocD = qocD + "'OC" + idocD_ + "',1,'"+oc.FechaProgramada+"','"+oc.Concepto+"',"+f.Total+",'"+oc.Penalizacion+"',"+oc.PorcentajePenalizacion+",'ce2222',0,"+f.Descuento+")";
            var insertOcPago = instruccionesSQL.InsertarFila(qocD);

            //INSERTAR los productos en la tabla "tbl_cotizacionproducto", son los detalles de la oc
            for (int i = 0; i < listProductos.Count(); i++)
            {
                var ulidDP = APIToolkit.SelectSP($"select ISNULL(max(id),0) as id from tbl_cotizacionproducto");
                var idDP = ((IDictionary<String, Object>)ulidDP[0]);
                int idDP_ = Convert.ToInt32(idDP["id"]) + 1;
                for (int z = 0; z < listProductos[i].Impuestos.Count(); z++){
                    if(listProductos[i].Impuestos[z]._Impuesto != "IVA 16%") {
                        oc.Retencion += listProductos[i].Impuestos[z].Tasa;
                    }
                    else { oc.Iva = listProductos[i].Impuestos[z].Tasa; }
                }
                var qDP = "INSERT INTO tbl_cotizacionproducto (IdCotizacion,idProducto,s_FolioOrdenCompra,idProveedor,n_Cantidad,d_PrecioUnitario,d_Descuento,d_IVA,d_PrecioTotal,d_Total,s_Observaciones,d_Retencion) VALUES (";
                qDP = qDP + "'22-" + idDP_ + "',"+listProductos[i].Id+",'OC"+ idocD_+ "',"+proveedor.Id+","+listProductos[i].Cantidad+","+listProductos[i].Valorunitario+","+listProductos[i].DescuentoProducto+","+oc.Iva+","+listProductos[i].Total+","+listProductos[i].Importe+",'"+listProductos[i].Unidad+"', "+ oc.Retencion + ")";
                var insertDP = instruccionesSQL.InsertarFila(qDP);
            }
            return View("");
        }
        public ActionResult OC()
        {
            return View();
        }
        [HttpPost]
        public JsonResult getOC()
        {
            List<Solicitud> list = new List<Solicitud>();
            try {
                //var sp = APIToolkit.SelectSP($"exec sp_get_oc");
                IList<object> Solicitudes = APIToolkit.SelectStoredProcedure("sp_get_oc", new Solicitud());
                for (int i = 0; i < Solicitudes.Count(); i++) {
                    Solicitud s = new Solicitud();
                    s.Id = Solicitudes[i].GetType().GetProperty("Id").GetValue(Solicitudes[i]).ToString();
                    s.Usuario = Solicitudes[i].GetType().GetProperty("Usuario").GetValue(Solicitudes[i]).ToString();
                    s.Dilacion = Solicitudes[i].GetType().GetProperty("Dilacion").GetValue(Solicitudes[i]).ToString();
                    s.TipoSolicitud = Solicitudes[i].GetType().GetProperty("TipoSolicitud").GetValue(Solicitudes[i]).ToString();
                    s.Monto = Convert.ToDouble(Solicitudes[i].GetType().GetProperty("Monto").GetValue(Solicitudes[i]));
                    s.Fecha = Solicitudes[i].GetType().GetProperty("Fecha").GetValue(Solicitudes[i]).ToString();
                    s.Empresa = Solicitudes[i].GetType().GetProperty("Empresa").GetValue(Solicitudes[i]).ToString();
                    s.Etapa = Solicitudes[i].GetType().GetProperty("Etapa").GetValue(Solicitudes[i]).ToString();
                    s.Responsable = Solicitudes[i].GetType().GetProperty("Responsable").GetValue(Solicitudes[i]).ToString();
                    s.Proveedor = Solicitudes[i].GetType().GetProperty("Proveedor").GetValue(Solicitudes[i]).ToString();
                    list.Add(s);
                }
            }
            catch (Exception e)
            {
                return Json(new { Value = false}, JsonRequestBehavior.AllowGet);
            }
            List<Solicitud> ls = (from s in list select s).ToList();
            return Json(ls);//return View("Detalle",f);
        }
    }
}