using System;

namespace rabbitmq_demo_services
{
    public class PeopleService : IDisposable
    {
        DemoContext _context;

        public PeopleService(DemoContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Execute(CreatePerson command)
        {
            _context.People.Add(new Person { FirstName = command.FirstName, LastName = command.LastName });
            _context.SaveChanges();
        }
    }
}