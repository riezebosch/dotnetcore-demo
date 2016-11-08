using ef_demo;
using rabbitmq_demo;
using System;

namespace rabbitmq_demo_service
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
            var person = new Person { FirstName = command.FirstName, LastName = command.LastName };
            _context.People.Add(person);
            _context.SaveChanges();

            using (var sender = new Sender())
            {
                sender.Publish(new PersonCreated
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName
                });
            }
        }
    }
}