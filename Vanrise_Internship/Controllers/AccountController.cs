using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Vanrise_Internship.Models;

public class AccountController : Controller
{
    private string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["InternshipConnectionString"].ConnectionString;

    [HttpPost]
    public ActionResult Login(User user)
    {
        if (ModelState.IsValid)
        {
            string hashedPassword = ComputeSha256Hash(user.Password);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@Password", hashedPassword);

                connection.Open();
                int count = Convert.ToInt32(command.ExecuteScalar());

                if (count == 1)
                {
                    // Redirect to Index if login successful
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid username or password.";
                    return View("~/Views/Home/Login.cshtml", user);  // Explicit path for Login view
                }
            }
        }

        return View("~/Views/Home/Login.cshtml", user);  // Ensure the view path is specified
    }



    public ActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            if (user.Password.Length < 8)
            {
                ModelState.AddModelError("Password", "Password must be at least 8 characters long.");
                return View("~/Views/Home/Register.cshtml", user);  // Explicit path for Register view
            }

            try
            {
                user.Password = ComputeSha256Hash(user.Password);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Check if username already exists
                    string checkUserQuery = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                    SqlCommand checkUserCommand = new SqlCommand(checkUserQuery, connection);
                    checkUserCommand.Parameters.AddWithValue("@Username", user.Username);

                    connection.Open();
                    int userExists = Convert.ToInt32(checkUserCommand.ExecuteScalar());

                    if (userExists > 0)
                    {
                        ViewBag.ErrorMessage = "This username is already taken. Please choose another one.";
                        return View("~/Views/Home/Register.cshtml", user);  // Specify correct view path
                    }

                    // Insert the new user
                    string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);

                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
                return View("~/Views/Home/Register.cshtml", user);  // Specify correct view path
            }
        }

        return View("~/Views/Home/Register.cshtml", user);  // Ensure view path is specified
    }






    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
