using Nuke.Common;
using Nuke.Common.IO;

public static class BuildPaths
{
    public static AbsolutePath Readme => NukeBuild.RootDirectory / "README.md";

    public static class OpenApiJson
    {
        public static readonly AbsolutePath Downloaded = NukeBuild.RootDirectory / "keycloak.openapi.json";
        public static readonly AbsolutePath Formatted = NukeBuild.RootDirectory / "keycloak.openapi.formatted.json";
        public static readonly AbsolutePath Fixed = NukeBuild.RootDirectory / "keycloak.openapi.fixed.json";
        public static readonly AbsolutePath Escaped = NukeBuild.RootDirectory / "keycloak.openapi.escaped.json";
        public static readonly AbsolutePath Humanized = NukeBuild.RootDirectory / "keycloak.openapi.humanized.json";
    }

    public static class Generator
    {
        public static readonly AbsolutePath Path = NukeBuild.RootDirectory / "build" / "openapi-generator";
        public static readonly AbsolutePath Templates = Path / "templates";
        public static readonly AbsolutePath ToolConfiguration = Path / "openapitools.json";
        public static readonly AbsolutePath GeneratorConfiguration = Path / "openapi-generator.config.json";
    }

    public static class Client
    {
        public static readonly AbsolutePath Path = NukeBuild.RootDirectory / "FS.Keycloak.RestApiClient";
        public static readonly AbsolutePath ProjectFile = Path / "src" / "FS.Keycloak.RestApiClient" / "FS.Keycloak.RestApiClient.csproj";
        public static readonly AbsolutePath Readme = Path / "README.md";
    }
}