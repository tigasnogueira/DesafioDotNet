using DesafioDotNet.Domain.Interfaces;
using DesafioDotNet.Domain.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DesafioDotNet.Application.Controllers;

[ApiController]
public abstract class MainController : ControllerBase
{
    private readonly INotifier _notifier;


    protected Guid UsuarioId { get; set; }
    protected bool UsuarioAutenticado { get; set; }

    protected MainController(INotifier notifier)
    {
        _notifier = notifier;


    }

    protected bool OperacaoValida()
    {
        return !_notifier.HasNotification();
    }

    protected ActionResult CustomResponse(object result = null)
    {
        if (OperacaoValida())
        {
            return Ok(new
            {
                success = true,
                data = result
            });
        }

        return BadRequest(new
        {
            success = false,
            errors = _notifier.GetNotifications().Select(n => n.Message)
        });
    }

    protected ActionResult CustomResponse(ModelStateDictionary modelState)
    {
        if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
        return CustomResponse();
    }

    protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
    {
        var erros = modelState.Values.SelectMany(e => e.Errors);
        foreach (var erro in erros)
        {
            var errorMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
            NotifyError(errorMsg);
        }
    }

    protected void NotifyError(string message)
    {
        _notifier.Handle(new Notification(message));
    }
}