using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using Nuke.Common;
using Nuke.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public static class OpenApiSpec
{
    public static void Download(string sourceOpenApiJson, string localOpenApiJson)
    {
        if (sourceOpenApiJson == localOpenApiJson)
            return;

        if (sourceOpenApiJson.StartsWith("http"))
            HttpTasks.HttpDownloadFile(sourceOpenApiJson, localOpenApiJson);

        if (File.Exists(sourceOpenApiJson))
            File.Copy(sourceOpenApiJson, localOpenApiJson, true);

        Assert.FileExists(localOpenApiJson);
    }

    public static void Reformat(string source, string destination)
    {
        var document = ReadOpenApiJson(source);
        WriteOpenApiJson(document, destination);
    }

    public static void ApplyFixes(string source, string destination)
    {
        var document = ReadOpenApiJson(source);

        // Fix 'ClientPolicyConditionRepresentation'
        var clientPolicyConditionRepresentation = document.Components.Schemas.Get("ClientPolicyConditionRepresentation")?.Properties.Get("configuration");
        if (clientPolicyConditionRepresentation != null)
            clientPolicyConditionRepresentation.Type = "string";

        // Fix 'ClientPolicyExecutorRepresentation'
        var clientPolicyExecutorRepresentation = document.Components.Schemas.Get("ClientPolicyExecutorRepresentation")?.Properties.Get("configuration");
        if (clientPolicyExecutorRepresentation != null)
            clientPolicyExecutorRepresentation.Type = "string";

        // Fix realm import request body type
        var adminRealmsRequestBody = document.Paths.Get("/admin/realms")?.Operations.Get(OperationType.Post)?.RequestBody.Content.Get("application/json");
        if (adminRealmsRequestBody != null)
        {
            adminRealmsRequestBody.Schema.Format = null;
            adminRealmsRequestBody.Schema.Type = null;
            adminRealmsRequestBody.Schema.Reference = document.Components.Schemas.Get("RealmRepresentation")?.Reference
                ?? throw new InvalidOperationException("Schema for 'RealmRepresentation' not found");
        }

        // Remove deprecated members producing name conflicts
        document.Components.Schemas.Get("AccessToken")?.Properties.Remove("authTime");
        document.Components.Schemas.Get("IDToken")?.Properties.Remove("authTime");

        // Remove deprecated members producing error on realm import, see https://github.com/fschick/Keycloak.RestApiClient/issues/4#issuecomment-2071597287
        document.Components.Schemas.Get("RealmRepresentation")?.Properties.Remove("userCacheEnabled");
        document.Components.Schemas.Get("RealmRepresentation")?.Properties.Remove("realmCacheEnabled");
        document.Components.Schemas.Get("RealmRepresentation")?.Properties.Remove("oAuth2DeviceCodeLifespan");
        document.Components.Schemas.Get("RealmRepresentation")?.Properties.Remove("oAuth2DevicePollingInterval");

        // Fix duplicate parameter names, https://github.com/fschick/Keycloak.RestApiClient/issues/15
        var adminRealmOrganizationMember = document.Paths.Get("/admin/realms/{realm}/organizations/{id}/members/{id}");
        if (adminRealmOrganizationMember != null)
        {
            foreach (var operation in adminRealmOrganizationMember.Operations.Values)
            {
                var originIdParam = operation.Parameters.First(parameter => parameter.Name == "id");
                var organizationIdParam = new OpenApiParameter(originIdParam) { Name = "organizationId" };
                var userIdParam = new OpenApiParameter(originIdParam) { Name = "userId" };

                operation.Parameters.Remove(originIdParam);
                operation.Parameters.Add(organizationIdParam);
                operation.Parameters.Add(userIdParam);
            }

            document.Paths.Remove("/admin/realms/{realm}/organizations/{id}/members/{id}");
            document.Paths.Add("/admin/realms/{realm}/organizations/{organizationId}/members/{userId}", adminRealmOrganizationMember);
        }

        // Fix duplicate parameter names, https://github.com/fschick/Keycloak.RestApiClient/issues/15
        var adminRealmOrganizationMemberOrganizations = document.Paths.Get("/admin/realms/{realm}/organizations/{id}/members/{id}/organizations");
        if (adminRealmOrganizationMemberOrganizations != null)
        {
            foreach (var operation in adminRealmOrganizationMemberOrganizations.Operations.Values)
            {
                var originIdParam = operation.Parameters.First(parameter => parameter.Name == "id");
                var organizationIdParam = new OpenApiParameter(originIdParam) { Name = "organizationId" };
                var userIdParam = new OpenApiParameter(originIdParam) { Name = "userId" };

                operation.Parameters.Remove(originIdParam);
                operation.Parameters.Add(organizationIdParam);
                operation.Parameters.Add(userIdParam);
            }

            document.Paths.Remove("/admin/realms/{realm}/organizations/{id}/members/{id}/organizations");
            document.Paths.Add("/admin/realms/{realm}/organizations/{organizationId}/members/{userId}/organizations", adminRealmOrganizationMemberOrganizations);
        }

        WriteOpenApiJson(document, destination);
    }

    public static void EscapeForXmlDocumentation(string source, string destination)
    {
        var document = ReadOpenApiJson(source);
        var schemas = document.Components.Schemas.Values.ToList();
        var properties = schemas.SelectMany(schema => schema.Properties.Values).ToList();
        var paths = document.Paths.Values.ToList();
        var operations = paths.SelectMany(path => path.Operations.Values).ToList();
        var parameters = operations.SelectMany(operation => operation.Parameters).ToList();
        var responses = operations.SelectMany(operation => operation.Responses.Values).ToList();

        document.Info.Description = document.Info.Description.EscapeForXmlDocumentation();

        foreach (var schema in schemas)
            schema.Description = schema.Description.EscapeForXmlDocumentation();

        foreach (var property in properties)
            property.Description = property.Description.EscapeForXmlDocumentation();

        foreach (var path in paths)
            path.Description = path.Description.EscapeForXmlDocumentation();

        foreach (var operation in operations)
            operation.Description = operation.Description.EscapeForXmlDocumentation();

        foreach (var path in parameters)
            path.Description = path.Description.EscapeForXmlDocumentation();

        foreach (var path in responses)
            path.Description = path.Description.EscapeForXmlDocumentation();

        WriteOpenApiJson(document, destination);
    }

    public static void HumanizeActionNames(string source, string destination)
    {
        var document = ReadOpenApiJson(source);

        foreach (var (pathName, path) in document.Paths)
        {
            foreach (var (operationType, operation) in path.Operations)
            {
                var humanizedPath = HumanizePath(pathName, operationType);
                operation.Extensions.Add("x-csharp-action", new OpenApiString(humanizedPath));
            }
        }

        WriteOpenApiJson(document, destination);
    }

    private static OpenApiDocument ReadOpenApiJson(string source)
    {
        using var streamReader = new StreamReader(source);
        var reader = new OpenApiStreamReader();
        return reader.Read(streamReader.BaseStream, out _);
    }

    private static void WriteOpenApiJson(OpenApiDocument document, string destination)
    {
        using var streamWriter = new StreamWriter(destination);
        var writer = new OpenApiJsonWriter(streamWriter);
        document.SerializeAsV3(writer);
    }

    private static string? HumanizePath(string? path, OperationType operationType)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var parts = Regex
            .Replace(path, "^/admin/realms", string.Empty)
            .Split('/')
            .Where(part => !string.IsNullOrEmpty(part) && part != "{realm}")
            .ToList();

        var pathParts = parts
            .Where(part => !part.StartsWith('{'))
            .Select(HumanizePathPart)
            .ToList();

        var parameterParts = parts
            .Where(part => part.StartsWith('{') && part.EndsWith('}'))
            .Select(part => part.Trim('{', '}'))
            .Select(HumanizePathPart)
            .ToList();

        var operation = operationType.ToString().UppercaseFirstChar();
        var camelCasedPath = $"{operation}{pathParts.Join()}";

        if (parameterParts.Count > 0)
            camelCasedPath += $"By{parameterParts.Join("And")}";

        return camelCasedPath;
    }

    private static string HumanizePathPart(string pathPart)
        => pathPart
            .Split('-', '_')
            .Select(UppercaseFirstChar)
            .Join();

    private static string? EscapeForXmlDocumentation(this string? text)
        => text?.Replace('<', '{').Replace('>', '}');

    private static TResult? Get<TKey, TResult>(this IDictionary<TKey, TResult> dictionary, TKey key) where TKey : notnull
        => dictionary.TryGetValue(key, out var value)
            ? value
            : default;

    private static string UppercaseFirstChar(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));
    }

    private static string Join(this IEnumerable<string> input, string separator = "")
        => string.Join(separator, input);
}