using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
class Build : NukeBuild
{
    [GitRepository]
    private readonly GitRepository Repository = default!;

    [Parameter]
    private string Version = "0.0.0";

    [Parameter("Path or download URL for Open API definition to build client for")]
    private readonly string OpenApiJson = "https://www.keycloak.org/docs-api/latest/rest-api/openapi.json";

    [Parameter]
    private AbsolutePath PublishFolder = default!;

    [Parameter, Secret]
    private readonly string? NuGetApiKey;

    protected override void OnBuildInitialized()
    {
        base.OnBuildInitialized();

        if (Version == "0.0.0" && Repository.Tags.Count > 0)
            Version = Repository.Tags.First();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (PublishFolder == null)
            PublishFolder = RootDirectory / $"Keycloak.RestApiClient.{Version}";
    }

    public static int Main()
        => Execute<Build>(x => x.BuildClient);

    public Target Restore => target => target
        .Executes(() =>
        {
            Npm.Restore(BuildPaths.Generator.Path);
        });

    public Target ProvideOpenApiSpec => target => target
        .Executes(() =>
        {
            OpenApiSpec.Download(OpenApiJson, BuildPaths.OpenApiJson.Downloaded);
            OpenApiSpec.Reformat(BuildPaths.OpenApiJson.Downloaded, BuildPaths.OpenApiJson.Formatted);
            OpenApiSpec.ApplyFixes(BuildPaths.OpenApiJson.Formatted, BuildPaths.OpenApiJson.Fixed);
            OpenApiSpec.EscapeForXmlDocumentation(BuildPaths.OpenApiJson.Fixed, BuildPaths.OpenApiJson.Escaped);
            OpenApiSpec.HumanizeActionNames(BuildPaths.OpenApiJson.Escaped, BuildPaths.OpenApiJson.Humanized);
        });

    public Target GenerateClient => target => target
        .DependsOn(Restore)
        .DependsOn(ProvideOpenApiSpec)
        .Executes(() =>
        {
            ClientGenerator.GenerateClient(BuildPaths.OpenApiJson.Humanized, BuildPaths.Client.Path);
        });

    public Target BuildClient => target => target
        .DependsOn(GenerateClient)
        .Executes(() =>
        {
            Project.CopyReadme(BuildPaths.Readme, BuildPaths.Client.Readme);
            Project.AdjustImports(BuildPaths.Client.ProjectFile);
            Project.Build(BuildPaths.Client.ProjectFile, Version);
            Project.PackNuget(BuildPaths.Client.ProjectFile, Version, PublishFolder);
        });

    public Target PublishClient => target => target
        .DependsOn(BuildClient)
        .Executes(() =>
        {
            Project.PushNuget(PublishFolder, NuGetApiKey);
        });
}
