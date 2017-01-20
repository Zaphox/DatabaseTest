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

                samplePerson.Buildings.Add(noHouse);
                samplePerson.Buildings.Add(oneHouse);
                samplePerson.Buildings.Add(oneGarage);
                samplePerson.Buildings.Add(noGarage);

                db.Persons.Add(samplePerson);
                db.SaveChanges();

                // At this point the DB should have a Person who owns two houses and two garages,
                // one with and one without the corresponding Garage_House relation.
            }

            using (TestContext db = new TestContext())
            {
                // Being able to get all Garages of a Person
                foreach (Person person in db.Persons.ToList())
                {
                    if (person.Buildings.OfType<Garage>().Count() != 2) // One Garage with House, one standalone Garage
                        Console.WriteLine($"Person {person.Id} is borked (mismatched Garages).");
                    if (person.Buildings.OfType<House>().Count() != 2) // Same as above
                        Console.WriteLine($"Person {person.Id} is borked (mismatched Houses).");
                }

                // Being able to access a Person from a House from a Garage
                if (db.Buildings.OfType<Garage>().Count(g => db.Buildings.OfType<House>().Count(h => h.Garage.Id == g.Id) > 0) != db.Persons.Count())
                    Console.WriteLine("Amount of Garages with a House not equal to amount of People.");
                if (db.Buildings.OfType<House>().Count(h => h.Garage != null) != db.Persons.Count())
                    Console.WriteLine("Amount of Houses with a Garage not equal to amount of People.");

                foreach (Garage garage in db.Buildings.OfType<Garage>().Where(g => db.Buildings.OfType<House>().Count(h => h.Garage.Id == g.Id) == 0).ToList())
                {
                    if (db.Persons.Where(p => p.Buildings.Count(g => g.Id == garage.Id) > 0).FirstOrDefault() == null)
                        Console.WriteLine($"Garage {garage.Id} has no associated Person.");

                    // Every person has 1 house with and 1 without a garage
                    if (db.Persons.Count(p => p.Buildings.OfType<House>().Count(b => b.Garage.Id == garage.Id) > 1) > 0)
                        Console.WriteLine($"There are people with multiple houses with no garage");
                }
                
                foreach (House house in db.Buildings.OfType<House>().Where(h => h.Garage == null).ToList())
                {
                    if (db.Persons.Where(p => p.Buildings.Count(b => b.Id == house.Id) > 0).FirstOrDefault() == null)
                        Console.WriteLine($"House {house.Id} has no associated Person.");
                }
            }
        }
    }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class TestContext : DbContext
    {
        public TestContext() : base("Server=localhost;Database=test;Uid=test;Pwd=test;") { }

        public DbSet<Person> Persons
        {
            get; set;
        }

        public DbSet<Building> Buildings
        {
            get; set;
        }
    }

    public class Person
    {
        public Person()
        {
            this.Buildings = new List<Building>();
        }
        
        public int Id
        {
            get; set;
        }

        public virtual List<Building> Buildings
        {
            get; set;
        }
    }

    public abstract class Building
    {
        public Building()
        {

        }

        public int Id
        {
            get; set;
        }


        public Person Person
        {
            get; set;
        }
    }

    public class House : Building
    {
        public House()
        {
        }

        public Garage Garage
        {
            get; set;
        }

        public string Style
        {
            get; set;
        }
    }
    
    public class Garage : Building
    {
        public Garage()
        {
        }

        public int Size
        {
            get; set;
        }
    }
}
