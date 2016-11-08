using Microsoft.EntityFrameworkCore;

namespace mvc_demo.database
{
    public interface IFrontEndContext
    {
        DbSet<Person> People { get; }

        int SaveChanges();
    }
}