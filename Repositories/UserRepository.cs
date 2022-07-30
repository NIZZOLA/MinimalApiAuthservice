using DotnetAuth.Models;

namespace DotnetAuth.Repositories;

public static class UserRepository
{
    public static User Get(string userName, string passWord)
    {
        var users = new List<User>();
        users.Add(new User { Id = 1, Username = "vader", Password = "Order66", Role = RoleEnum.Employee });
        users.Add(new User { Id = 1, Username = "palpatine", Password = "Plagueis", Role = RoleEnum.Manager });
        
        return users.FirstOrDefault(x =>
            string.Equals(x.Username, userName, StringComparison.CurrentCultureIgnoreCase)
            && x.Password == passWord);
    }
}