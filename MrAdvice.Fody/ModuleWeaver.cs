#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

public class ModuleWeaver
{
    /// <summary>
    /// Gets or sets the module definition (injected by Fody).
    /// </summary>
    /// <value>
    /// The module definition.
    /// </value>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public ModuleDefinition ModuleDefinition { get; set; }

    /// <summary>
    /// Gets or sets the logger at information level (injected by Fody).
    /// </summary>
    /// <value>
    /// The log information.
    /// </value>
    // ReSharper disable once MemberCanBePrivate.Global
    public Action<string> LogInfo { get; set; }
    /// <summary>
    /// Gets or sets the log at warning level (injected by Fody).
    /// </summary>
    /// <value>
    /// The log warning.
    /// </value>
    // ReSharper disable once MemberCanBePrivate.Global
    public Action<string> LogWarning { get; set; }
    /// <summary>
    /// Gets or sets the log at error level (injected by Fody).
    /// </summary>
    /// <value>
    /// The log warning.
    /// </value>
    // ReSharper disable once MemberCanBePrivate.Global
    public Action<string> LogError { get; set; }

    /// <summary>
    /// Gets or sets the assembly resolver (injected by Fody).
    /// </summary>
    /// <value>
    /// The assembly resolver.
    /// </value>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public IAssemblyResolver AssemblyResolver { get; set; }

    /// <summary>
    /// Gets or sets the assembly file path (injected by Fody).
    /// </summary>
    /// <value>
    /// The assembly file path.
    /// </value>
    public string AssemblyFilePath { get; set; }
    /// <summary>
    /// Gets or sets the references (injected by Fody).
    /// </summary>
    /// <value>
    /// The references.
    /// </value>
    public string References { get; set; }


    public void Execute()
    {
        var references = LoadReferences().ToArray();
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        var moduleWeaverType = (from reference in references
                                let weaver = reference.GetType(GetType().Name)
                                where weaver != null
                                select weaver).Single();
        var moduleWeaver = Activator.CreateInstance(moduleWeaverType);
        Bind(this, moduleWeaver);
        var executeMethod = moduleWeaverType.GetMethod(nameof(Execute));
        if (executeMethod == null)
        {
            LogError($"Method {nameof(Execute)} not found in {moduleWeaverType.FullName}");
            return;
        }
        executeMethod.Invoke(moduleWeaver, new object[0]);
    }

    private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        LogInfo($"Trying to resolve {args.Name}");
        var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => ReferencesAssembly(assemblyName, a.GetName()));
        if (loadedAssembly == null)
            LogError($"{args.Name} not resolved");
        return loadedAssembly;
    }

    private void ListAssemblies()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            LogInfo($"Assembly: {assembly.GetName()}");
        }
    }

    private static bool ReferencesAssembly(AssemblyName a, AssemblyName b)
    {
        if (a.Name != b.Name)
            return false;
        if (a.Version != null && a.Version != b.Version)
            return false;
        if (!a.CultureInfo.IsNeutralCulture && !a.CultureInfo.Equals(b.CultureInfo))
            return false;
        if (a.GetPublicKeyToken() == null || b.GetPublicKeyToken() == null)
            return true;
        return a.GetPublicKeyToken().SequenceEqual(b.GetPublicKeyToken());
    }

    /// <summary>
    /// Binds the specified source to target.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    private void Bind(object source, object target)
    {
        var index = new object[0];
        var sourceType = source.GetType();
        foreach (var targetPropertyInfo in target.GetType().GetProperties())
        {
            var sourcePropertyInfo = sourceType.GetProperty(targetPropertyInfo.Name);
            if (sourcePropertyInfo == null)
            {
                LogError($"Source property {targetPropertyInfo.Name} not found.");
                continue;
            }
            if (sourcePropertyInfo.PropertyType != targetPropertyInfo.PropertyType)
            {
                LogError($"Types mismatch for property {targetPropertyInfo.Name} between source and target");
                continue;
            }
            var propertyValue = sourcePropertyInfo.GetValue(source, index);
            targetPropertyInfo.SetValue(target, propertyValue, index);
        }
    }

    /// <summary>
    /// Loads the references.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<Assembly> LoadReferences()
    {
        var assembly = GetType().Assembly;
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (resourceName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                var resourceStream = assembly.GetManifestResourceStream(resourceName);
                var assemblyBytes = new byte[resourceStream.Length];
                resourceStream.Read(assemblyBytes, 0, assemblyBytes.Length);
                yield return Assembly.Load(assemblyBytes);
            }
        }
    }
}
