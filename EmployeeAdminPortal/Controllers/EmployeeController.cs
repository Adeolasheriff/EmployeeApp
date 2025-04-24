using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Model.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly Jwt _Jwt;

        public EmployeeController(AppDbContext appDbContext, Jwt jwt)
        {
            _appDbContext = appDbContext;
            _passwordHasher = new PasswordHasher<User>();
            _Jwt = jwt;


        }

        [HttpPost("SignUp")]

        public async Task<IActionResult> SignUp(AddEmployee addEmployee)
        {
            try
            {
                var employee = _appDbContext.Users.FirstOrDefault(c => c.Email == addEmployee.Email);
                if (employee != null)
                {
                    return StatusCode(403, "user already exists");
                }

                var newEmployee = new User()
                {
                    Name = addEmployee.Name,
                    Email = addEmployee.Email,
                    Password = addEmployee.Password,
                    IsVerified = false,    //user not verified yet
                    verificationToken = Guid.NewGuid().ToString() //generate a unique token for the user
                };
                var hashed = _passwordHasher.HashPassword(newEmployee, newEmployee.Password);
                newEmployee.Password = hashed;
                var verificationToken = _Jwt.GenerateJwtToken(newEmployee);
                await _appDbContext.Users.AddAsync(newEmployee); // add users to d db
                await _appDbContext.SaveChangesAsync();  //  saves details to the db
                return Ok(new
                {
                    message = "user created successfully,Please check your email for verification code",
                    newEmployee.verificationToken   // saves token to the db
                });
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("Verify-User")]

        public async Task<IActionResult> VerifyUser(string Email, string token)
        {
            try
            {
                var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == Email);
                if (user == null)
                {
                    return BadRequest("incorrect password or email");
                }
                // if user token is not equal to the token the user is inputting return incorrect token
                if (user.verificationToken != token)
                {
                    Unauthorized("Incorrect token");
                }
                // if token is correct
                user.IsVerified = true;
                user.verificationToken = null; //remove the token after verification
                await _appDbContext.SaveChangesAsync();
                return Ok(new
                {
                    message = "user verified successfully",

                    
                });
            }




            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpPost("SignIn")]
        public async Task<IActionResult> Login(SignIn signIn)
        {
            try
            {
                var findEmployee = await _appDbContext.Users.FirstOrDefaultAsync(c => c.Email == signIn.Email);
                if (findEmployee == null)
                {
                    return NotFound("user not found in the database");
                }
                if(!findEmployee.IsVerified)
                {
                    return Unauthorized("user not verified");

                }

                var verifyHashedPassword = _passwordHasher.VerifyHashedPassword(findEmployee, findEmployee.Password, signIn.Password);
                if (verifyHashedPassword == PasswordVerificationResult.Failed)
                {
                    return Unauthorized("incorrect password");
                }

                if (verifyHashedPassword == PasswordVerificationResult.Success)
                {
                    var token = _Jwt.GenerateJwtToken(findEmployee);
                    return Ok(new
                    {
                        token,
                        message = "Login successful"
                    });
                }

                return Ok();



            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpGet]
        [Route("{Id:guid}")]


        public async Task<IActionResult> GetById(Guid Id)
        {
            try
            {
                var findEmployee = await _appDbContext.Users.FirstOrDefaultAsync(c => c.Id == Id);
                if (findEmployee == null)
                {
                    return StatusCode(401, "user not found");
                }

                return StatusCode(200, findEmployee);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployee()
        {
            try
            {
                var employee = await _appDbContext.Users.ToListAsync();
                if (employee == null)
                {
                    return StatusCode(401, "user not found");
                }
                return StatusCode(200, employee);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("{Id:guid}")]
        public async Task<IActionResult> UpdateEmployee(updateEmployee update, Guid Id)
        {
            try
            {
                var employee = await _appDbContext.Users.FirstOrDefaultAsync(c => c.Id == Id);
                if (employee == null)
                {
                    return StatusCode(401, "user not found");
                }
                employee.Name = update.Name;
                employee.Email = update.Email;
                // only rehashed password if user enter a new password
                if (!string.IsNullOrWhiteSpace(update.Password))
                {

                 
                    var hashed = _passwordHasher.HashPassword(employee, update.Password);
                    employee.Password = hashed;
                }

                _appDbContext.Users.Update(employee);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(200, "user updated successfully");

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [HttpDelete]
        [Route("{Id:guid}")]

        public async Task<IActionResult> DeleteEmployee(Guid Id)
        {
            try
            {
                var employee = await _appDbContext.Users.FirstOrDefaultAsync(c => c.Id == Id);
                if (employee == null)
                {
                    return StatusCode(401, "user not found");
                }
                _appDbContext.Users.Remove(employee);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(200, "user deleted successfully");
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}




