#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace IntegrityTest
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ArxOne.MrAdvice.Advice;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;

    [TestFixture]
    [Category("Build")]
    public class IncrementalWeavingTest
    {
        private const string UpToDate = "Skipping target \"MrAdviceWeaver\" because all output files are up-to-date";

        /// <summary>
        /// The type the weaver injects into every assembly it weaves.
        /// </summary>
        private static readonly byte[] WeavingMarker = Encoding.UTF8.GetBytes("⚡Invocation");

        private string _projectDirectory;

        [SetUp]
        public void CreateWeavedProject()
        {
            _projectDirectory = Path.Combine(Path.GetTempPath(), "MrAdvice.Incremental." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_projectDirectory);

            // A Directory.Build.props or NuGet.config above the temp directory would otherwise apply to the build.
            foreach (var barrier in new[] { "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props" })
                File.WriteAllText(Path.Combine(_projectDirectory, barrier), "<Project />");
            File.WriteAllText(Path.Combine(_projectDirectory, "NuGet.config"), "<configuration />");

            var targets = Path.Combine(GetRepositoryRoot(), "MrAdvice.Weaver", "MrAdvice.targets");
            var mrAdvice = typeof(IMethodAdvice).Assembly.Location;

            File.WriteAllText(Path.Combine(_projectDirectory, "Weaved.csproj"), $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>{GetTargetFramework()}</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <Reference Include="MrAdvice">
                      <HintPath>{mrAdvice}</HintPath>
                    </Reference>
                  </ItemGroup>
                  <Import Project="{targets}" />
                </Project>
                """);

            File.WriteAllText(Path.Combine(_projectDirectory, "Weaved.cs"), """
                using System;
                using ArxOne.MrAdvice.Advice;

                public class EmptyAdvice : Attribute, IMethodAdvice
                {
                    public void Advise(MethodAdviceContext context) => context.Proceed();
                }

                public class Weaved
                {
                    [EmptyAdvice]
                    public void Method() { }
                }
                """);
        }

        [TearDown]
        public void DeleteWeavedProject()
        {
            // A failed test keeps the project, its build output and its stamps for diagnosis.
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed)
                return;

            try
            {
                Directory.Delete(_projectDirectory, true);
            }
            catch (Exception)
            {
            }
        }

        [Test]
        public void WeaverIsSkippedWhenNothingRecompiled()
        {
            Build();
            var wovenAt = GetStampTime();

            var secondBuild = Build();
            Assert.That(secondBuild, Does.Contain(UpToDate),
                "The weaver ran again even though nothing was recompiled.");
            Assert.That(GetStampTime(), Is.EqualTo(wovenAt), "The stamp was updated by a skipped weaving.");

            var thirdBuild = Build();
            Assert.That(thirdBuild, Does.Contain(UpToDate),
                "The weaver ran again on a second no-op build.");
            Assert.That(GetStampTime(), Is.EqualTo(wovenAt), "The stamp did not survive the no-op builds.");

            AssertAssemblyIsWeaved();
        }

        [Test]
        public void WeaverRunsAgainAfterSourceChanged()
        {
            Build();
            Build();
            var wovenAt = GetStampTime();

            File.AppendAllText(Path.Combine(_projectDirectory, "Weaved.cs"), Environment.NewLine + "// touched");

            Build();
            Assert.That(GetStampTime(), Is.GreaterThan(wovenAt),
                "The weaver was skipped after the assembly was recompiled.");
            AssertAssemblyIsWeaved();
        }

        [Test]
        public void WeaverRunsAgainAfterWeaverChanged()
        {
            Build();
            Build();
            var wovenAt = GetStampTime();

            // The weaver is an input, so a rebuilt weaver must re-weave assemblies it wove before.
            var weaver = GetWeaverAssembly();
            var weaverWrittenAt = File.GetLastWriteTimeUtc(weaver);
            try
            {
                File.SetLastWriteTimeUtc(weaver, DateTime.UtcNow);

                Build();
                Assert.That(GetStampTime(), Is.GreaterThan(wovenAt),
                    "The weaver was skipped after the weaver itself changed.");
                AssertAssemblyIsWeaved();
            }
            finally
            {
                File.SetLastWriteTimeUtc(weaver, weaverWrittenAt);
            }
        }

        /// <summary>
        /// Asserts the built assembly carries the type the weaver injects, so that skipping never ships an unweaved assembly.
        /// </summary>
        private void AssertAssemblyIsWeaved()
        {
            var assembly = Directory.GetFiles(Path.Combine(_projectDirectory, "bin"), "Weaved.dll", SearchOption.AllDirectories).Single();
            var bytes = File.ReadAllBytes(assembly);
            Assert.That(Contains(bytes, WeavingMarker), Is.True, "The built assembly was not weaved.");
        }

        private static bool Contains(byte[] bytes, byte[] value)
        {
            for (var index = 0; index <= bytes.Length - value.Length; index++)
            {
                if (new ReadOnlySpan<byte>(bytes, index, value.Length).SequenceEqual(value))
                    return true;
            }
            return false;
        }

        private DateTime GetStampTime()
        {
            var stamps = Directory.GetFiles(Path.Combine(_projectDirectory, "obj"), "*.MrAdvice.stamp", SearchOption.AllDirectories);
            Assert.That(stamps, Is.Not.Empty, "The weaver did not run.");
            return File.GetLastWriteTimeUtc(stamps.Single());
        }

        private string Build()
        {
            var processStartInfo = new ProcessStartInfo("dotnet", "build --verbosity detailed --nologo /nodeReuse:false")
            {
                WorkingDirectory = _projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            // The test host runs under MSBuild, whose environment would make the nested build resolve the wrong SDK.
            foreach (var inherited in new[] { "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBUILD_EXE_PATH", "MSBuildLoadMicrosoftTargetsReadOnly" })
                processStartInfo.Environment.Remove(inherited);
            // The assertions match MSBuild messages, which are localized.
            processStartInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";
            processStartInfo.Environment["VSLANG"] = "1033";

            using var process = Process.Start(processStartInfo);
            Assert.That(process, Is.Not.Null, "dotnet could not be started.");

            var error = process.StandardError.ReadToEndAsync();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Assert.That(process.ExitCode, Is.Zero, "Build failed:" + Environment.NewLine + output + error.Result);
            return output;
        }

        /// <summary>
        /// The framework the test runs on, which is also the one the weaver is picked for.
        /// </summary>
        private static string GetTargetFramework()
        {
            return "net" + Environment.Version.Major + ".0";
        }

        private static string GetWeaverAssembly()
        {
            var weaver = Path.Combine(GetRepositoryRoot(), "tools", GetTargetFramework(), "MrAdvice.Weaver.dll");
            Assert.That(weaver, Does.Exist, "The weaver was not built for the framework the test runs on.");
            return weaver;
        }

        private static string GetRepositoryRoot()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "MrAdvice.slnx")))
                    return directory.FullName;
            }

            throw new InvalidOperationException("The repository root was not found above " + AppContext.BaseDirectory);
        }
    }
}
