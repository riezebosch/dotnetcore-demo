using ef_demo;
using System;
using System.Collections.Generic;

namespace rabbitmq_demo.tests
{
    internal class PersonComparer : IEqualityComparer<Person>
    {
        public bool Equals(Person x, Person y)
        {
            return x.FirstName == y.FirstName && x.LastName == y.LastName;
        }

        public int GetHashCode(Person obj)
        {
            throw new NotImplementedException();
        }
    }
}