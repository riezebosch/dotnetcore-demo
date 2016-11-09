using mvc_demo.database;
using rabbitmq_demo;
using System.Collections.Generic;
using System;

namespace mvc_demo.service
{
    public class FrontEndService : IReceive<PersonCreated>
    {
        private readonly IFrontEndContext _context;

        public FrontEndService(IFrontEndContext context)
        {
            this._context = context;
        }

        public void Execute(PersonCreated p)
        {
            _context.People.Add(new Person { Id = p.Id, Name = $"{p.FirstName}, {p.LastName}" });
            _context.SaveChanges();
        }
    }
}
