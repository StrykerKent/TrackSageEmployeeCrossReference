using ClosedXML.Excel;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Track_XRref.Models;

namespace Track_XRref.Controllers
{
    public class ContractController : Controller
    {
        private SageBillingEntities db = new SageBillingEntities();

        // GET: Contract
        public ActionResult Index(int? clientsiteid, int? clientcontractid, string ponumber, string sagejob, string issuedoa)
        {
            /* Original way to populate data before stored procedures
            var contract_PO_Line = db.Contract_PO_Line.Include(c => c.Billing_Method).Include(c => c.Client_Contract);
            return View(contract_PO_Line.ToList());*/

            //dates with yy throw error...this converts them to yyyy...also tries to parse date data, if it can't, it changes date to 1/1/2001...also if string is null, length throws an error
            if (issuedoa != null && issuedoa != "") {
                if (issuedoa.Length < 10) {
                    DateTime issuedoa_date;
                    if (DateTime.TryParse(issuedoa, out issuedoa_date)) {
                        issuedoa = issuedoa_date.ToString("MM/dd/yyyy");
                    } else {
                        issuedoa = "1/1/2001";
                    }
                }
            }

            //send username to procedure. procedure converts username to sid and sends back sid and 1 client id the user is allowed to see.
            var get_user_sites = db.func_sp_get_user_sites(curr_user()).FirstOrDefault();
            //access denied if user does not authenticate//redirect to access denied picture
            if (get_user_sites == null) {
                return Redirect("http://access-denied.ca/img/logo-accessdenied.png");
            }
            //if no clientsite picked yet, pick the first one associated with his username in database
            if (clientsiteid == null) {
                clientsiteid = get_user_sites.Client_Site_ID;
            }

            //2 lines below are to populate dropdowns (client and contract). Client_Site_ID takes the sid number and shows all clients that user has security for in the dropdown.
            //old way before procedure for get user sites //ViewBag.clientsiteid = new SelectList(db.User_Site_Security.Where(a => a.Suser_SID == get_user_sites.Suser_SID).Include(s => s.Client_Site), "Client_Site.Client_Site_ID", "Client_Site.Client_Site_Name", clientsiteid);
            ViewBag.clientsiteid = new SelectList(db.func_sp_get_user_sites(curr_user()), "Client_Site_ID", "Client_Site_Name", clientsiteid);
            ViewBag.clientcontractid = new SelectList(db.Client_Contract.Where(a => a.Client_Site_ID == clientsiteid), "Client_Contract_ID", "Contract_Code", clientcontractid);

            //this is just to grab the clientsitename and contractor code to display on index page. //delete. not in use now.
            ViewBag.clientsitename = db.Client_Site.Where(s => s.Client_Site_ID == clientsiteid).Select(s => s.Client_Site_Name).FirstOrDefault();
            ViewBag.clientcontractcode = db.Client_Contract.Where(s => s.Client_Contract_ID == clientcontractid).Select(s => s.Contract_Code).FirstOrDefault();

            //used to populate the url parameters
            ViewBag.csid = clientsiteid;
            ViewBag.ccid = clientcontractid;
            ViewBag.pon = ponumber;
            ViewBag.sj = sagejob;
            ViewBag.ioa = issuedoa;

            //pull data from stored procedure
            var sp_po_line_query = db.func_sp_po_line_query(clientsiteid, clientcontractid, ponumber, sagejob, issuedoa);
            //count rows returned... from stored procedure
            ViewBag.rowcount = db.func_sp_po_line_query(clientsiteid, clientcontractid, ponumber, sagejob, issuedoa).Count();
            return View(sp_po_line_query.ToList());
        }

        [HttpPost]
        public ActionResult Index(int clientsiteid, int? clientcontractid, string ponumber, string sagejob, string issuedoa)
        {
            //dates with yy throw error...this converts them to yyyy...also tries to parse date data, if it cant, it changes date to 1/1/2001...also if string is null, length throws an error
            if (issuedoa != null && issuedoa != "")
            {
                if (issuedoa.Length < 10)
                {
                    DateTime issuedoa_date;
                    if (DateTime.TryParse(issuedoa, out issuedoa_date))
                    {
                        issuedoa = issuedoa_date.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        issuedoa = "1/1/2001";
                    }
                }
            }

            //ViewBag.clientsiteid = new SelectList(db.User_Site_Security.Where(a => a.Suser_SID == get_user_sites.Suser_SID).Include(s => s.Client_Site), "Client_Site.Client_Site_ID", "Client_Site.Client_Site_Name", clientsiteid);
            ViewBag.clientsiteid = new SelectList(db.func_sp_get_user_sites(curr_user()), "Client_Site_ID", "Client_Site_Name", clientsiteid);
            ViewBag.clientcontractid = new SelectList(db.Client_Contract.Where(a => a.Client_Site_ID == clientsiteid), "Client_Contract_ID", "Contract_Code", clientcontractid);

            //used to populate the url parameters
            ViewBag.csid = clientsiteid;
            ViewBag.ccid = clientcontractid;
            ViewBag.pon = ponumber;
            ViewBag.sj = sagejob;
            ViewBag.ioa = issuedoa;

            var sp_po_line_query = db.func_sp_po_line_query(clientsiteid, clientcontractid, ponumber, sagejob, issuedoa);
            ViewBag.rowcount = db.func_sp_po_line_query(clientsiteid, clientcontractid, ponumber, sagejob, issuedoa).Count();
            return View(sp_po_line_query.ToList());
        }

        // GET: Contract/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contract_PO_Line contract_PO_Line = db.Contract_PO_Line.Find(id);
            if (contract_PO_Line == null)
            {
                return HttpNotFound();
            }
            return View(contract_PO_Line);
        }

        // GET: Contract/Create
        public ActionResult Create(int clientsiteid, int? clientcontractid, string sort, string sortdir)
        {
            ViewBag.so = sort;
            ViewBag.sod = sortdir;
            ViewBag.clientsiteid = clientsiteid;
            ViewBag.clientcontractid = clientcontractid;
            ViewBag.PO_Line_Billing_Method_ID = new SelectList(db.Billing_Method, "Billing_Method_ID", "Billing_Method_Code");
            ViewBag.Client_Contract_ID = new SelectList(db.Client_Contract.Where(s=>s.Client_Site_ID==clientsiteid), "Client_Contract_ID", "Contract_Code", clientcontractid);
            ViewBag.PO_Line_Status_ID = new SelectList(db.Ref_PO_Line_Status, "PO_Line_Status_ID", "PO_Line_Status");
            return View();
        }

        // POST: Contract/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Contract_PO_ID,Client_Contract_ID,Sage_Job,PO_Number,PO_Line,PO_Line_Description,PO_Line_Issued_Date,PO_Line_Billing_Method_ID,Echo_Contact,Customer_Contact,PO_Line_Approved_Amount_Orig,PO_Line_Approved_Amount_Revised,PO_Line_Percent_Work_Completed,PO_Line_Status_ID,PO_Line_Notes")] Contract_PO_Line contract_PO_Line,int clientsiteid, int? clientcontractid, string sort, string sortdir)
        {
            if (ModelState.IsValid)
            {
                contract_PO_Line.Sage_Job = contract_PO_Line.Sage_Job.ToUpper();
                db.Contract_PO_Line.Add(contract_PO_Line);
                db.SaveChanges();
                if (sort == "" || sort == null) { sort = "Sage_Job"; }
                if (sortdir == "" || sortdir == null) { sortdir = "ASC"; }
                return RedirectToAction("Index", new { sort = sort, sortdir = sortdir, ponumber = contract_PO_Line.PO_Number, clientsiteid= clientsiteid, clientcontractid= clientcontractid, _id = contract_PO_Line.Contract_PO_ID });
            }
            ViewBag.so = sort;
            ViewBag.sod = sortdir;
            ViewBag.clientsiteid = clientsiteid;
            ViewBag.clientcontractid = clientcontractid;
            ViewBag.PO_Line_Billing_Method_ID = new SelectList(db.Billing_Method, "Billing_Method_ID", "Billing_Method_Code", contract_PO_Line.PO_Line_Billing_Method_ID);
            ViewBag.Client_Contract_ID = new SelectList(db.Client_Contract, "Client_Contract_ID", "Contract_Code", contract_PO_Line.Client_Contract_ID);
            ViewBag.PO_Line_Status_ID = new SelectList(db.Ref_PO_Line_Status, "PO_Line_Status_ID", "PO_Line_Status");
            return View(contract_PO_Line);
        }

        // GET: Contract/Edit/5
        public ActionResult Edit(int? id, int? clientsiteid, int? clientcontractid, string ponumber, string sagejob, string sort, string sortdir, string issuedoa)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contract_PO_Line contract_PO_Line = db.Contract_PO_Line.Find(id);
            if (contract_PO_Line == null)
            {
                return HttpNotFound();
            }
            ViewBag.pon = ponumber;
            ViewBag.sj = sagejob;
            ViewBag.so = sort;
            ViewBag.sod = sortdir;
            ViewBag.ioa = issuedoa;
            ViewBag.clientsiteid = clientsiteid;
            ViewBag.clientcontractid = clientcontractid;
            ViewBag.PO_Line_Billing_Method_ID = new SelectList(db.Billing_Method, "Billing_Method_ID", "Billing_Method_Code", contract_PO_Line.PO_Line_Billing_Method_ID);
            ViewBag.Client_Contract_ID = new SelectList(db.Client_Contract.Where(s=>s.Client_Site_ID== clientsiteid), "Client_Contract_ID", "Contract_Code", contract_PO_Line.Client_Contract_ID);
            ViewBag.PO_Line_Status_ID = new SelectList(db.Ref_PO_Line_Status, "PO_Line_Status_ID", "PO_Line_Status", contract_PO_Line.PO_Line_Status_ID);
            return View(contract_PO_Line);
        }

        // POST: Contract/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Contract_PO_ID,Client_Contract_ID,Sage_Job,PO_Number,PO_Line,PO_Line_Description,PO_Line_Issued_Date,PO_Line_Billing_Method_ID,Echo_Contact,Customer_Contact,PO_Line_Approved_Amount_Orig,PO_Line_Approved_Amount_Revised,PO_Line_Percent_Work_Completed,PO_Line_Status_ID,PO_Line_Attachment,PO_Line_Notes")] Contract_PO_Line contract_PO_Line, int? clientsiteid, int? clientcontractid, string ponumber, string sagejob, string sort, string sortdir, string issuedoa)
        {
            //strip amount values of commas
            var cats = contract_PO_Line.PO_Line_Approved_Amount_Orig.ToString();
            
            if (ModelState.IsValid)
            {
                contract_PO_Line.Sage_Job = contract_PO_Line.Sage_Job.ToUpper();
                db.Entry(contract_PO_Line).State = EntityState.Modified;
                db.SaveChanges();
                if (sort == "" || sort == null) { sort = "none"; }
                if (sortdir == "" || sortdir == null) { sortdir = "ASC"; }
                return RedirectToAction("Index", new { sort = sort, sortdir = sortdir, sagejob = sagejob, ponumber = ponumber, issuedoa = issuedoa, clientsiteid = clientsiteid, clientcontractid = clientcontractid, _id = contract_PO_Line.Contract_PO_ID });
            }
            
            ViewBag.PO_Line_Billing_Method_ID = new SelectList(db.Billing_Method, "Billing_Method_ID", "Billing_Method_Code", contract_PO_Line.PO_Line_Billing_Method_ID);
            ViewBag.Client_Contract_ID = new SelectList(db.Client_Contract, "Client_Contract_ID", "Contract_Code", contract_PO_Line.Client_Contract_ID);
            ViewBag.PO_Line_Status_ID = new SelectList(db.Ref_PO_Line_Status, "PO_Line_Status_ID", "PO_Line_Status", contract_PO_Line.PO_Line_Status_ID);
            return View(contract_PO_Line);
        }

        // Export To Excel
        public FileResult ExportExcel(string ponumber, string sagejob, string issuedoa, int? clientsiteid, int? clientcontractid)
        {
            /* Original way to populate data
            var contract_PO_Line = db.Contract_PO_Line.Include(c => c.Billing_Method).Include(c => c.Client_Contract);
            return View(contract_PO_Line.ToList());*/

            //dates with yy throw error...this converts them to yyyy...also tries to parse date data, if it cant, it changes date to 1/1/2001...also if string is null, length throws an error
            if (issuedoa != null && issuedoa != "")
            {
                if (issuedoa.Length < 10)
                {
                    DateTime issuedoa_date;
                    if (DateTime.TryParse(issuedoa, out issuedoa_date))
                    {
                        issuedoa = issuedoa_date.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        issuedoa = "1/1/2001";
                    }
                }
            }

            //pull data from stored procedure
            var sp_po_line_query = db.func_sp_po_line_query(clientsiteid, clientcontractid, ponumber, sagejob, issuedoa);

            DataTable dt = new DataTable("TrackXref");
            dt.Columns.AddRange(new DataColumn[9] { new DataColumn("Sage Job"),
                                            new DataColumn("PO Number"),
                                            new DataColumn("PO Line"),
                                            new DataColumn("Line Description"),
                                            new DataColumn("Issued Date"),
                                            new DataColumn("Echo Contact"),
                                            new DataColumn("Customer Contact"),
                                            new DataColumn("Approved Amount Original"),
                                            new DataColumn("Approved Amount Revised") });

            foreach (var po_line in sp_po_line_query)
            {
                dt.Rows.Add(po_line.Sage_Job, po_line.PO_Number, po_line.PO_Line, po_line.PO_Line_Description, po_line.PO_Line_Issued_Date, po_line.Echo_Contact, po_line.Customer_Contact, po_line.PO_Line_Approved_Amount_Orig, po_line.PO_Line_Approved_Amount_Revised);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TrackXRef_"+ DateTime.Now.ToString().Replace(" ","_") + ".xlsx");
                }
            }

        }

        // GET: Contract/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contract_PO_Line contract_PO_Line = db.Contract_PO_Line.Find(id);
            if (contract_PO_Line == null)
            {
                return HttpNotFound();
            }
            return View(contract_PO_Line);
        }

        // POST: Contract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contract_PO_Line contract_PO_Line = db.Contract_PO_Line.Find(id);
            db.Contract_PO_Line.Remove(contract_PO_Line);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        // Global
        public string curr_user()
        {
            //get username ex. echogroup\stryker.stinnette
            return User.Identity.Name.ToString();
        }

        //dates with yy throw error...this converts them to yyyy...also tries to parse date data, if it cant, it changes date to 1/1/2001...also if string is null, length throws an error
        //public string issuedoa_fix(string issuedoa) //not in use...//todo:fix and implement
        //{
        //    if (issuedoa != null && issuedoa != "")
        //    {
        //        if (issuedoa.Length < 10)
        //        {
        //            DateTime issuedoa_date;
        //            if (DateTime.TryParse(issuedoa, out issuedoa_date))
        //            {
        //                return issuedoa = issuedoa_date.ToString("MM/dd/yyyy");
        //            }
        //            else
        //            {
        //                return issuedoa = "1/1/2001";
        //            }
        //        }
        //        return "";
        //    }
        //    return "";
        //}
    }
}
