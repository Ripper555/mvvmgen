// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MvvmGen.Inspectors;
using MvvmGen.Model;
using MvvmGen.SourceGenerators.Model;

namespace MvvmGen;

/// <summary>
/// Receives all the classes that have the MvvmGen.ViewModelAttribute set.
/// </summary>
internal class ViewModelSyntaxReceiver : ISyntaxContextReceiver
{
    public List<ViewModelToGenerate> ViewModelsToGenerate { get; } = new();

    public List<ServiceClass> ServiceClasses { get; } = new();

    /// <summary>
    /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
    /// </summary>
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } classDeclarationSyntax)
        {
            var viewModelClassSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            var viewModelAttributeData = viewModelClassSymbol?.GetAttributes().SingleOrDefault(x => x.AttributeClass?.ToDisplayString() == "MvvmGen.ViewModelAttribute");

            if (viewModelClassSymbol is null || viewModelAttributeData is null) return;

            var (commandsToGenerate,
                commandsToInvalidateByPropertyName,
                propertiesToGenerate,
                propertyInvalidationsByGeneratedPropertyName) = ViewModelMemberInspector.Inspect(viewModelClassSymbol);

            var (generateConstructor, validation) = ViewModelAttributeInspector.Inspect(viewModelAttributeData);

            var viewModelToGenerate = new ViewModelToGenerate(viewModelClassSymbol)
            {
                InjectionsToGenerate = ViewModelInjectAttributeInspector.Inspect(viewModelClassSymbol),
                GenerateConstructor = generateConstructor,
                Validation = validation,
                ViewModelFactoryToGenerate = ViewModelGenerateFactoryAttributeInspector.Inspect(viewModelClassSymbol),
                CommandsToGenerate = commandsToGenerate,
                PropertiesToGenerate = propertiesToGenerate,
                CommandsToInvalidateByPropertyName = commandsToInvalidateByPropertyName
            };

            viewModelToGenerate.WrappedModelType =
                ModelMemberInspector.Inspect(viewModelAttributeData, viewModelToGenerate.PropertiesToGenerate);

            SetPropertiesToInvalidatePropertyOnPropertiesToGenerate(viewModelToGenerate.PropertiesToGenerate,
                propertyInvalidationsByGeneratedPropertyName);

            viewModelToGenerate.IsEventSubscriber = viewModelClassSymbol.Interfaces.Any(x =>
                x.ToDisplayString().StartsWith("MvvmGen.Events.IEventSubscriber"));

            ViewModelsToGenerate.Add(viewModelToGenerate);
        }

        if (context.Node is ClassDeclarationSyntax classDeclarationSyntax2)
        {
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax2);
            if (classSymbol is null || !classSymbol.Interfaces.Any())
                return;

            ServiceClasses.Add(new ServiceClass(classSymbol, classSymbol.Interfaces));
        }

    }

    private void SetPropertiesToInvalidatePropertyOnPropertiesToGenerate(IList<PropertyToGenerate> propertiesToGenerate,
        Dictionary<string, List<string>> propertyInvalidationsByGeneratedPropertyName)
    {
        foreach (var propertiesToInvalidate in propertyInvalidationsByGeneratedPropertyName)
        {
            var propertyToGenerate = propertiesToGenerate.SingleOrDefault(x => x.PropertyName == propertiesToInvalidate.Key);
            if (propertyToGenerate is not null)
            {
                propertyToGenerate.PropertiesToInvalidate = propertiesToInvalidate.Value;
            }
        }
    }
}
