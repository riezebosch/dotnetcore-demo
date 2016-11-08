using Microsoft.AspNetCore.Mvc;
using Model.Commands;
using rabbitmq_demo;
using System;

namespace Controllers
{
    public class PeopleController : Controller
    {
        [HttpPost]
        public ActionResult Create(string first, string last)
        {
            using (var sender = new Sender())
            {
                sender.Publish(new CreatePerson { FirstName = first, LastName = last });
            }
            return RedirectToAction("Index");
        }


    }
}