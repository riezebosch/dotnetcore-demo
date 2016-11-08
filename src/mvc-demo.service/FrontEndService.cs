using mvc_demo.database;
using rabbitmq_demo;
using System.Collections.Generic;

namespace mvc_demo.service
{
    public class FrontEndService
    {
        public FrontEndService(IFrontEndContext context, IReceiver receiver)
        {
            receiver.Subscribe<PersonCreated>(p =>
            {
                context.People.Add(new Person { Id = p.Id, Name = $"{p.FirstName}, {p.LastName}" });
                context.SaveChanges();
            });
        }
    }
}
