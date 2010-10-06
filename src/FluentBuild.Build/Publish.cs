﻿using FluentBuild.Core;
using FluentBuild.Publishing;

namespace Build
{
    public class Publish : Default
    {
        internal readonly BuildArtifact AssemblyFluentBuildRelease;
        internal readonly BuildArtifact AssemblyFluentBuildRunnerRelease;
        internal string _finalFileName;
        internal BuildArtifact ZipFilePath;

        public Publish()
        {
            AssemblyFluentBuildRelease = directory_compile.File("FluentBuild.dll");
            AssemblyFluentBuildRunnerRelease = directory_compile.File("fb.exe");
            _finalFileName = "FluentBuild-Alpha-" + _version + ".zip";
            ZipFilePath = directory_release.File(_finalFileName);

            AddTask(Clean);
            AddTask(CompileCoreWithOutTests);
            AddTask(CompileRunner);
            AddTask(Compress);
            //move to tools folder here?
            AddTask(PublishToRepository);
        }

        private void CompileRunner()
        {
            FileSet sourceFiles = new FileSet()
                .Include(directory_base.SubFolder("src").SubFolder("FluentBuild.BuildExe"))
                .RecurseAllSubDirectories.Filter("*.cs");

            FluentBuild.Core.Build.UsingCsc.Target.Executable
                .AddSources(sourceFiles)
                .AddRefences(AssemblyFluentBuildRelease)
                
                .OutputFileTo(AssemblyFluentBuildRunnerRelease)
                .Execute();
        }

        private void PublishToRepository()
        {
            FluentBuild.Core.Publish.ToGoogleCode.LocalFileName(ZipFilePath.ToString())
             .UserName(Properties.CommandLineProperties.GetProperty("GoogleCodeUsername"))
             .Password(Properties.CommandLineProperties.GetProperty("GoogleCodePassword"))
             .ProjectName("fluent-build")
             .Summary("Alpha Release (v" + _version + ")")
             .TargetFileName(_finalFileName)
             .Upload();
        }

        private void CompileCoreWithOutTests()
        {
            FileSet sourceFiles = new FileSet()
                .Include(directory_base.SubFolder("src").SubFolder("FluentBuild"))
                             .RecurseAllSubDirectories.Filter("*.cs")
                .Exclude(directory_base.SubFolder("src").SubFolder("FluentBuild"))
                             .RecurseAllSubDirectories.Filter("*Tests.cs");

            FluentBuild.Core.Build.UsingCsc.Target.Library
                .AddSources(sourceFiles)
                .AddRefences(thirdparty_sharpzip)
                .OutputFileTo(AssemblyFluentBuildRelease)
                .Execute();
        }

        private void Compress()
        {
            thirdparty_sharpzip.Copy.To(directory_compile);
            Run.Zip.Compress.SourceFolder(directory_compile).To(ZipFilePath);
        }
    }
}