using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators_ns21
{
    [Generator]
    public class TimingGeneratorExecute:ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // 不需要初始化
        }

        public void Execute(GeneratorExecutionContext context)
        {
           var syntaxTrees = context.Compilation.SyntaxTrees;

            foreach (var tree in syntaxTrees)
            {
                var root = tree.GetRoot();
                var methods = root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m=>m.AttributeLists.Select(a=>a.ToString()).Contains("[Timed]")); //指定 method 上有 `[Timed]` 標記的 
                
                foreach (var method in methods)
                {
                    var methodName = method.Identifier.Text;
                    
                    var containingClass = method.Parent as ClassDeclarationSyntax;

                    if (containingClass != null)
                    {
                        var @namespace = (containingClass.Parent as NamespaceDeclarationSyntax).Name.ToString(); 
                        var className = containingClass.Identifier.Text;
                        
                        var newMethod = $@"
                                        using System;
                                        using System.Diagnostics;

                                        namespace {@namespace}
                                        {{
                                            public partial class {className}
                                            {{
                                                public void Execute_{methodName}()
                                                {{
                                                    long timestamp = Stopwatch.GetTimestamp();
                                                    
                                                    //Console.WriteLine($""正在生成 {@namespace}.{className} 的 Execute_{methodName} 方法"");
                                                    {methodName}();

                                           
                                                    Console.WriteLine($""Execute_{methodName} Elapsed time: {{Stopwatch.GetElapsedTime(timestamp)}} "");
                                                }}
                                            }}
                                        }}";

                        context.AddSource($"Execute_{containingClass.Identifier.Text}_{methodName}.g.cs",
                            SourceText.From(newMethod, Encoding.UTF8));
                    }
                }
            }
        }
    }
}