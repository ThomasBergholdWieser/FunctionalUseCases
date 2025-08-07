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
        version.ShouldNotBeNull();
        informationalVersion.ShouldNotBeNull();
        fileVersion.ShouldNotBeNull();
        
        // Version should be at least 1.0.0.0 (as configured in version.json)
        (version.Major >= 1).ShouldBeTrue();
        (version.Minor >= 0).ShouldBeTrue();
        
        // Informational version should contain git commit information (format: 1.0.0+gitcommit)
        informationalVersion.ShouldContain("+");
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
        thisAssemblyType.ShouldNotBeNull();
        
        // Check that expected constants are present
        var assemblyVersionField = thisAssemblyType.GetField("AssemblyVersion", BindingFlags.Static | BindingFlags.NonPublic);
        var gitCommitIdField = thisAssemblyType.GetField("GitCommitId", BindingFlags.Static | BindingFlags.NonPublic);
        
        assemblyVersionField.ShouldNotBeNull();
        gitCommitIdField.ShouldNotBeNull();
        
        var assemblyVersion = assemblyVersionField?.GetValue(null) as string;
        var gitCommitId = gitCommitIdField?.GetValue(null) as string;
        
        assemblyVersion.ShouldNotBeNull();
        gitCommitId.ShouldNotBeNull();
        assemblyVersion.ShouldNotBeEmpty();
        gitCommitId.ShouldNotBeEmpty();
    }
}