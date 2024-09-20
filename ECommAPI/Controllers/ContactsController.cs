using ECommAPI.Models;
using ECommAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {

        private readonly ApplicationDbContext context;
        private readonly string senderEmail;
        private readonly string senderName;




        public ContactsController(ApplicationDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.senderEmail = configuration["BrevoApi:SenderEmail"]!;
            this.senderName = configuration["BrevoApi:SenderName"]!;


        }




        [HttpGet("subjects")]
        public IActionResult GetSubjects()
        {
            var listSubjects = context.Subjects.ToList();
            return Ok(listSubjects);
        }







        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetContacts(int? page)
        {

            if (page == null || page < 1)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = context.Contacts.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);



            //for every contact include the subject
            var contacts = context.Contacts
                .Include(c => c.Subject)
                .OrderByDescending(c => c.Id)  //get each id and order by descending
                .Skip((int)(page - 1) * pageSize)  //skip the pages before the requested page
                .Take(pageSize)
                .ToList();


            var response = new
            {
                Contacts = contacts,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };


            return Ok(response);
        }






        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public IActionResult GetContact(int id)
        {
            var contact = context.Contacts.Include(c => c.Subject).FirstOrDefault(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);


        }








        [HttpPost]
        public IActionResult CreateContact(ContactDto contactDto)
        {
            var subject = context.Subjects.Find(contactDto.SubjectId);
            if (subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }

            Contact contact = new Contact()
            {
                FirstName = contactDto.FirstName,
                LastName = contactDto.LastName,
                Email = contactDto.Email,
                Phone = contactDto.Phone ?? "",
                Subject = subject,
                Message = contactDto.Message,
                CreatedAt = DateTime.Now,
            };

            context.Contacts.Add(contact);
            context.SaveChanges();

            string successMessage = "Your message has been received successfully";

            //send confirmation email

            string receiverEmail = contactDto.Email;
            string receiverName = contactDto.FirstName + " " + contactDto.LastName;
            //string subject = "Contact Confirmation";
            string message = "Dear" + receiverName + "\n" +
                "We received your message. Thank you for contacting us. \n" +
                "Out ream will contact you as soon as we can. \n" +
                "Best regards \n\n" +
                "Your Message: \n" + contactDto.Message;

            EmailSender.SendEmail(senderEmail, senderName, receiverEmail, receiverName, "Contact Information", message);

            //clear

            contactDto.FirstName = "";
            contactDto.LastName = "";
            contactDto.Email = "";
            contactDto.Message = "";



            return Ok(contact);
        }








        [HttpPut("{id}")]
        public IActionResult UpdateContact(int id, ContactDto contactDto)
        {
            var subject = context.Subjects.Find(contactDto.SubjectId);
            if (subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }


            var contact = context.Contacts.Find(id);

            if (contact == null)
            {
                return NotFound();
            }
            contact.FirstName = contactDto.FirstName;
            contact.LastName = contactDto.LastName;
            contact.Email = contactDto.Email;
            contact.Phone = contactDto.Phone ?? "";
            contact.Subject = subject;
            contact.Message = contactDto.Message;

            context.SaveChanges();


            return Ok(contact);
        }



        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {

            //Method 1
            /*
            var contact = context.Contacts.Find(id);

            if (contact == null)
            {
                return NotFound();
            }
            context.Contacts.Remove(contact);
            context.SaveChanges();

            return Ok();
            */

            //Method 2 

            try
            {
                //the subject is "required" so it needs to be intialized

                var contact = new Contact() { Id = id, Subject = new Subject() };
                context.Contacts.Remove(contact);
                context.SaveChanges();
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Ok();

        }


    }
}
