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
    public class ProveedoresController : Controller
    {
        // GET: Proveedores
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult getProveedores()
        {
            List<Proveedor> lisP = new List<Proveedor>();
            try
            {
                IList<object> proveedores = APIToolkit.SelectStoredProcedure("select id as Id, s_descripcion as S_descripcion, s_RFC as S_RFC from cat_proveedores", new Proveedor());
                for (int i = 0; i < proveedores.Count(); i++){
                    Proveedor p = new Proveedor();
                    p.Id = Convert.ToInt32(proveedores[i].GetType().GetProperty("Id").GetValue(proveedores[i]));
                    p.S_descripcion = proveedores[i].GetType().GetProperty("S_descripcion").GetValue(proveedores[i]).ToString();
                    p.S_RFC = proveedores[i].GetType().GetProperty("S_RFC").GetValue(proveedores[i]).ToString();
                    lisP.Add(p);
                }
            }
            catch (Exception e)
            {
                return Json(new { Value = false }, JsonRequestBehavior.AllowGet);
            }
            List<Proveedor> ls = (from s in lisP select s).ToList();
            return Json(ls);//return View("Detalle",f);
        }

        // GET: Proveedores/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
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

        // GET: Proveedores/Edit/5
        public ActionResult Edit(int id)
        {
            var q = APIToolkit.SelectSP("Select id, s_descripcion, s_RFC from cat_proveedores where id=" + id + ";");
            Proveedor p = new Proveedor();
            var proveedor = ((IDictionary<String, Object>)q[0]);
            p.Id = Convert.ToInt32(proveedor["id"]);
            p.S_descripcion =proveedor["s_descripcion"].ToString();
            p.S_RFC = proveedor["s_RFC"].ToString();
            return View(p);
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        public JsonResult Edit(Proveedor p)
        {
            try
            {
                var qDP = "UPDATE cat_proveedores SET s_descripcion = '" + p.S_descripcion + "', s_RFC='" + p.S_RFC + "' WHERE id=" + p.Id + ";";
                var updateP = instruccionesSQL.InsertarFila(qDP);
                if (updateP == true)
                {
                    return Json("Proveedor actualizado!!!");
                }
                else { return Json("No se actualizo el Proveedor!"); }

            }
            catch
            {
                return Json("Error controller Proveedor");
            }
        }
        // POST: Proveedores/Delete/5
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
