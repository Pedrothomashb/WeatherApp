using System.Net;
using System.Text.Json;

namespace WeatherApp.Api.Middleware;

public class ExceptionMiddleware
{
    /// <summary>
    /// Proximo passo no pipeline de requisição
    /// </summary>
    private readonly RequestDelegate objNext;
    /// <summary>
    /// Identificar de qual classe veio a mensagem no log
    /// </summary>
    private readonly ILogger<ExceptionMiddleware> objLogger;

    public ExceptionMiddleware(RequestDelegate pNext, ILogger<ExceptionMiddleware> pLogger)
    {
        objNext = pNext;
        objLogger = pLogger;
    }

    /// <summary>
    /// Metodo que o ASP.NET chama automaticamente para cada requisição HTTP que chega na API
    /// </summary>
    /// <param name="pContext">objeto que contem tudo sobre a requisição</param>
    /// <returns>Resposta do HTTP</returns>
    public async Task InvokeAsync(HttpContext pContext)
    {
        try
        {
            await objNext(pContext);
        }
        catch (HttpRequestException ex)
        {
            objLogger.LogError(ex, "Erro ao consultar provedor de clima");
            await WriteError(pContext, HttpStatusCode.BadGateway, "Provedor de clima indisponível. Tente novamente.");
        }
        catch (ArgumentException ex)
        {
            await WriteError(pContext, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            objLogger.LogError(ex, "Erro interno inesperado");
            await WriteError(pContext, HttpStatusCode.InternalServerError, "Erro interno. Tente novamente.");
        }
    }

    /// <summary>
    /// Metodo que escreve o erro ocorrido na requisição
    /// </summary>
    /// <param name="pContext">objeto que contem tudo sobre a requisição</param>
    /// <param name="pStatus">Status da requisição</param>
    /// <param name="pMessage">Mensagem de erro</param>
    /// <returns>Retorna Erro</returns>
    private static Task WriteError(HttpContext pContext, HttpStatusCode pStatus, string pMessage)
    {
        pContext.Response.StatusCode = (int)pStatus;
        pContext.Response.ContentType = "application/json";
        string strBody = JsonSerializer.Serialize(new { error = pMessage });
        return pContext.Response.WriteAsync(strBody);
    }
}
