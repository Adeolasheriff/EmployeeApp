using System.ComponentModel.DataAnnotations;

namespace EmployeeAdminPortal.Model.Entity
{
    public class AddEmployee
    {
        public required string Name { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]

        public required string Password { get; set; }
    }
}
