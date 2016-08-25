using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

[Route("api/[controller]")]
public class PeopleController : Controller
{
    private DemoContext context;

    public PeopleController(DemoContext context)
    {
        this.context = context;
        
        if (this.context.Database.EnsureCreated())
        {
            context.People.Add(new Person { FirstName = "Test", LastName = "Man" });
            context.SaveChanges();
        }
    }

    [HttpGet]
    public IList<Person> Get()
    {
        return context.People.ToList(); ;
    }
}