using DataAccessLibrary;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        SqlCrud sql = new SqlCrud(GetConnectionString());
        //ReadAllContacts(sql);
        //CreateNewContact(sql);
        UpdateContact(sql);
        Console.WriteLine("done!");
    }
    private static void UpdateContact(SqlCrud sql)
    {
        BasicContactModel contact = new BasicContactModel
        {
            Id = 1002,
            FirstName = "Leah",
            LastName = "Strong"
        };
        sql.UpdateContactName(contact);
    }
    private static void CreateNewContact(SqlCrud sql)
    {
        FullContactModel user = new FullContactModel
        {
            BasicInfo = new BasicContactModel
            {
                FirstName = "Lea",
                LastName = "Hay"
            }
        };
        user.PhoneNumbers.Add(new PhoneNumberModel() {PhoneNumber = "09-876543" });
        user.EmailAddresses.Add(new EmailAddressModel() { EmailAddress = "lea.hay@mamamia.com" });
        sql.CreateContact(user);
    }
    private static void ReadAllContacts(SqlCrud sql)
    {
        var rows = sql.GetAllContacts();
        foreach (var row in rows)
        {
            Console.WriteLine($"{row.Id}: {row.FirstName} {row.LastName}");
        }
    }
    private static void ReadContact(SqlCrud sql, int contactId)
    {
        var contact = sql.GetFullContactById(contactId);
        Console.WriteLine($"{contact.BasicInfo.Id}: {contact.BasicInfo.FirstName} {contact.BasicInfo.LastName}");
    }
    private static string GetConnectionString(string connectionString = "Default")
    {
        string output = "";
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
        var config = builder.Build();
        output = config.GetConnectionString(connectionString);
        return output;
    }

}