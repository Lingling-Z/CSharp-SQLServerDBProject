using DataAccessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataAccessLibrary
{
    public class SqlCrud
    {
        private string _connectionString;
        private SqlDataAccess db = new();

        public SqlCrud(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<BasicContactModel> GetAllContacts()
        {
            string sql = "select id, FirstName, LastName from dbo.Contact";
            return db.LoadData<BasicContactModel, dynamic>(sql, new { }, _connectionString);
        }
        public FullContactModel GetFullContactById(int id)
        {
            string sql = "select id, FirstName, LastName from dbo.Contact where id=@Id";
            FullContactModel output = new FullContactModel();

            output.BasicInfo = db.LoadData<BasicContactModel, dynamic>(sql, new { Id = id }, _connectionString).FirstOrDefault();

            sql = @"select e.*
                    from dbo.EmailAddresses e
                    inner join dbo.ContactEmail ce on ce.EmailAdressId = e.Id
                    where ce.ContactId = @Id";
            output.EmailAddresses = db.LoadData<EmailAddressModel, dynamic>(sql, new { Id = id }, _connectionString);

            sql = @"select p.*
                    from dbo.PhoneNumbers p
                    inner join dbo.ContactPhoneNumber cp on cp.PhoneNumberId = p.Id
                    where cp.ContactId = @Id";
            output.PhoneNumbers = db.LoadData<PhoneNumberModel, dynamic>(sql, new { Id = id }, _connectionString);

            return output;
        }
        public void CreateContact(FullContactModel contact)
        {
            //save basic contact
            string sql = "insert into dbo.Contact (FirstName, LastName) values (@FirstName, @LastName);";
            db.SaveData(sql, new {contact.BasicInfo.FirstName, contact.BasicInfo.LastName }, _connectionString);
            //get the ID number of the contact
            sql = "select Id from dbo.Contact where FirstName = @FirstName and LastName = @LastName;";
            int contactId = db.LoadData<IdLookupModel, dynamic>(sql, new { contact.BasicInfo.FirstName, contact.BasicInfo.LastName }, _connectionString)
                              .First().Id;
            //Identify if the phone number exists
            //if not, insert into the phone number table, and get the id, then insert into the link table
            //insert into the link table
            foreach (var phoneNumber in contact.PhoneNumbers)
            {
                if(phoneNumber.Id == 0)
                {
                    sql = "insert into dbo.PhoneNumbers (PhoneNumber) values (@PhoneNumber);";
                    db.SaveData(sql, new { phoneNumber.PhoneNumber }, _connectionString);

                    sql = "select Id from dbo.PhoneNumbers where PhoneNumber = @PhoneNumber;";
                    phoneNumber.Id = db.LoadData<IdLookupModel, dynamic>(sql, new { phoneNumber.PhoneNumber }, _connectionString).First().Id;
                }
                sql = "insert into dbo.ContactPhoneNumber (ContactId, PhoneNumberId) values (@ContactId, @PhoneNumberId);";
                db.SaveData(sql, new { ContactId = contactId, PhoneNumberId = phoneNumber.Id}, _connectionString);
            }
            //do the same for email
            foreach (var email in contact.EmailAddresses)
            {
                if(email.Id == 0)
                {
                    sql = "insert into dbo.EmailAddresses (EmailAddress) values (@EmailAddress)";
                    db.SaveData(sql, new {email.EmailAddress }, _connectionString);

                    sql = "select Id from dbo.EmailAddresses where EmailAddress = @EmailAddress";
                    email.Id = db.LoadData<IdLookupModel, dynamic>(sql, new {email.EmailAddress}, _connectionString).First().Id;
                }
                sql = "insert into dbo.ContactEmail (ContactId, EmailAddressId) values (@ContactId, @EmailAddressId);";
                db.SaveData(sql, new {ContactId = contactId, EmailAddressId = email.Id}, _connectionString);
            }
        }
        public void UpdateContactName(BasicContactModel contact)
        {
            string sql = "update dbo.Contact set FirstName = @FirstName, LastName = @LastName where Id = @Id;";
            db.SaveData(sql, contact, _connectionString);
        }
    }
}
