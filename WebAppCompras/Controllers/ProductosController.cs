using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WebAppCompras.Models;
using WebAppCompras.db;
using Newtonsoft.Json.Linq;
using System.Data;
using Newtonsoft.Json;

namespace WebAppCompras.Controllers
{
    public class ProductosController : Controller
    {

        // GET: Productos
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult getProductos()
        {
            List<Producto> lisP = new List<Producto>();
            try
            {
                IList<object> productos = APIToolkit.SelectStoredProcedure("select id as Id, s_NombreProducto as Descripcion, s_Unidad as Unidad from cat_Producto", new Producto2());
                for (int i = 0; i < productos.Count(); i++) {
                    Producto p = new Producto();
                    p.Id = Convert.ToInt32(productos[i].GetType().GetProperty("Id").GetValue(productos[i]));
                    p.Descripcion = productos[i].GetType().GetProperty("Descripcion").GetValue(productos[i]).ToString();
                    p.Unidad = productos[i].GetType().GetProperty("Unidad").GetValue(productos[i]).ToString();
                    lisP.Add(p);
                }
            }
            catch (Exception e)
            {
                return Json(new { Value = false }, JsonRequestBehavior.AllowGet);
            }
            List<Producto> ls = (from s in lisP select s).ToList();
            return Json(ls);//return View("Detalle",f);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        public ActionResult Create(Producto p)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        // GET: Productos/Edit/5
        public ActionResult Edit(int id){
            List<Impuesto> li = new List<Impuesto>();
            var q = APIToolkit.SelectSP("Select * from cat_Producto where id=" + id + ";");
            Producto p = new Producto();
            var producto = ((IDictionary<String, Object>)q[0]);
            p.Id = Convert.ToInt32(producto["id"]);
            p.Descripcion = producto["s_NombreProducto"].ToString();
            p.Unidad = producto["s_Unidad"].ToString();
            p.ClaveProdServ = producto["s_clavesat"].ToString();
            p.StrImpuestos = producto["s_Impuestos"].ToString();
            
            return View(p);
        }

        public JsonResult getImpuestos(){
            try{
                List<Impuesto> l = new List<Impuesto>();
                var q = APIToolkit.SelectSP("select id,s_Descripcion from cat_Impuesto;");
                
                for (int i = 0; i < q.Count(); i++){
                    Impuesto im = new Impuesto();
                    var impto = ((IDictionary<String, Object>)q[i]);
                    im.Id = Convert.ToInt32(impto["id"]);
                    im._Impuesto = impto["s_Descripcion"].ToString();
                    l.Add(im);
                }
                return Json(new { Data=l}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e){
                return Json(new { Value = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Productos/Edit/5
        [HttpPost]
        public JsonResult Edit(Producto p){
            try{
                var qDP = "UPDATE cat_Producto SET s_NombreProducto = '"+p.Descripcion+"', s_clavesat='"+p.ClaveProdServ+"', s_Unidad='"+p.Unidad+"', s_Impuestos='"+p.StrImpuestos+"' WHERE id="+p.Id+";";
                var updateP = instruccionesSQL.InsertarFila(qDP);
                if(updateP == true) { 
                    return Json("Producto actualizado!!!"); }
                else { return Json("No se actualizo el producto!"); }

            }
            catch
            {
                return Json("Error controller Producto");
            }
        }

        // GET: Productos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Productos/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
