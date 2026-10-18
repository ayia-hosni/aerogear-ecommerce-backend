using Microsoft.AspNetCore.Mvc;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Config;

namespace SportEcommerce.Api.Controllers;

[ApiController]
[Route("api/config")]
public sealed class ConfigController(IFirebaseWebConfigProvider firebase) : ControllerBase
{
    [HttpGet("firebase")]
    [ResponseCache(Duration = 300)]
    public ActionResult<FirebaseWebConfig> Firebase() =>
        firebase.Get() is { } config ? config : NotFound();
}
