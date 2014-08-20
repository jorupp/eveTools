using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EveAI.Live;
using EveTools.Domain;

namespace EveTools.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiKeyRepository _apiKeyRepository;

        public HomeController(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository;
        }


        // GET: Home
        public ActionResult Index()
        {
            var keys = _apiKeyRepository.GetAll();

            return View(keys);
        }
    }
}