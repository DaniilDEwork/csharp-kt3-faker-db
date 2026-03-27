using System;
using System.Collections.Generic;
using Bogus;
using Microsoft.Data.Sqlite;

class User
{
    public string FullName { get; set; } = "";
    public int Age { get; set; }
    public string Email { get; set; } = "";
}

class AgeException : Exception
{
    public AgeException(string message) : base(message)
    {
    }
}

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source=users.db";

        PrepareDatabase(connectionString);

        List<User> users = GetUsers();

        users[0].Age = 12;
        users[1].Age = 13;

        for (int i = 0; i < users.Count; i++)
        {
            try
            {
                RegisterUser(users[i], connectionString);
                Console.WriteLine("Добавлен пользователь: " + users[i].FullName + ", возраст: " + users[i].Age);
            }
            catch (AgeException ex)
            {
                Console.WriteLine("Ошибка регистрации: " + users[i].FullName + ". " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка базы данных: " + ex.Message);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Пользователи в базе данных:");
        ShowUsers(connectionString);

        Console.ReadLine();
    }

    static List<User> GetUsers()
    {
        Faker<User> faker = new Faker<User>("ru")
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Age, f => f.Random.Int(14, 60))
            .RuleFor(u => u.Email, f => f.Internet.Email());

        return faker.Generate(10);
    }

    static void PrepareDatabase(string connectionString)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string createTableQuery =
                "CREATE TABLE IF NOT EXISTS Users (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "FullName TEXT NOT NULL, " +
                "Age INTEGER NOT NULL, " +
                "Email TEXT NOT NULL)";

            SqliteCommand createCommand = new SqliteCommand(createTableQuery, connection);
            createCommand.ExecuteNonQuery();

            string clearTableQuery = "DELETE FROM Users";
            SqliteCommand clearCommand = new SqliteCommand(clearTableQuery, connection);
            clearCommand.ExecuteNonQuery();
        }
    }

    static void RegisterUser(User user, string connectionString)
    {
        if (user.Age < 14)
        {
            throw new AgeException("Регистрация запрещена для пользователей младше 14 лет");
        }

        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string insertQuery =
                "INSERT INTO Users (FullName, Age, Email) " +
                "VALUES (@fullName, @age, @email)";

            SqliteCommand command = new SqliteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@fullName", user.FullName);
            command.Parameters.AddWithValue("@age", user.Age);
            command.Parameters.AddWithValue("@email", user.Email);
            command.ExecuteNonQuery();
        }
    }

    static void ShowUsers(string connectionString)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string selectQuery = "SELECT Id, FullName, Age, Email FROM Users";
            SqliteCommand command = new SqliteCommand(selectQuery, connection);
            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine(
                    reader["Id"] + ". " +
                    reader["FullName"] + " | " +
                    reader["Age"] + " лет | " +
                    reader["Email"]);
            }
        }
    }
}