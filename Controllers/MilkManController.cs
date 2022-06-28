
using BAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model.EntityModels;
using Model.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace DairyForm.Controllers
{
    public class MilkManController : Controller
    {
        

        private readonly IMilkMan _MilkManService;
        private IPasswordHasher<MilkMan> _passwordHasher;
       
        private readonly IWebHostEnvironment _env ; 
        

        public MilkManController( IMilkMan MilkManService, IPasswordHasher<MilkMan> passwordHasher, IWebHostEnvironment env)
        {
            //_signInManager = signInManager;
            _MilkManService = MilkManService;
            _passwordHasher = passwordHasher;
            _env = env;
        }

       

        public IActionResult Index()
        {
            var milkMen = _MilkManService.GetAll();
            return View(milkMen);
        }
        public IActionResult GetMilkManById(string Id)
        {
            var result =  _MilkManService.GetMilkManById(Id);
            return View(result);
        }
        public IActionResult GetMilkManByIEmail(string Email)
        {
            var result =  _MilkManService.GetMilkManByEmail(Email);
            return View(result);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(MilkManCreateViewModel milkMan)
        {
            var result=await _MilkManService.Create(milkMan);
            if(result)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("create");
            
            
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string Id)
        {
            MilkMan user = await _MilkManService.GetMilkManById(Id);
            if (user != null)
            {
                IdentityResult result = await _MilkManService.Delete(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "User Not Found");
                }
               
            }
           
               
            return View("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Update(string Id)
        {
            MilkMan user = await _MilkManService.GetMilkManById(Id);

            if (user != null)
            return RedirectToAction("Index");
                     
            else
            {
                string? firstName = user.FirstName;
                MilkManCreateViewModel model = new MilkManCreateViewModel()
                {
                   Id= user.Id,
                   FirstName= firstName,
                   LastName= user.LastName,
                   Email= user.Email,
                   Address= user.Address,
                   Age = (int)user.Age
                };
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(MilkManCreateViewModel createViewModel)
        {
            if(ModelState.IsValid)
            {
                MilkMan user = await _MilkManService.GetMilkManById(createViewModel.Id);
                if (user != null)
                {
                    user.Email =createViewModel.Email;
                    user.Id =createViewModel. Id;
                    user.NormalizedEmail =createViewModel. Email;
                    user.FirstName =createViewModel. FirstName;
                    user.LastName = createViewModel.LastName;
                    user.Age = createViewModel.Age;
                    user.PasswordHash = createViewModel.Password;
                    var result = await _MilkManService.Update(user);
                    if(result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Usernot found");
                    }
                }

            }
            return View(createViewModel);
           
           
        }


        [Route("login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
       
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await _MilkManService.PasswordSignInAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index","MilkMan");
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Welcome()
        {
            return View();
        }

        //public async Task<IActionResult> ForgotPassword(ForgotViewModel model)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        var user = await _MilkManService.GetMilkManById(model.Email);
        //        if(user != null)
        //        {
        //          //  await _MilkManService.GenerateForgotPasswordTokenAsync(user);
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "some thing went wrong");
        //        }
        //        //code here
        //        ModelState.Clear();
        //        return RedirectToAction("Index");
        //        //return View(model);
        //       // model.EmailSent = true;
        //    }
        //    return View(model);
        //}
        [HttpGet]
        public IActionResult  ResetPassword(string token,string Email)
        {
            var model = new ResetPasswordViewModel { Token=token, Email=Email};

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                model.Token = model.Token.Replace(" ", "+");
                var result = await _MilkManService.ResetPassword(model);
                if(result)
                {
                    ModelState.Clear();
                    return RedirectToAction("Index", "DairyMan");
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            if(email != null)
            {
                var user = await _MilkManService.GetMilkManByEmail(email);
                if(user == null)
                {
                    TempData["SendMsg"] = "Invalid User";
                    return View();
                }
                // if (!ModelState.IsValid)
                string FullName = user.FirstName +""+user.LastName;
                if(user != null)
                {
                   // var token = await _MilkManService.GenerateForgotPasswordTokenAsync(user);
                    var path = _env.ContentRootPath + "wwwroot\\Templates\\ForgotPassword.html";
                    var link = Url.Action("ResetPassword", "MilkMan", new {  email = user.Email }, Request.Scheme);
                    if(link != null)
                    {
                        bool emailResponse = _MilkManService.SendEmailPasswordResetLink(user.Email, FullName, link, path);
                        if(emailResponse == true)
                        {
                            TempData["SendMsgEmail"] = "Email Send successfully";
                            return View();
                        }
                        TempData["SendMsgEmail"] = "Email not send";
                        return View();
                    }
                }

                //var user = await _MilkManService.GetMilkManById(email);
                //if (user == null)
                //    return RedirectToAction(nameof(ForgotPasswordConfirmation));


                //EmailHelper emailHelper = new EmailHelper();
                //bool emailResponse = emailHelper.SendEmailPasswordReset(user.Email, link);

                //if (emailResponse)
                //    return RedirectToAction("ForgotPasswordConfirmation");
                //else
                //{
                //    // log email failed
                //}
                return View(email);
            }
            return View();
           
        }


    }
}




