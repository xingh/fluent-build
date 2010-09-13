﻿using System;
using System.IO;
using System.Reflection;

namespace FluentBuild.BuildExe
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: fb.exe buildassembly.dll [BuildClass]");
                Console.WriteLine("OR");
                Console.WriteLine("fb.exe PathToSources [BuildClass]");
                Console.WriteLine();
                Console.WriteLine("BuildClass: Optional, the class to run. If none is specified then \"Default\" is assumed");
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            MessageLogger.ShowDebugMessages = true;

            var classToRun = "Default";
            if (args.Length > 1)
                classToRun = args[1];

            string pathToAssembly = Path.Combine(Environment.CurrentDirectory, args[0]);
            if (Path.GetExtension(args[0]).ToLower() != "dll")
            {
                Console.WriteLine("building task from sources");
                pathToAssembly = BuildAssemblyFromSources(pathToAssembly);
            }

            ExecuteBuildTask(pathToAssembly, classToRun);
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Environment.ExitCode = 1;
            MessageLogger.Write("ERROR", "An unexpected error has occurred. Details:" + e.ExceptionObject);
            Environment.Exit(1);
        }

        /// <summary>
        /// Builds an assembly from a source folder. Currently this only works with .cs files
        /// </summary>
        /// <param name="path">The path to the source files</param>
        /// <returns>returns the path to the compiled assembly</returns>
        private static string BuildAssemblyFromSources(string path)
        {
            Console.WriteLine("Press enter key to start");
            Console.ReadLine();
            MessageLogger.WriteDebugMessage("Sources found in: " + path);
            var fileset = new FileSet();
            fileset.Include(path + "\\**\\*.cs");

            string startPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");

            string dllReference = Path.Combine(startPath, "FluentBuild.dll");
            MessageLogger.WriteDebugMessage("Adding in reference to the FluentBuild DLL from: " + dllReference);
            string outputAssembly = Path.Combine(path, "build.dll");
            MessageLogger.WriteDebugMessage("Output Assembly: " + outputAssembly);
            Build.UsingCsc.AddSources(fileset).AddRefences(dllReference).OutputFileTo(outputAssembly).Target.Library.Execute();
            return outputAssembly;
        }

        /// <summary>
        /// Executes a DLL.
        /// </summary>
        /// <param name="path">The path to the DLL that has a class that implements IBuild</param>
        /// <param name="classToRun"></param>
        private static void ExecuteBuildTask(string path, string classToRun)
        {
            MessageLogger.WriteDebugMessage("Executing DLL build from " + path);
            Assembly assemblyInstance = Assembly.LoadFile(path);
            Type[] types = assemblyInstance.GetTypes();
            foreach (Type t in types)
            {
                if (t.Name == classToRun)
                {
                    StartRun(assemblyInstance, t);
                }
                
            }
        }

        private static void StartRun(Assembly assemblyInstance, Type t)
        {
                var build = (BuildFile)assemblyInstance.CreateInstance(t.FullName);
                MessageLogger.WriteHeader("Execute");
                MessageLogger.Write("EXECUTE", "Running Class: " + t.FullName);
                build.InvokeNextTask();

            //Type[] interfaces = t.GetInterfaces();
            //foreach (Type i in interfaces)
            //{
            //    if (i.FullName == typeof(IBuild).FullName)
            //    {
            //        var build = (IBuild)assemblyInstance.CreateInstance(t.FullName);
            //        MessageLogger.WriteHeader("Execute");
            //        MessageLogger.Write("EXECUTE", "Running Class: " + t.FullName);
            //        build.Execute();
            //    }
            //}
        }
    }
}