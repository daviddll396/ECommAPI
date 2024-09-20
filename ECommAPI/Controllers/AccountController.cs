using ECommAPI.Models;
using ECommAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;
        private readonly EmailSender emailSender;

        public AccountController(IConfiguration configuration, ApplicationDbContext context, EmailSender emailSender)
        {
            this.configuration = configuration;
            this.context = context;
            this.emailSender = emailSender;
        }


        [HttpPost("Register")]
        public IActionResult Register(UserDto userDto)
        {
            //check if email address is already in use or not 
            var emailCount = context.Users.Count(u => u.Email == userDto.Email);

            if (emailCount > 0)
            {
                ModelState.AddModelError("Email", "This Email is already in use");
                return BadRequest(ModelState);
            }


            //encrypt the password

            var passwordHasher = new PasswordHasher<User>();

            var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);



            //create new account

            User user = new User()
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone ?? "",
                Address = userDto.Address,
                Password = encryptedPassword,
                Role = "client",
                CreatedAt = DateTime.Now,
            };


            context.Users.Add(user);

            context.SaveChanges();


            var jwt = CreateJWToken(user);


            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = DateTime.Now

            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto,
            };

            return Ok(response);




        }









        [HttpPost("Login")]
        public IActionResult Login(string email, string password)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Email or password not valid");
                return BadRequest(ModelState);
            }

            //verify password

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(new User(), user.Password, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Password", "Wrong Password");
                return BadRequest(ModelState);
            }

            var jwt = CreateJWToken(user);


            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = DateTime.Now

            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto,
            };

            return Ok(response);
        }









        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword(string email)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return NotFound();
            }
            //delete any old password reset requests

            var oldPasswordReset = context.PasswordResets.FirstOrDefault(r => r.Email == email);

            if (oldPasswordReset != null)
            {
                context.Remove(oldPasswordReset);
            }

            //create password reset token

            string token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

            var passwordReset = new PasswordReset()
            {
                Email = email,
                Token = token,
                CreatedAt = DateTime.Now
            };


            context.PasswordResets.Add(passwordReset);

            context.SaveChanges();

            //Console.WriteLine(token);

            //send pass reset email


            //string emailSubject = "Password Reset";
            //string username = user.FirstName + " " + user.LastName;
            //string emailMessage = "Dear" + username + "\n" +
            //    "We received your password reset request. \n" +
            //    "Please copy the following token and paste it in the password reset form\n" +
            //      token + " \n\n" +
            //    "Best Regards \n";

            //emailSender.SendEmail("thestore@store.com", "The Store", email, username, "Contact Information", emailMessage);


            return Ok(token);


        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword(string token, string password)
        {
            var passwordReset = context.PasswordResets.FirstOrDefault(r => r.Token == token);

            if (passwordReset == null)
            {
                ModelState.AddModelError("Token", "Wrong or expired token");
                return BadRequest(ModelState);
            }


            var user = context.Users.FirstOrDefault(u => u.Email == passwordReset.Email);

            if (user == null)
            {
                ModelState.AddModelError("Token", "Wrong or expired token");
                return BadRequest(ModelState);
            }



            //encrypt password 


            var passwordHasher = new PasswordHasher<User>();
            string encryptedPassword = passwordHasher.HashPassword(new User(), password);



            //save the new encrypted password

            user.Password = encryptedPassword;

            //delete token 

            context.PasswordResets.Remove(passwordReset);

            context.SaveChanges();

            return Ok();

        }








        [Authorize]
        [HttpGet("Profile")]
        public IActionResult GetProfile()
        {
            int id = JwtReader.GetUserId(User);

            var user = context.Users.Find(id);
            if (user == null)
            {
                return Unauthorized();
            }
            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt,

            };

            return Ok(userProfileDto);


        }








        [Authorize]
        [HttpPut("UpdateProfile")]
        public IActionResult UpdateProfile(UserProfileUpdateDto userProfileUpdateDto)
        {
            int id = JwtReader.GetUserId(User);

            var user = context.Users.Find(id);

            if (user == null)
            {
                return Unauthorized();
            }


            //update the user profile

            user.FirstName = userProfileUpdateDto.FirstName;
            user.LastName = userProfileUpdateDto.LastName;
            user.Email = userProfileUpdateDto.Email;
            user.Phone = userProfileUpdateDto.Phone ?? "";
            user.Address = userProfileUpdateDto.Address;


            context.SaveChanges();

            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt,

            };

            return Ok(userProfileDto);


        }





        [Authorize]
        [HttpPut("UpdatePassword")]
        public IActionResult UpdatePassword([Required, MinLength(8), MaxLength(100)] string password)
        {
            int id = JwtReader.GetUserId(User);

            var user = context.Users.Find(id);

            if (user == null) { return Unauthorized(); };

            //encrypt password 


            var passwordHasher = new PasswordHasher<User>();
            string encryptedPassword = passwordHasher.HashPassword(new User(), password);



            //save the new encrypted password

            user.Password = encryptedPassword;

            context.SaveChanges();


            return Ok();


        }























        /*

                [Authorize]
                [HttpGet("AuthorizeAuthenticatedUsers")]
                public IActionResult AuthorizeAuthenticatedUsers()
                {
                    return Ok("You are authorized");
                }



                [Authorize(Roles = "admin")]
                [HttpGet("AuthorizeAdmin")]
                public IActionResult AuthorizeAdmin()
                {
                    return Ok("You are authorized");
                }


                [Authorize(Roles = "admin, seller")]
                [HttpGet("AuthorizeAdminAndSeller")]
                public IActionResult AuthorizeAdminAndSeller()
                {
                    return Ok("You are authorized");
                }


                */

        /*
                [Authorize]
                [HttpGet("GetTokenClaims")]
                public IActionResult GetTokenClaims()
                {
                    var identity = User.Identity as ClaimsIdentity;
                    if(identity != null)
                    {
                        Dictionary<string, string> claims = new Dictionary<string, string>();

                        foreach (Claim claim in identity.Claims)
                        {
                            claims.Add(claim.Type, claim.Value);
                        }

                        return Ok(claims);
                    }

                    return Ok();
                }

                */


















        /*
        [HttpGet("TestToken")]
        public IActionResult TestToken()
        {
            User user = new User()
            {
                Id = 2,
                Role = "admin"
            };
            string jwt = CreateJWToken(user);

            var response = new { JWToken = jwt };

            return Ok(response);
        }
        */

        private string CreateJWToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id","" + user.Id),
                new Claim("role", user.Role)


            };

            string strKey = configuration["JwtSettings:Key"]!;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
               signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;


        }

    }
}
