using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Tract_XRref.Models;

namespace Tract_XRref.Controllers
{
    public class EmployeeController : Controller
    {
        private SageBillingEntities db = new SageBillingEntities();

        // GET: Employee //Original Index Class
        //public ActionResult Index()
        //{
        //    return View(db.spEmployeeSelect().ToList());
        //}

        // GET: Employee/Details/5
        //public ActionResult Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    spEmployeeSelect_Result spEmployeeSelect_Result = db.spEmployeeSelect_Result.Find(id);
        //    if (spEmployeeSelect_Result == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(spEmployeeSelect_Result);
        //}

        // get employee/view  / index
        public ActionResult Index(string echo_employee_code, string employee_name)
        {
            //spEmployeeSelect_Result spEmployeeSelect_Result = db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).FirstOrDefault();
            if (echo_employee_code == null && employee_name != "")
            {
                //employee_name = employee_name.Replace("%20", " ");
                echo_employee_code = db.spEmployeeSelect().Where(s => s.Employee_Full_Name == employee_name).Select(s => s.Echo_Employee_Code).FirstOrDefault();
            }
            ViewBag.Echo_Employee_Code = new SelectList(db.spEmployeeSelect().Select(s => new { s.Echo_Employee_Code, Echo_Employee_Code2 = s.Echo_Employee_Code }), "Echo_Employee_Code", "Echo_Employee_Code2", echo_employee_code);

            if (echo_employee_code != "") {
                ViewBag.Employee_Full_Name = db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).Select(s => s.Employee_Full_Name).FirstOrDefault();
                ViewBag.Occupation = db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).Select(s => s.Occupation).LastOrDefault();
            } else if (employee_name != "") {
                ViewBag.Employee_Full_Name = employee_name;
            }

            ViewBag.eec = echo_employee_code;
            ViewBag.Client_Employee_ID_vb = db.spEmployeeSelect().Select(s => new { s.Client_Employee_ID, Client_Employee_ID2 = s.Client_Employee_ID }).Distinct().OrderBy(s => s.Client_Employee_ID).ToList();

            return View(db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(List<spEmployeeSelect_Result> spEmployeeSelect_Result, string echo_employee_code, string employee_name)
        {
            if (ModelState.IsValid)
            {
                foreach (spEmployeeSelect_Result viewData in spEmployeeSelect_Result)
                {
                    if (viewData.Original_Client_Employee_ID != viewData.Client_Employee_ID)
                    {
                        db.spEmployeeUpdate(viewData.Client_Site_ID, viewData.Original_Client_Employee_ID, viewData.Client_Employee_ID, echo_employee_code);
                    }
                }
                return RedirectToAction("Index", new { echo_employee_code = echo_employee_code });
            }

            if (echo_employee_code == null && employee_name != "")
            {
                echo_employee_code = db.spEmployeeSelect().Where(s => s.Employee_Full_Name == employee_name).Select(s => s.Echo_Employee_Code).FirstOrDefault();
            }
            ViewBag.Echo_Employee_Code = new SelectList(db.spEmployeeSelect().Select(s => new { s.Echo_Employee_Code, Echo_Employee_Code2 = s.Echo_Employee_Code }), "Echo_Employee_Code", "Echo_Employee_Code2", echo_employee_code);
            if (echo_employee_code != "")
            {
                ViewBag.Employee_Full_Name = db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).Select(s => s.Employee_Full_Name).FirstOrDefault();
                ViewBag.Occupation = db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).Select(s => s.Occupation).LastOrDefault();
            }
            else if (employee_name != "")
            {
                ViewBag.Employee_Full_Name = employee_name;
            }
            ViewBag.Client_Employee_ID_vb = db.spEmployeeSelect().Select(s => new { s.Client_Employee_ID, Client_Employee_ID2 = s.Client_Employee_ID }).Distinct().OrderBy(s => s.Client_Employee_ID).ToList();

            return View(db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).ToList());
        }

        public ActionResult Match(int? site_id, string f, int? page, string first, string last, string contains)
        {
            if (f == null) {
                f = "";
            }
            ViewBag.CurrentFilter = f;

            ViewBag.Site_ID = new SelectList(db.func_sp_get_user_sites(User.Identity.Name), "Client_Site_ID", "Client_Site_Name");
            if (first == null && last == null && contains == null)
            {
                ViewBag.MatchingNames = "";
            } else {
                ViewBag.MatchingNames = db.spEmployeeMatchBtmTblEmpCodeSelect().Where(s => s.Employee_First_Name.ToLower().StartsWith(first.ToLower()) && s.Employee_Last_Name.ToLower().StartsWith(last.ToLower()) && s.Employee_Full_Name.ToLower().Contains(contains.ToLower())).ToList();
            }

            ViewBag.count = db.spEmployeeMatchNullEmpCodeSelect().Count();
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if(f == "")
            {
                return View(db.spEmployeeMatchNullEmpCodeSelect().Where(s => s.Client_Site_ID == site_id).ToList().ToPagedList(pageNumber, pageSize));
            } else {
                return View(db.spEmployeeMatchNullEmpCodeSelect().Where(s => s.Client_Site_ID == site_id && s.Client_Employee_ID == f).Take(1).ToList().ToPagedList(pageNumber, pageSize));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Match(spEmployeeSelect_Result spEmployeeSelect_Result, int site_id, int? page)
        {
            if (ModelState.IsValid)
            {
                db.spEmployeeUpdate(site_id, null, spEmployeeSelect_Result.Client_Employee_ID, spEmployeeSelect_Result.Echo_Employee_Code);
                int pageNumber = (page ?? 1);
                return RedirectToAction("Match", new { site_id = site_id, page = pageNumber });
            }
            return View();
        }

        //json
        [HttpGet]
        public JsonResult jsonName(string s_name)
        {
            var jsonData = db.spEmployeeSelect().Where(s => s.Employee_Full_Name.ToLower().Contains(s_name.ToLower())).Select(s => s.Employee_Full_Name).Distinct().ToList();
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        // GET: Employee/Create
        public ActionResult Create(string echo_employee_code)
        {
            ViewBag.eec = echo_employee_code;
            ViewBag.Client_Site_ID = new SelectList(db.func_sp_get_user_sites(User.Identity.Name), "Client_Site_ID", "Client_Site_Name");
            ViewBag.FullName = db.spEmployeeSelect().Where(s => s.Echo_Employee_Code == echo_employee_code).Select(s => s.Employee_Full_Name).FirstOrDefault();
            return View();
        }

        // POST: Employee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Client_Employee_ID,Echo_Employee_Code,Client_Site_ID,Client_ID,Client_Site_Code,Client_Site_Name,is_Active")] spEmployeeSelect_Result spEmployeeSelect_Result, string echo_employee_code)
        {
            if (ModelState.IsValid)
            {
                db.spEmployeeInsert(spEmployeeSelect_Result.Client_Site_ID, spEmployeeSelect_Result.Client_Employee_ID, spEmployeeSelect_Result.Echo_Employee_Code);
                return RedirectToAction("Index", new { echo_employee_code = echo_employee_code });
            }

            return View(spEmployeeSelect_Result);
        }

        //// GET: Employee/Edit/5
        //public ActionResult Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    spEmployeeSelect_Result spEmployeeSelect_Result = db.spEmployeeSelect().Where(s=>s.Echo_Employee_Code==id).FirstOrDefault();
        //    if (spEmployeeSelect_Result == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(spEmployeeSelect_Result);
        //}

        //// POST: Employee/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Client_Employee_ID,Echo_Employee_Code,Client_Site_ID,Client_ID,Client_Site_Code,Client_Site_Name,is_Active")] spEmployeeSelect_Result spEmployeeSelect_Result)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(spEmployeeSelect_Result).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(spEmployeeSelect_Result);
        //}

        // GET: Employee/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    //spemployeeselect_result spemployeeselect_result = db.spemployeeselect_result.find(id);
        //    if (spEmployeeSelect_Result == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(spEmployeeSelect_Result);
        //}

        // POST: Employee/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string id)
        //{
        //    spEmployeeSelect_Result spEmployeeSelect_Result = db.spEmployeeSelect_Result.Find(id);
        //    db.spEmployeeSelect_Result.Remove(spEmployeeSelect_Result);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
