using System;
using System.Collections.Generic;
using System.Linq;

public class PeopleController
{
    private DemoContext context;

    public PeopleController(DemoContext context)
    {
        this.context = context;
    }

    public IList<Person> Get()
    {
        return context.People.ToList(); ;
    }
}