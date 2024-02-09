using Dapper;
using Microsoft.Data.SqlClient;
using System;

namespace HomeWork8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Server=localhost;Database=TaskDb;Trusted_Connection=True;TrustServerCertificate=True;";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var isExists = connection.ExecuteScalar<int>(@"
                    IF OBJECT_ID('Tasks', 'U') IS NOT NULL
                        SELECT 1
                    ELSE
                        SELECT 0
                ");

                if (isExists == 0)
                {
                    connection.Execute(@"
                        CREATE TABLE [Tasks] (
                            [Id] INT PRIMARY KEY IDENTITY,
                            [Title] NVARCHAR(255),
                            [Description] NVARCHAR(MAX),
                            [DueDate] DATETIME,
                            [IsCompleted] BIT
                        );
                    ");
                    connection.Execute(@"
                        INSERT INTO [Tasks] ([Title], [Description], [DueDate], [IsCompleted]) 
                        VALUES 
                            ('Task 1', 'Description 1', GETDATE(), 0),
                            ('Task 2', 'Description 2', GETDATE(), 1),
                            ('Task 3', 'Description 3', GETDATE(), 0),
                            ('Task 4', 'Description 4', GETDATE(), 1),
                            ('Task 5', 'Description 5', GETDATE(), 0);
                    ");
                }

                PrintTasks(connectionString);
            }

            // Добавление новой задачи
            using (var connection = new SqlConnection(connectionString))
            {
                var newTask = new TaskModel
                {
                    Title = "New Task",
                    Description = "New Task Description",
                    DueDate = DateTime.Now.AddDays(7),
                    IsCompleted = false
                };

                connection.Execute(@"
                    INSERT INTO [Tasks] ([Title], [Description], [DueDate], [IsCompleted]) 
                    VALUES (@Title, @Description, @DueDate, @IsCompleted)",
                    newTask);

                PrintTasks(connectionString);
            }

            // Изменение задачи
            using (var connection = new SqlConnection(connectionString))
            {
                var taskToUpdate = connection.QueryFirstOrDefault<TaskModel>("SELECT TOP 1 * FROM Tasks WHERE IsCompleted = 0");
                if (taskToUpdate != null)
                {
                    taskToUpdate.Title = "Updated Task Title";
                    connection.Execute(@"
                        UPDATE [Tasks] 
                        SET [Title] = @Title, [Description] = @Description, [DueDate] = @DueDate, [IsCompleted] = @IsCompleted 
                        WHERE Id = @Id",
                        taskToUpdate);
                }

                PrintTasks(connectionString);
            }

            // Удаление задачи
            using (var connection = new SqlConnection(connectionString))
            {
                var taskToDelete = connection.QueryFirstOrDefault<TaskModel>("SELECT TOP 1 * FROM Tasks WHERE IsCompleted = 1");
                if (taskToDelete != null)
                {
                    connection.Execute("DELETE FROM [Tasks] WHERE Id = @Id", new { Id = taskToDelete.Id });
                }

                PrintTasks(connectionString);
            }
        }

        private static void PrintTasks(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var tasks = connection.Query<TaskModel>("SELECT * FROM Tasks");
                foreach (var task in tasks)
                {
                    Console.WriteLine($"Id: {task.Id}, Title: {task.Title}, Due Date: {task.DueDate}, Completed: {task.IsCompleted}");
                }
                Console.WriteLine("___________________________________________________________________________");
            }
        }
    }
}