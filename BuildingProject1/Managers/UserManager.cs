using BuildingProject.Models;
using Dapper;
using System;
using System.Linq;

namespace BuildingProject.Managers
{
    public class UserManager
    {
        public static UserModel CurrentUser { get; private set; }

        public bool RegisterUser(UserModel userModel)
        {
            var connection = DbService.GetConnection();

            var userExists = connection.ExecuteScalar<bool>("select count(1) FROM Users WHERE Email = @Email", new { Email = userModel.Email });

            if (userExists)
            {
                return false;
            }

            string sql = "INSERT INTO Users (Id, FirstName, LastName, Email, PasswordHash, RoleId) Values (@Id, @FirstName, @LastName, @Email, @PasswordHash, @RoleId);";

            var affectedRowsCount = connection.Execute(sql, new { Id = Guid.NewGuid(), FirstName = userModel.FirstName, LastName = userModel.LastName, Email = userModel.Email, PasswordHash = HashService.Hash(userModel.Password), RoleId = (int)userModel.Role });

            return affectedRowsCount > 0;
        }

        public bool LoginUser(string email, string password)
        {
            var connection = DbService.GetConnection();

            var user = connection.Query<UserTable>("select * FROM Users WHERE Email = @Email", new { email }).AsList();

            if (!user.Any() || !HashService.Verify(password, user.First().PasswordHash))
            {
                return false;
            }

            CurrentUser = new UserModel(user.First());

            return true;
        }
    }
}
