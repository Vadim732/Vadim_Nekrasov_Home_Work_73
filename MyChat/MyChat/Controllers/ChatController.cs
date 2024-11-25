using Microsoft.AspNetCore.Mvc;
using MyChat.Models;

namespace MyChat.Controllers;

public class ChatController : Controller
{
    public readonly MyChatContext _context;
    
    public IActionResult Index()
    {
        return View();
    }
}