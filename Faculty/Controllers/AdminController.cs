using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Faculty.Models;
using Faculty.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FacultyMVC.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private UserManager<AppUser> userManager;
        private IPasswordHasher<AppUser> passwordHasher;
        private IPasswordValidator<AppUser> passwordValidator;
        private IUserValidator<AppUser> userValidator;
        private RoleManager<IdentityRole> roleManager;
        public AdminController(RoleManager<IdentityRole> roleMgr, UserManager<AppUser> usrMgr, IPasswordHasher<AppUser> passwordHash, IPasswordValidator<AppUser> passwordVal, IUserValidator<AppUser> userValid)
        {
            roleManager = roleMgr;
            userManager = usrMgr;
            passwordHasher = passwordHash;
            passwordValidator = passwordVal;
            userValidator = userValid;
        }


        public IActionResult Index()
        {
            return View(userManager.Users);
        }


        public IActionResult Create(int studentId, int teacherId)
        {
            ViewData["studentId"] = studentId;
            ViewData["teacherId"] = teacherId;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {

            if (ModelState.IsValid)
            {
                AppUser appUser = new AppUser
                {
                    UserName = user.FirstName + user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email
                };

                if (user.StudentId != 0)
                {
                    appUser.StudentId = user.StudentId;
                    appUser.Role = "Student";
                }
                else if (user.TeacherId != 0)
                {
                    appUser.TeacherId = user.TeacherId;
                    appUser.Role = "Teacher";
                }
                else
                    appUser.Role = "Admin";

                IdentityResult result = await userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    if (user.StudentId != 0)
                    {
                        await userManager.AddToRoleAsync(appUser, "Student");
                    }
                    else if (user.TeacherId != 0)
                    {
                        await userManager.AddToRoleAsync(appUser, "Teacher");
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(appUser, "Admin");
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.TryAddModelError(error.Code, error.Description);
                }
            }
            return View(user);
        }


        public async Task<IActionResult> Update(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);

            if (user != null)
                return View(user);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, string email, string phoneNumber, string password)
        {
            //to use my Custom Validation policies for Email and Password I am using the IPasswordValidator and IUserValidator objects
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
                    ModelState.AddModelError("", "Email cannot be empty"); //apply the Custom Password, Username and Email Policies when Updating a User Account

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
                        return RedirectToAction("Index");
                    else
                        Errors(result);
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");

            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View("Index", userManager.Users);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}