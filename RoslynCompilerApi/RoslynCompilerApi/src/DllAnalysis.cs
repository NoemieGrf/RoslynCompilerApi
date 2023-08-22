using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynCompilerApi;

public class DllAnalysis
{
    public static void Run()
    {
        string dllPath = "../../../input/input.dll";
        
        var reference = MetadataReference.CreateFromFile(dllPath);
        
        var compilation = CSharpCompilation.Create("Temp")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(reference);

        var assemblySymbol = (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(reference)!;

        List<INamedTypeSymbol> allTypes = GetAllTypes(assemblySymbol.GlobalNamespace).ToList();
        
        foreach (var type in allTypes)
        {
            Console.WriteLine(type);

            var allMembers = type.GetMembers();
            foreach (var member in allMembers)
            {
                if (member is IMethodSymbol methodMember)
                {
                    Console.WriteLine($"{type}: {methodMember.Name}");
                }
            }
        }
    }
    
    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceOrTypeSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            if (member is INamedTypeSymbol namedTypeSymbol)
            {
                yield return namedTypeSymbol;
            }

            if (member is INamespaceSymbol namespaceSymbol)
            {
                foreach (var type in GetAllTypes(namespaceSymbol))
                {
                    yield return type;
                }
            }
        }
    }
}