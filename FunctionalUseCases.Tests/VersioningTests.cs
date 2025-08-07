using System.Reflection;

namespace FunctionalUseCases.Tests;

public class VersioningTests
{
    [Fact]
    public void Assembly_ShouldHaveVersionInformation()
    {
        // Arrange
        var assembly = typeof(Execution).Assembly;

        // Act
        var version = assembly.GetName().Version;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

        // Assert
        Assert.NotNull(version);
        Assert.NotNull(informationalVersion);
        Assert.NotNull(fileVersion);

        // Version should be at least 1.0.0.0 (as configured in version.json)
        Assert.True(version.Major >= 1);
        Assert.True(version.Minor >= 0);

        // Informational version should contain git commit information (format: 1.0.0+gitcommit)
        Assert.Contains("+", informationalVersion);
    }

    [Fact]
    public void ThisAssembly_ShouldProvideVersionConstants()
    {
        // This test verifies that Nerdbank.GitVersioning generates the ThisAssembly class
        // We'll use reflection to access it since it's internal

        // Arrange
        var assembly = typeof(Execution).Assembly;
        var thisAssemblyType = assembly.GetType("ThisAssembly");

        // Act & Assert
        Assert.NotNull(thisAssemblyType);

        // Check that expected constants are present
        var assemblyVersionField = thisAssemblyType.GetField("AssemblyVersion", BindingFlags.Static | BindingFlags.NonPublic);
        var gitCommitIdField = thisAssemblyType.GetField("GitCommitId", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(assemblyVersionField);
        Assert.NotNull(gitCommitIdField);

        var assemblyVersion = assemblyVersionField?.GetValue(null) as string;
        var gitCommitId = gitCommitIdField?.GetValue(null) as string;

        Assert.NotNull(assemblyVersion);
        Assert.NotNull(gitCommitId);
        Assert.NotEmpty(assemblyVersion);
        Assert.NotEmpty(gitCommitId);
    }
}