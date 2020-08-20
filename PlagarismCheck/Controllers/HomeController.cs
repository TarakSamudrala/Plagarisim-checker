using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlagarismCheck.Models;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace PlagarismCheck.Controllers
{
    public class HomeController : Controller
    {
        plagarismEntities db = new plagarismEntities();
        public ActionResult Index(string Message=null)
        {
            if (Session["UserName"] != null)
            {
                ViewBag.Message = Message;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult CandidateStatus(string Message=null)
        {
            if (Session["UserName"] != null)
            {
                CheckPlagarismModel model = new CheckPlagarismModel();
                ViewBag.Message = !string.IsNullOrEmpty(Message) ? Message : "";
                if (Session["UserName"] != null)
                {
                    string UserName = Session["UserName"].ToString();
                    user Users = db.users.FirstOrDefault(c => c.UserName.ToLower() == UserName.ToLower());
                    model.UserID = Users.UserID;
                    model.DocumentContent = Users.DocumentText;
                    model.Status = Users.Status;
                    model.Result = Users.Result;
                    if (string.IsNullOrEmpty(Users.DocumentText))
                    {
                        ViewBag.Message= "Please Upload the document";
                    }
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult SaveCandidateStatus(CheckPlagarismModel model)
        {
            if (Session["UserName"] != null)
            {
                if (Session["UserName"] != null)
                {
                    user Users = db.users.FirstOrDefault(c => c.UserID == model.UserID);
                    Users.DocumentText = model.DocumentContent;
                    db.SaveChanges();
                }
                return RedirectToAction("CandidateStatus", new { Message = "Successfully Saved" });
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        [AllowAnonymous]
        public ActionResult FacultyLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Register(string Message=null)
        {
            ViewBag.Message = Message;
            return View();
        }
      

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult SaveRegister(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    user user = new user();
                    user.UserName = model.UserName;
                    user.Password = model.Password;
                    user.Email = model.Email;
                    user.FullName = model.FullName;
                    user.Group = model.Group;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Year = model.Year;
                    db.users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Register", "Home", new { Message = "Successfully Saved" });
                }
                catch
                {
                    
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {

            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        public ActionResult VerifyLogin(LoginModel model)
        {
            if (ModelState.IsValid && db.users.FirstOrDefault(c => c.UserName.ToLower() == model.UserName.ToLower() && c.Password.ToLower() == model.Password.ToLower()) != null)
            {
                Session["UserName"] = model.UserName;
                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return RedirectToAction("Login", "Home");
        }


        public ActionResult ULogOff()
        {
            Session["UserName"] = null;
            return RedirectToAction("Login", "Home");
        }
        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult FacultyLogin(LoginModel model)
        {
            if (ModelState.IsValid && model.UserName.ToLower()=="faculty.srit" && model.Password.ToLower()=="123")
            {
                Session["UserName"] = "Faculty.srit_123";
                return RedirectToAction("RegisteredUsers", "Home");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        public ActionResult CheckPlagarism(int UserID=0,string Message=null)
        {
          if (Session["UserName"] != null)
            {
            CheckPlagarismModel model = new CheckPlagarismModel();
            ViewBag.Message = Message;
            if (UserID > 0)
            {
                user Users = db.users.FirstOrDefault(c => c.UserID == UserID);
                model.UserID = Users.UserID;
                model.DocumentContent = Users.DocumentText;
                model.Status = Users.Status;
                model.FullName = Users.FullName;
            }
            return View(model);
                 }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }
        public int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }


        public double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        public string CheckPlagarismResult(int UserID = 0,string Group=null,string Year=null)
        {
            double CheckPercentage = 0;
            string UserDocumentText = null;
            string AllUsersDocumentText = null;
            user Users = db.users.FirstOrDefault(c => c.UserID == UserID);
            UserDocumentText = Users.DocumentText;
           
            if (!string.IsNullOrEmpty(UserDocumentText))
            {
                var AllUsers = db.users.Where(c=>c.UserID!=Users.UserID).ToList();
                if (!string.IsNullOrEmpty(Group))
                {
                    AllUsers = AllUsers.Where(c => c.Group.ToLower() == Group.ToLower()).ToList();

                }
                if (!string.IsNullOrEmpty(Year))
                {
                    AllUsers = AllUsers.Where(c => c.Year.ToLower() == Year.ToLower()).ToList();

                }
                foreach (var item in AllUsers)
                {
                    AllUsersDocumentText += item.DocumentText;
                }
                // Taking a string 
              

                String[] spearator = { ".",",",";",":","#" };

                // using the method 
               

                AllUsersDocumentText = !string.IsNullOrEmpty(AllUsersDocumentText) ? AllUsersDocumentText.ToLower() : "";
                UserDocumentText = !string.IsNullOrEmpty(UserDocumentText) ? UserDocumentText.ToLower() : "";
                if (!string.IsNullOrEmpty(AllUsersDocumentText))
                {
                    var paWords = AllUsersDocumentText.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
                    var pbWords = UserDocumentText.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
                    double pmatches = (double)paWords.Count(x => pbWords.Contains(x));
                    double PCheckPercentage = pmatches / (double)paWords.Count();

                    var aWords = AllUsersDocumentText.Split(' ');
                    var bWords = UserDocumentText.Split(' ');
                    double matches = (double)aWords.Count(x => bWords.Contains(x));
                    double wCheckPercentage = matches / (double)aWords.Count();

                    double totalPercentage = PCheckPercentage + wCheckPercentage;
                    double totalPercentageVal = (pmatches + matches) / (((double)paWords.Count()) + ((double)aWords.Count()));

                    return totalPercentageVal.ToString("P", CultureInfo.InvariantCulture);
                }
                else
                {
                    return "NO Data";
                }
            }
            else
            {
                return "Fail";
            }
          
        }

        public ActionResult SaveCheckPlagarism(CheckPlagarismModel model)
        {
           
            if (model.UserID > 0)
            {
                user Users = db.users.FirstOrDefault(c => c.UserID == model.UserID);
                if (!string.IsNullOrEmpty(model.Status))
                {
                    Users.Status = model.Status;
                    Users.Result = model.Result;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("CheckPlagarism", "Home", new { UserID = model.UserID, Message = "Successfully Saved" });
        }

        public ActionResult RegisteredUsers(RegisteredUsersViewModel model)
        {
            model.Users = new List<RegisteredUsers>();
            model.CurrentPage = model.CurrentPage == 0 ? 1 : model.CurrentPage;
            int pageIndex = model.CurrentPage == 0 ? 0 : model.CurrentPage - 1;
            // int pageIndex = model.CurrentPage == 1 ? 0 : model.CurrentPage;
            model.PageSize = 25;
            if (Session["UserName"] != null)
            {
                try
                {
                    model.Users = (from u in db.users
                                   select new RegisteredUsers
                                   {
                                       Email = u.Email,
                                       FullName = u.FullName,
                                       PhoneNumber = u.PhoneNumber,
                                       Group = u.Group,
                                       Year = u.Year,
                                       UserID = u.UserID
                                   }).Distinct().ToList();

                   
                    
                    if (!string.IsNullOrEmpty(model.Fullname))
                    {
                        model.Users = model.Users.Where(c => c.FullName.ToLower().Contains(model.Fullname.ToLower())).ToList();
                    }
                    if (!string.IsNullOrEmpty(model.Group))
                    {
                        model.Users = model.Users.Where(c => c.Group.ToLower() == model.Group.ToLower()).ToList();
                    }
                    if (!string.IsNullOrEmpty(model.Year))
                    {
                        model.Users = model.Users.Where(c => c.Year.ToLower() == model.Year.ToLower()).ToList();
                    }
                    if (model.Users != null && model.Users.Count() > 0)
                    {
                        model.RowCount = model.Users.ToList().Count();
                        model.Users = model.Users.OrderByDescending(c => c.UserID).Skip(pageIndex * model.PageSize).Take(model.PageSize).Distinct().ToList();
                        model.PageCount = model.RowCount / model.PageSize;
                        if (model.RowCount % model.PageSize > 0)
                        {
                            model.PageCount += 1;
                        }
                    }

                    return View(model);
                }
                catch
                {
                    return RedirectToAction("Login", "Home");
                }

            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult CheckStatus(CheckStatus model)
        {
            model = new CheckStatus();
            if (Session["UserName"] != null)
            {
                try
                {
                        string UserName = Session["UserName"].ToString();
                        user user = db.users.FirstOrDefault(c => c.UserName.ToLower() == UserName.ToLower());
                        model.FileContent = !string.IsNullOrEmpty(model.FileContent) ? model.FileContent : string.Empty;
                        model.FileStatus = !string.IsNullOrEmpty(model.FileStatus) ? model.FileStatus : "Evaluating";
                        db.SaveChanges();
                    

                    //ViewBag.Message = "File Uploaded Successfully!!";
                    return View(model);
                }
                catch
                {
                   // ViewBag.Message = "File upload failed!!";
                    return RedirectToAction("Login", "Home");
                }
              
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

         [HttpPost]  
        public ActionResult UploadFile(HttpPostedFileBase file)  
        {
            if (Session["UserName"] != null)
            {
                string _path=null;
                try
                {
                    string FileContent = null;
                    if (file.ContentLength > 0)
                    {
                        Stream inStream = file.InputStream;
                        int length = file.ContentLength;
                        string contentType = file.ContentType;
                        byte[] binaryData = new byte[length];
                        int n = inStream.Read(binaryData, 0, length);
                         FileContent = Encoding.ASCII.GetString(binaryData); 

                        //string _FileName = Path.GetFileName(file.FileName);
                        //String strAppDir = Server.MapPath("~/UploadFiles");
                        //_path = Path.Combine(strAppDir, _FileName);
                        //file.SaveAs(_path);
                        //if (_FileName != null)
                        //{
                        //    // Read a text file line by line.  
                        //    string[] lines = System.IO.File.ReadAllLines(_path);
                        //    foreach (string line in lines)
                        //        FileContent += line;
                        //}
                        string UserName = Session["UserName"].ToString();
                        user user = db.users.FirstOrDefault(c => c.UserName.ToLower() == UserName.ToLower());
                        user.DocumentText = FileContent;
                        db.SaveChanges();
                    }

                    ViewBag.Message = "File Uploaded Successfully!!";
                    return RedirectToAction("Index", "Home", new { Message = "Successfully Uploaded" });
                }
                catch
                {
                    ViewBag.Message = "File upload failed!!";
                    return RedirectToAction("Login", "Home");
                }
                finally
                {
                    if (_path != null)
                    {
                        System.IO.File.Delete(_path);
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        } 

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
