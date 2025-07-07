namespace AuthService.Application.Repositories
{
    public interface IHashingService
    {

        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    

}
}
