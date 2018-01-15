using FastiCalSync.Data;
using FastiCalSync.UI.Extensions;
using FastiCalSync.UI.Models.HomeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastiCalSync.UI.Controllers
{
    [Route(""), Authorize]
    public class HomeController : Controller
    {
        private readonly CalendarRepository repository;

        public HomeController(CalendarRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost(""), ValidateAntiForgeryToken, AllowAnonymous]
        public async Task<IActionResult> Create(string url)
        {
            // Is the URL valid?
            if (!Uri.TryCreate(url ?? string.Empty, UriKind.Absolute, out Uri _))
                return await Index();

            if (!User.Identity.IsAuthenticated)
            {
                Response.Cookies.Append("deferred-create-url", url);
                return RedirectToAction("SignIn", "Auth");
            }

            Calendar calendar = new Calendar(User.Identity.Name, url);
            await repository.Create(calendar);

            return RedirectToAction("Index");
        }

        [HttpPost("{rowKey}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string rowKey, SyncState? state)
        {
            Calendar calendar = await repository.Read(User.Identity.Name, rowKey);
            if (calendar == null)
                return RedirectToAction("Index");

            switch (state)
            {
                case SyncState.Syncing: calendar.Sync(); break;
                case SyncState.Deleting: calendar.Delete(); break;
                case SyncState.Paused: calendar.Pause(); break;
                default: throw new NotSupportedException();
            }

            await repository.Update(calendar);
            return RedirectToAction("Index");
        }

        protected async Task HandleDeferredCreate()
        {
            string url = Request.Cookies["deferred-create-url"];
            if (!Uri.TryCreate(url ?? string.Empty, UriKind.Absolute, out Uri _)) return;

            Calendar calendar = new Calendar(User.Identity.Name, url);
            await repository.Create(calendar);

            Response.Cookies.Delete("deferred-create-url");
        }

        [HttpGet(""), AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return View("MarketingIndex");

            await HandleDeferredCreate();

            var calendars = await repository.Read(User.Identity.Name);
            var model = calendars
                .OrderByDescending(m => m.CreatedTimestampUtc)
                .Select(CalendarViewModel.Create)
                .ToArray();
            return Request.IsAjaxRequest() ? PartialView("_IndexTable", model)
                : (IActionResult)View("Index", model);
        }
    }
}
