namespace TaskManagementAPI.DTO
{
    public class UserCreateDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool IsSuperUser { get; set; }
    }
}
