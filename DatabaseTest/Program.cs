using MySql.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Couple of demonstrations of the capabilities the relations should have
            using (TestContext db = new TestContext())
            {
                Person samplePerson = new Person();
                House noGarage = new House();
                Garage noHouse = new Garage();
                
                House oneGarage = new House();
                Garage oneHouse = new Garage();
                oneGarage.Garage = oneHouse;
                oneHouse.House = oneGarage;

                samplePerson.Garages.Add(noHouse);
                samplePerson.Garages.Add(oneHouse);

                samplePerson.Houses.Add(noGarage);
                samplePerson.Houses.Add(oneGarage);

                db.Persons.Add(samplePerson);
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (DbEntityValidationResult res in ex.EntityValidationErrors)
                    {
                        Console.WriteLine(res.Entry);
                    }
                }

                // At this point the DB should have a Person who owns two houses and two garages,
                // one with and one without the corresponding Garage_House relation.
            }

            using (TestContext db = new TestContext())
            {
                // Being able to get all Garages of a Person
                foreach (Person person in db.Persons)
                {
                    if (person.Garages.Count != 2) // One Garage with House, one standalone Garage
                        Console.WriteLine($"Person {person.Id} is borked (mismatched Garages).");
                    if (person.Houses.Count != 2) // Same as above
                        Console.WriteLine($"Person {person.Id} is borked (mismatched Houses).");
                }

                // Being able to access a Person from a House from a Garage
                if (db.Garages.Count(g => g.House != null) == db.Persons.Count())
                    Console.WriteLine("Amount of Garages with a House not equal to amount of People.");
                if (db.Houses.Count(h => h.Garage != null) == db.Persons.Count())
                    Console.WriteLine("Amount of Houses with a Garage not equal to amount of People.");

                foreach (Garage garage in db.Garages.ToList().Where(g => g.House != null))
                {
                    if (garage.Person == null)
                        Console.WriteLine($"Garage {garage.Id} has no associated Person.");

                    if (garage.House.Person == null)
                        Console.WriteLine($"House {garage.House.Id} has no associated Person.");
                }

                // Having Houses and Garages be optional
                if (db.Garages.Count(g => g.House == null) == db.Persons.Count())
                    Console.WriteLine("Amount of Garages without a House not equal to amount of People.");

                if (db.Houses.Count(h => h.Garage == null) == db.Persons.Count())
                    Console.WriteLine("Amount of Houses without a Garage not equal to amount of People.");

                foreach (House house in db.Houses.Where(h => h.Garage == null))
                {
                    if (house.Person == null)
                        Console.WriteLine($"House {house.Id} has no associated Person.");
                }
            }
        }
    }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class TestContext : DbContext
    {
        public TestContext() : base("Server=localhost;Database=test;Uid=test;Pwd=test;") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<House>()
                .HasOptional(h => h.Garage)
                .WithOptionalPrincipal()
                .Map(m => m.MapKey("House_Id"));

            modelBuilder.Entity<Garage>()
                .HasOptional(g => g.House)
                .WithOptionalPrincipal()
                .Map(m => m.MapKey("Garage_Id"));
        }

        public DbSet<Person> Persons
        {
            get; set;
        }

        public DbSet<Garage> Garages
        {
            get; set;
        }

        public DbSet<House> Houses
        {
            get; set;
        }
    }

    public class Person
    {
        public Person()
        {
            this.Houses = new List<House>();
            this.Garages = new List<Garage>();
        }

        public int Id
        {
            get; set;
        }

        public virtual List<House> Houses
        {
            get; set;
        }

        public virtual List<Garage> Garages
        {
            get; set;
        }
    }

    public class House
    {
        public House()
        {

        }
        
        public int Id
        {
            get; set;
        }

        public virtual Garage Garage
        {
            get; set;
        }

        public virtual Person Person
        {
            get; set;
        }
    }
    
    public class Garage
    {
        public Garage()
        {

        }
        
        public int Id
        {
            get; set;
        }

        public House House
        {
            get; set;
        }

        public virtual Person Person
        {
            get; set;
        }
    }
}
