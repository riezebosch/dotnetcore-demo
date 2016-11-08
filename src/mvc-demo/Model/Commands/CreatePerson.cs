using System.Collections.Generic;

namespace Model.Commands
{
    public class CreatePerson
    {
        public IEnumerable<char> FirstName { get; set; }
        public IEnumerable<char> LastName { get; set; }
    }
}