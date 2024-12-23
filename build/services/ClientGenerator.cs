using Nuke.Common;
using Nuke.Common.Tooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ClientGenerator
{
    private static readonly Tool _openapiGeneratorCmd = ToolResolver.GetPathTool(NukeBuild.RootDirectory / $"build/openapi-generator/node_modules/.bin/openapi-generator-cli{(EnvironmentInfo.IsWin ? ".cmd" : "")}");

    public static void GenerateClient(string openApiSpec, string outputFolder)
    {
        List<string> arguments =
        [
            $"generate",
            $"--openapitools {BuildPaths.Generator.ToolConfiguration}",
            $"--config {BuildPaths.Generator.GeneratorConfiguration}",
            $"--generator-name csharp",
            $"--template-dir {BuildPaths.Generator.Templates}",
            $"--type-mappings date-span='TimeSpan'",
            $"--global-property apiTests=false",
            $"--output {outputFolder}",
            $"--input-spec {openApiSpec}"
        ];

        var environmentVariables = GetEnvironmentVariables();
        environmentVariables.Add("JAVA_OPTS", "-Dlog.level=error");
        _openapiGeneratorCmd(string.Join(" ", arguments), environmentVariables: environmentVariables);
    }

    private static Dictionary<string, string?> GetEnvironmentVariables()
        => Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .ToDictionary(x => (string)x.Key, x => (string?)x.Value);
}