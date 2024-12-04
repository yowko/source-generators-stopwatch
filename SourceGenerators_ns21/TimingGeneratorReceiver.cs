using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators_ns21
{
    [Generator]
    public class TimingGeneratorReceiver : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new MethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // the generator infrastructure will create a receiver and populate it
            // we can retrieve the populated instance via the context
            MethodSyntaxReceiver methodSyntaxReceiver = (MethodSyntaxReceiver)context.SyntaxReceiver;
            // 遍歷所有候選方法，生成程式碼
            foreach (var method in methodSyntaxReceiver.CandidateMethods)
            {
                var @namespace = (method.Parent.Parent as NamespaceDeclarationSyntax).Name.ToString();
                // // 獲取類別名稱與方法名稱
                var className =
                    (method.Parent as ClassDeclarationSyntax).Identifier.Text;
                var methodName = method.Identifier.Text;
                // 生成新方法的程式碼
                var sourceCode = GenerateTimedMethod(@namespace, className, methodName);
                // 加入到編譯
                context.AddSource($"Receiver_{className}_{methodName}_ExecutionTime.g.cs", sourceCode);
            }
        }

        private SourceText GenerateTimedMethod(string @namespace, string className, string methodName)
        {
            SourceText sourceText = SourceText.From($@"
                                    using System;
                                    using System.Diagnostics;

                                    namespace {@namespace}
                                    {{
                                        public partial class {className}
                                        {{
                                            public void Receiver_{methodName}()
                                            {{
                                                long timestamp = Stopwatch.GetTimestamp();
                                                
                                                //Console.WriteLine($""正在生成 {@namespace}.{className} 的 Receiver_{methodName} 方法"");
                                                {methodName}();

                                                Console.WriteLine($""Receiver_{methodName} Elapsed time: {{Stopwatch.GetElapsedTime(timestamp)}} "");
                                            }}
                                        }}
                                    }}", Encoding.UTF8);
            return sourceText;
        }
    }

    class MethodSyntaxReceiver : ISyntaxReceiver
    {
        public HashSet<MethodDeclarationSyntax> CandidateMethods { get; } = new HashSet<MethodDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // 篩選出方法節點並檢查是否標記為 [Timed]
            if (syntaxNode is MethodDeclarationSyntax methodDeclaration &&
                methodDeclaration.AttributeLists.Select(a => a.ToString()).Contains("[Timed]"))
            {
                CandidateMethods.Add(methodDeclaration);
            }
        }
    }
}