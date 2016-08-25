using System;
using System.Collections;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

public class DemoContext : DbContext
{
    public DemoContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Person> People { get; set; }
}