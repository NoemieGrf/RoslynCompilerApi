using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;

namespace RoslynCompilerApi;

public class SolutionAnalysis
{
    public static void Run()
    {
        VisualStudioInstance instance = MSBuildLocator.QueryVisualStudioInstances().First();
        MSBuildLocator.RegisterInstance(instance);
        
        string slnPath = "../../../../../ToBeAnalyzedSolution/ToBeAnalyzedSolution.sln";
        string fullPath = Path.GetFullPath(slnPath);
        
        using var workspace = MSBuildWorkspace.Create();
        var solution = workspace.OpenSolutionAsync(fullPath).Result;
        
        foreach (var project in solution.Projects)
        {
            Console.WriteLine($"Project Name: {project.Name}, Doc Nums: {project.Documents.Count()}");
            
            foreach (var document in project.Documents)
            {
                Console.WriteLine($"Document Name: {document.Name}");
                
                var model = document.GetSemanticModelAsync().Result;
                var root = model.SyntaxTree.GetRoot();
                
                var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

                foreach (var methodDeclaration in methodDeclarations)
                {
                    // ...
                }
            }
        }
    }
}