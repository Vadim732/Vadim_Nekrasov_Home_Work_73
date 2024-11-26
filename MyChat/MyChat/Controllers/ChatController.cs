using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyChat.Models;

namespace MyChat.Controllers;

public class ChatController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly MyChatContext _context;

    public ChatController(UserManager<User> userManager, MyChatContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    
    public IActionResult Index()
    {
        List<Message> messages = _context.Messages.Include(m => m.User).ToList();
        return View(messages);
    }
}