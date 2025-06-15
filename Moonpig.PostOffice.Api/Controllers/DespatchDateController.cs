namespace Moonpig.PostOffice.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using Services.Interfaces;

    [Route("api/[controller]")]
    public class DespatchDateController : Controller
    {
        private readonly IDespatchDateService _despatchDateService;

        public DespatchDateController(IDespatchDateService despatchDateService)
        {
            _despatchDateService = despatchDateService;
        }

        [HttpGet]
        public DespatchDate Get(List<int> productIds, DateTime orderDate)
        {
            var despatchDate = _despatchDateService.CalculateDespatchDate(productIds, orderDate);
            return new DespatchDate { Date = despatchDate };
        }
    }
}
