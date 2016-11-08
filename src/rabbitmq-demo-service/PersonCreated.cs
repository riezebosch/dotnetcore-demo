using System.Collections.Generic;

namespace rabbitmq_demo_service
{
    public class PersonCreated
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}