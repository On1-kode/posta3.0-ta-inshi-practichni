using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace PostService.Swagger;
/// <summary>Додає пояснення параметра маршруту до документації API.</summary>
public class IdParameterFilter : IOperationFilter
{
    /// <summary>Описує ідентифікатор відправлення в кожній операції з параметром id.</summary>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var parameter in operation.Parameters)
            if (parameter.Name == "id") parameter.Description = "Унікальний ідентифікатор поштового відправлення.";
    }
}
