using LanguageLearnNETWebAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearnNETWebAPI.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static IActionResult HandleResult<TData>(this ControllerBase controllerBase, Result<TData> result)
        {
            if (result == null)
            {
                return controllerBase.StatusCode(StatusCodes.Status500InternalServerError, "Result is null");
            }
            else if (result.IsSuccess)
            {
                return controllerBase.Ok(result.Data);
            }
            else
            {
                return controllerBase.StatusCode(result.StatusCode, result.Message);
            }
        }
    }
}
