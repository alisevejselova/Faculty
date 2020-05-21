using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faculty.Controllers;
using Faculty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FacultyMVC.Controllers
{
    [Authorize(Roles = "Admin, Student, Teacher")]
    public class AccountController : Controller
    {

        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;
        private IPasswordHasher<AppUser> passwordHasher;
        private IPasswordValidator<AppUser> passwordValidator;
        private IUserValidator<AppUser> userValidator;

        public AccountController(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr, IPasswordHasher<AppUser> passhash, IPasswordValidator<AppUser> passvalid, IUserValidator<AppUser> uservalid)
        {
            userManager = userMgr;
            signInManager = signinMgr;
            passwordHasher = passhash;
            passwordValidator = passvalid;
            userValidator = uservalid;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = await userManager.FindByEmailAsync(login.Email);
                if (appUser != null)
                {
                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.CheckPasswordSignInAsync(appUser, login.Password, false);
                    if (result.Succeeded)
                        return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(nameof(login.Email), "Login Failed: Invalid Email or password");
            }
            return View(login);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");

        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()   /*The ‘/Account/AccessDenied’ URL is the default URL set by Identity which you can change by setting the AccessDeniedPath configuration property in the ConfigureServices method of Statup Class.*/
        {
            return View();
        }


        //GET: Account/UserInfo/{Userid}
        public async Task<IActionResult> UserInfo(string id)
        {
            AppUser loggedUser = await userManager.GetUserAsync(User);
            AppUser user = await userManager.FindByIdAsync(id);

            if (loggedUser.Id != id)
            {
                return RedirectToAction("AccessDenied", "Account", null);
            }

            if (user != null)
                return View(user);
            else
                return RedirectPermanent("~/Home/Index");
        }

        [HttpPost]
        public async Task<IActionResult> UserInfo(string id, string email, string phoneNumber, string password)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult validEmail = null;
                if (!string.IsNullOrEmpty(email))
                {
                    validEmail = await userValidator.ValidateAsync(userManager, user);
                    if (validEmail.Succeeded)
                        user.Email = email;
                    else
                        Errors(validEmail);
                }
                else
                    ModelState.AddModelError("", "Email cannot be empty");

                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(password))
                {
                    validPass = await passwordValidator.ValidateAsync(userManager, user, password);
                    if (validPass.Succeeded)
                        user.PasswordHash = passwordHasher.HashPassword(user, password);
                    else
                        Errors(validPass);
                }
                else
                    validPass = IdentityResult.Success;

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    if (phoneNumber.Length == 9)
                        user.PhoneNumber = phoneNumber;
                    else
                        ModelState.AddModelError("", "Phone number must be 9 digits long");
                }

                if (validEmail != null && validEmail.Succeeded && validPass.Succeeded)
                {
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return RedirectPermanent("~/Home/Index");
                    else
                        Errors(result);
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");

            return View(user);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}