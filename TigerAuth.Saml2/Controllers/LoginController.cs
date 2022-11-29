using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TigerAuth.Saml2.Controllers;

public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;

    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }

    public void Login()
    {
        // TODO
        Debug.WriteLine("Login started");
        // return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        // TODO
        return Ok();
    }
}