using System;
using System.ServiceModel;
using System.Web.Mvc;
using ClientManager.Business;
using ClientManager.Data;
using ClientManager.Web.Models;

namespace ClientManager.Web.Controllers
{
    public class ClientsController : Controller
    {
        private const int PageSize = 25;

        // -- list / search

        public ActionResult Index(string search, int page = 1)
        {
            if (page < 1) page = 1;

            ClientPage result = ServiceGateway.Call(s => s.SearchClients(search, page, PageSize));
            int totalPages = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)PageSize));

            if (page > totalPages)  
                return RedirectToAction("Index", new { search = search, page = totalPages });

            return View(new ClientListViewModel { Search = search, Page = result, TotalPages = totalPages });
        }

        // --add / edit

        public ActionResult Create()
        {
            return ShowForm(new ClientDto());
        }

        public ActionResult Edit(int id)
        {
            ClientDto client = ServiceGateway.Call(s => s.GetClient(id));
            if (client == null) return HttpNotFound();
            return ShowForm(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(ClientDto client)
        {
            if (client == null) return RedirectToAction("Index");

            try
            {
                ServiceGateway.Call(s => s.SaveClient(client));
                TempData["Message"] = "Client saved.";
                return RedirectToAction("Index");
            }
            catch (FaultException<ServiceFault> ex)
            {
                if (ex.Detail.ValidationErrors.Count > 0)
                    foreach (string error in ex.Detail.ValidationErrors)
                        ModelState.AddModelError("", error);
                else
                    ModelState.AddModelError("", ex.Detail.Message);

                return ShowForm(client);
            }
        }

        // ------------------------------------------------------------------ delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                ServiceGateway.Call(s => s.DeleteClient(id));
                TempData["Message"] = "Client deleted.";
            }
            catch (FaultException<ServiceFault> ex)
            {
                TempData["Error"] = ServiceGateway.Describe(ex);
            }
            return RedirectToAction("Index");
        }

        // -- export
        public ActionResult Export()
        {
            string fileName = "clients_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";

            ServiceGateway.Call(s =>
            {
                using (var source = s.ExportClientsCsv())
                {
                    Response.BufferOutput = false;
                    Response.ContentType = "text/csv";
                    Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
                    source.CopyTo(Response.OutputStream);
                }
            });

            return new EmptyResult();
        }
        private ActionResult ShowForm(ClientDto client)
        {
            ViewBag.Lookups = ServiceGateway.Call(s => s.GetLookups());
            return View("Edit", client);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            System.Diagnostics.Trace.TraceError(filterContext.Exception.ToString());
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            filterContext.Result = View("Error", (object)ServiceGateway.Describe(filterContext.Exception));
        }
    }
}
