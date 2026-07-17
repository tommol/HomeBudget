using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HomeBudget.Api.OpenApi;

internal sealed class ApiVersionPathDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Paths is null)
        {
            return Task.CompletedTask;
        }

        var documentVersion = context.DocumentName.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? context.DocumentName
            : $"v{context.DocumentName}";
        var paths = new OpenApiPaths();

        foreach (var (path, pathItem) in document.Paths)
        {
            paths[SubstituteApiVersion(path, documentVersion)] = pathItem;
        }

        document.Paths = paths;

        return Task.CompletedTask;
    }

    private static string SubstituteApiVersion(string path, string documentVersion)
        => path
            .Replace("v{version:apiVersion}", documentVersion, StringComparison.OrdinalIgnoreCase)
            .Replace("v{version}", documentVersion, StringComparison.OrdinalIgnoreCase);
}
