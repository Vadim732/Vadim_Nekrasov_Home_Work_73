using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyChat.Models;
using MyChat.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MyChat.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly MyChatContext _context;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, MyChatContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.FindByEmailAsync(model.Identifier) ?? await _userManager.FindByNameAsync(model.Identifier);
        
            if (user != null)
            {
                SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Chat");
                }
            }
            
            ModelState.AddModelError("", "Неверный логин или пароль!");
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUserEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserEmail != null)
            {
                ViewBag.ErrorMessage = "Ошибка: Этот адрес электронной почты уже используется другим пользователем!";
                return View(model);
            }
            
            var existingUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null)
            {
                ViewBag.ErrorMessage = "Ошибка: Этот логин уже используется другим пользователем!";
                return View(model);
            }
            
            var currentDate = DateTime.UtcNow;
            var userAge = currentDate.Year - model.DateOfBirth.Year;
            if (model.DateOfBirth > currentDate.AddYears(-userAge)) 
            {
                userAge--;
            }
            if (userAge < 18)
            {
                ViewBag.ErrorMessage = "Ошибка: Нельзя зарегистрироваться пользователям моложе 18 лет!";
                return View(model);
            }

            User user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Avatar = model.Avatar,
                DateOfBirth = model.DateOfBirth.ToUniversalTime()
                
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Chat");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        User user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            return View(user);
        }
        return RedirectToAction("Login", "Account");
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        User user = await _userManager.GetUserAsync(User);
        var model = new EditViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Avatar = user.Avatar,
            DateOfBirth = user.DateOfBirth
        };
        return View(model);
    }
    
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var existingUserEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserEmail != null && existingUserEmail.Id != user.Id)
                {
                    ViewBag.ErrorMessage = "Ошибка: Этот адрес электронной почты уже используется другим пользователем!";
                    return View(model);
                }
            
                var existingUserName = await _userManager.FindByNameAsync(model.UserName);
                if (existingUserName != null && existingUserName.Id != user.Id)
                {
                    ViewBag.ErrorMessage = "Ошибка: Этот логин уже используется другим пользователем!";
                    return View(model);
                }
            
                var currentDate = DateTime.Now;
                var userAge = currentDate.Year - model.DateOfBirth.Year;
                if (model.DateOfBirth > currentDate.AddYears(-userAge)) 
                {
                    userAge--;
                }
                if (userAge < 18)
                {
                    ViewBag.ErrorMessage = "Ошибка: Нельзя зарегистрироваться пользователям моложе 18 лет!";
                    return View(model);
                }
                
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Avatar = model.Avatar;
                user.DateOfBirth = model.DateOfBirth.ToUniversalTime();

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Profile", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
    
        return View(model);
    }
    
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}