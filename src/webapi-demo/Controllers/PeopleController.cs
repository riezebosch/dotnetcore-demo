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
        this.context.Database.EnsureCreated();
    }

    [HttpGet]
    public IList<Person> Get()
    {
        return context.People.ToList(); ;
    }
}