using Microsoft.AspNetCore.Mvc;
using Model.Commands;
using rabbitmq_demo;
using System;

namespace Controllers
{
    public class PeopleController : Controller
    {
        [HttpPost]
        public ActionResult Create([FromBody]CreatePerson data)
        {
            using (var sender = new Sender())
            {
                sender.Publish(data);
            }
            return RedirectToAction("Index");
        }


    }
}