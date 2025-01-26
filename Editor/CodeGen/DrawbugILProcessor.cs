using System.Diagnostics;
using System.Linq;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Editor.CodeGen
{
    public class DrawbugILProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            // Know assemblies
            if (compiledAssembly.Name.StartsWith("Unity."))
                 return false;
            if (compiledAssembly.Name.StartsWith("UnityEngine."))
                return false;
            if (compiledAssembly.Name.StartsWith("UnityEditor."))
                return false;
            if (compiledAssembly.Name.Equals("Drawbug"))
                return false;
            if (compiledAssembly.Name.Equals("Drawbug.Editor"))
                return false;
            if (compiledAssembly.Name.Equals("Drawbug.Editor.Tests"))
                return false;
            
            // Only assemblies that reference Drawbug.
            if (!compiledAssembly.References.Any(s => s.EndsWith("Drawbug.dll")))
                return false;

            return true;
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var watch = new Stopwatch();
            watch.Start();
            if (!WillProcess(compiledAssembly))
                return null!;
            
            var context = new ProcessorContext(compiledAssembly);
            if (!context.IsValid)
                return null!;
            
            context.LogWarning($"Processing: {compiledAssembly.Name} with defines: {string.Join(", ", compiledAssembly.References)}");
            foreach (var typeDefinition in context.AssemblyDefinition.MainModule.Types)
            {
                context.LogWarning($"Processing Type: {typeDefinition.Name}");
            }
            
            watch.Stop();
            context.LogWarning($"Process executed in {watch.ElapsedMilliseconds}ms");
            return context.GetResult();
        }
    }
}