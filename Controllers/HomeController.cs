using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using belt_exam.Models;
using Microsoft.AspNetCore.Http;
using belt_exam.Connections;
using Microsoft.AspNetCore.Authorization;

namespace belt_exam.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbConnector _dbConnector;
        public HomeController(DbConnector connect) {
            _dbConnector = connect;
        }

        [HttpGet]
        [Route("")]
        public IActionResult LoginReg()
        {
            ViewBag.Errors = TempData["Errors"];
            return View("LoginReg");
        }
        [HttpPost]
        [Route("register")]
        public IActionResult Register(User user) {
            string query0 = ($"SELECT * FROM users WHERE users.username='{user.UserName}'");
            var unique = _dbConnector.Query(query0).SingleOrDefault();
            if(unique != null) {
                ModelState.AddModelError("UserName", "Username taken");
                return View("LoginReg");
            }
            if(ModelState.IsValid) {
                string query = ($"INSERT INTO users (name, username, password, balance, created_at, updated_at) VALUES ('{user.Name}', '{user.UserName}', '{user.Password}', 1000, NOW(), NOW())");
                _dbConnector.Execute(query);
                HttpContext.Session.SetString("name", user.Name);
                string query2 =($"SELECT * FROM users WHERE username='{user.UserName}'");
                Dictionary<string, object> currUser = _dbConnector.Query(query2).SingleOrDefault();
                HttpContext.Session.SetInt32("user_id", (int) currUser["id"]);
                HttpContext.Session.SetString("name", user.Name);
                string query3 = ($"SELECT * from users WHERE users.id={HttpContext.Session.GetInt32("user_id")}");
                var currUser1 = _dbConnector.Query(query2).SingleOrDefault();
                HttpContext.Session.SetInt32("balance", (int) currUser["balance"]);
                return RedirectToAction("Dashboard");
            } else {
                return View("LoginReg");
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(string UserName, string Password) {
            string query = ($"SELECT * from users WHERE username='{UserName}'");
            var user = _dbConnector.Query(query).FirstOrDefault();
            if(user == null) {
                TempData["Errors"] = "Invalid Username/Password";
            } else {
                if((string) user["password"] != Password) {
                    TempData["Errors"] = "Username/Password Mismatch";
                } else {
                    HttpContext.Session.SetInt32("user_id", (int) user["id"]);
                    HttpContext.Session.SetString("name", (string) user["name"]);
                    string query2 = ($"SELECT * from users WHERE users.id={HttpContext.Session.GetInt32("user_id")}");
                    var currUser = _dbConnector.Query(query2).SingleOrDefault();
                    HttpContext.Session.SetInt32("balance", (int) currUser["balance"]);
                    return RedirectToAction("Dashboard");
                }
            }
            return RedirectToAction("LoginReg");
        }

        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard() {
            if(HttpContext.Session.GetInt32("user_id") == null) {
                return RedirectToAction("LoginReg");
            }
            ViewBag.user_name = HttpContext.Session.GetString("name");
            ViewBag.user_id = HttpContext.Session.GetInt32("user_id");
            ViewBag.user_balance = HttpContext.Session.GetInt32("balance");
            string query = ($"SELECT products.name as ProductName, users.name as UserName, products.bid, products.bidder_name, products.end_date, products.id as ProductId, users.id as UserId, user_id as productUserId, products.end_date_time FROM products JOIN users on products.user_id = users.id ORDER BY products.end_date ASC");
            ViewBag.AllProducts = _dbConnector.Query(query);
            foreach(var product in ViewBag.AllProducts) {
                System.Console.WriteLine("HERE IT IS: " + product["ProductId"]);
                int days_remaining = (int) Math.Ceiling((product["end_date_time"] - DateTime.Now).TotalDays);
                string query8 = ($"UPDATE products SET end_date = {days_remaining} WHERE products.id={product["ProductId"]}");
                _dbConnector.Execute(query8);
            }
            foreach (var product in ViewBag.AllProducts) {
                if (product["end_date"] == 0) {
                    string query2 = ($"SELECT * from users WHERE users.id={product["UserId"]}");
                    var productOwner = _dbConnector.Query(query2).SingleOrDefault();
                    if (product["bidder_name"] != "") {
                        string query3 = ($"SELECT * from users WHERE users.name='{product["bidder_name"]}'");
                        var bidder = _dbConnector.Query(query3).SingleOrDefault();
                        var query4 = ($"UPDATE users SET users.balance ={(int)productOwner["balance"] + product["bid"]} WHERE users.id ={productOwner["id"]}");
                        _dbConnector.Execute(query4);
                        var query5 = ($"UPDATE users SET users.balance ={(int)bidder["balance"] - product["bid"]} WHERE users.id = {bidder["id"]}");
                        _dbConnector.Execute(query5);
                    }
                    string query6 =($"DELETE FROM products WHERE id={product["ProductId"]}");
                    _dbConnector.Execute(query6);
                }
            }
            string query7 = ($"SELECT products.name as ProductName, users.name as UserName, products.bid, products.bidder_name, products.end_date, products.id as ProductId, users.id as UserId, user_id as productUserId FROM products JOIN users on products.user_id = users.id ORDER BY products.end_date ASC");
            ViewBag.AllProducts = _dbConnector.Query(query);
            return View("Dashboard");
        }

        [HttpGet]
        [Route("newAuction")]
        public IActionResult NewAuction() {
            if(HttpContext.Session.GetInt32("user_id") == null) {
                return RedirectToAction("LoginReg");
            }
            return View("NewAuction");
        }

        [HttpPost]
        [Route("AddAuction")]
        public IActionResult AddAuction(Product product) {
            if(HttpContext.Session.GetInt32("user_id") == null) {
                return RedirectToAction("LoginReg");
            }
            if(ModelState.IsValid){
                int days_remaining = (int) Math.Ceiling((product.EndDate - DateTime.Now).TotalDays);
                var user_id = HttpContext.Session.GetInt32("user_id");
                string query = ($"SELECT * from users WHERE id ={user_id}");
                var currUser = _dbConnector.Query(query).SingleOrDefault();
                string query2 = ($"INSERT INTO products (name, description, end_date, created_at, updated_at, bidder_name, bid, user_id, end_date_time) VALUES ('{product.Name}', '{product.Description}', {days_remaining}, NOW(), NOW(), '{""}', '{product.Bid}', {user_id}, '{(product.EndDate).ToString("yyyy-MM-dd HH:mm:ss")}') ");
                _dbConnector.Execute(query2);
                return RedirectToAction("Dashboard");
            } else {
                return View("NewAuction");
            }
        }

        [HttpGet]
        [Route("/products/{id}")]
        public IActionResult SpecAuction(int id) {
            if(HttpContext.Session.GetInt32("user_id") == null) {
                return RedirectToAction("LoginReg");
            }
            string query = ($"SELECT products.name as ProductName, users.name as UserName, products.bid, products.end_date, products.description, products.id as ProductId, users.id as UserId, user_id as productUserId, products.bidder_name FROM products JOIN users on products.user_id = users.id WHERE products.id = {id}");
            ViewBag.SpecProduct = _dbConnector.Query(query).SingleOrDefault();
            return View("SpecAuction");
        }

        [HttpPost]
        [Route("newBid")]
        public IActionResult NewBid(int bid, int product_id) {
            if(HttpContext.Session.GetInt32("user_id") == null) {
                return RedirectToAction("LoginReg");
            }
            string query2 = ($"SELECT * from users where users.id={HttpContext.Session.GetInt32("user_id")}");
            var currUser = _dbConnector.Query(query2).SingleOrDefault();
            string query1 = ($"SELECT * from products WHERE products.id = {product_id}");
            var currProduct = _dbConnector.Query(query1).SingleOrDefault();
            if ((int)currProduct["bid"] >= bid) {
                TempData["errors"] = "Cannot bid lower than current bid";
                return Redirect("/products/" + product_id);
            }
            if((int)currUser["balance"] < bid) {
                TempData["errors"] = "Bid cannot be higher than current balance";
                return Redirect("/products/" + product_id);
            }
            
            var user_name = HttpContext.Session.GetString("name");
            string query = ($"UPDATE products SET bid={bid}, bidder_name='{user_name}' WHERE products.id ={product_id}");
            _dbConnector.Execute(query);
            string query3 = ($"UPDATE users SET balance={(int)currUser["balance"] - bid} WHERE users.id ={HttpContext.Session.GetInt32("user_id")}");
            return Redirect("/products/" + product_id);
        }

        [HttpPost]
        [Route("/delete")]
        // [AutoValidateAntiforgeryToken]
        public IActionResult Delete(int id) {
            if(HttpContext.Session.GetInt32("user_id") == null) {
                return RedirectToAction("LoginReg");
            }
            string query =($"DELETE FROM products WHERE id={id}");
            _dbConnector.Execute(query);
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Route("/logout")]
        public IActionResult Logout() {
            HttpContext.Session.Clear();
            return RedirectToAction("LoginReg");
        }
    }
}
