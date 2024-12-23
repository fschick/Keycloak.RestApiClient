using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;

public static class Npm
{
    public static void Restore(string path)
        => NpmTasks.NpmCi(settings => settings
            .SetProcessWorkingDirectory(path)
            .AddProcessAdditionalArguments(
                "--prefer-offline",
                "--no-audit"
             ));
}