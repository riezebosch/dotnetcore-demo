using Microsoft.AspNetCore.Mvc;
using Model.Commands;
using rabbitmq_demo;
using System;

namespace Controllers
{
    public class PeopleController : Controller
    {
        private readonly ISender _sender;

        public PeopleController(ISender sender)
        {
            this._sender = sender;
        }
        [HttpPost]
        public ActionResult Create([FromBody]CreatePerson data)
        {
            _sender.PublishCommand(data);
            return RedirectToAction("Index");
        }


    }
}