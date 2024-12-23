using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using System.IO;
using System.Text.RegularExpressions;

public static class Project
{
    public static void AdjustImports(string projectFile)
    {
        const string projectImports = """
            <Import Project="../../../build/targets/net_standard.props" />
            <Import Project="../../../build/targets/version.props" />
            <Import Project="../../../build/targets/nuget.props" />

            <PropertyGroup>
            <NoWarn>1701;1702;IDE0079;CS0472;CS0612</NoWarn>
            </PropertyGroup>

            <ItemGroup>
            <None Include="../../../FS.Keycloak.RestApiClient.png" Pack="true" PackagePath="Schick.Keycloak.RestApiClient.png"/>
            <None Include="../../README.md" Pack="true" PackagePath="README.md" />
            </ItemGroup>
            """;

        var project = File.ReadAllText(projectFile);

        project = Regex.Replace(project, "<PropertyGroup>.*<\\/PropertyGroup>", string.Empty, RegexOptions.Singleline);
        project = Regex.Replace(project, "<ItemGroup>((?!PackageReference).)*?<\\/ItemGroup>", string.Empty, RegexOptions.Singleline);
        project = Regex.Replace(project, "<Project Sdk=\"Microsoft.NET.Sdk\">", $"<Project Sdk=\"Microsoft.NET.Sdk\">\n{projectImports}");

        File.WriteAllText(projectFile, project);
    }

    public static void CopyReadme(string source, string destination)
        => File.Copy(source, destination, true);

    public static void Build(string projectFile, string version)
    {
        var fileVersion = GetFileVersion(version);

        DotNetTasks.DotNetBuild(settings => settings
            .SetProjectFile(projectFile)
            .SetConfiguration(Configuration.Debug)
        );

        DotNetTasks.DotNetBuild(settings => settings
            .SetProjectFile(projectFile)
            .SetConfiguration(Configuration.Release)
            .SetWarningsAsErrors()
            .SetVersion(version)
            .SetFileVersion(fileVersion)
        );
    }

    public static void PackNuget(string projectFile, string version, string? publishFolder = null)
    {
        var fileVersion = GetFileVersion(version);

        DotNetTasks.DotNetPack(settings => settings
            .SetProject(projectFile)
            .SetConfiguration(Configuration.Release)
            .SetVersion(version)
            .SetFileVersion(fileVersion)
            .SetOutputDirectory(publishFolder)
        );
    }

    public static void PushNuget(AbsolutePath publishFolder, string? nugetApiKey, string? nugetServerUrl = null)
    {
        if (!Path.IsPathRooted(publishFolder))
            publishFolder = NukeBuild.RootDirectory + publishFolder;

        nugetServerUrl ??= "https://api.nuget.org/v3/index.json";

        var nuGetPackages = publishFolder.GlobFiles("*.nupkg");

        DotNetTasks.DotNetNuGetPush(settings => settings
            .SetSource(nugetServerUrl)
            .SetApiKey(nugetApiKey)
            .CombineWith(nuGetPackages, (settings, package) => settings.SetTargetPath(package))
        );
    }

    private static string GetFileVersion(string version)
        => Regex.Replace(version, @"(\d+(?:\.\d+)*)(.*)", "$1");
}