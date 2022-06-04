// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MvvmGen.Model
{
    /// <summary>
    /// Contains all the details that must be generated for a class that is decorated with the MvvmGen.ViewModelAttribute.
    /// </summary>
    internal class ViewModelToGenerate
    {
        public ViewModelToGenerate(INamedTypeSymbol viewModelClassSymbol)
        {
            ViewModelClassSymbol = viewModelClassSymbol;
        }

        public INamedTypeSymbol ViewModelClassSymbol { get; }

        public string? WrappedModelType { get; set; }

        public bool IsEventSubscriber { get; set; }

        public bool GenerateConstructor { get; set; }

        public ICollection<CommandToGenerate> CommandsToGenerate { get; set; } = new List<CommandToGenerate>();

        public IDictionary<string, List<string>>? CommandsToInvalidateByPropertyName { get; set; }

        public IList<PropertyToGenerate>? PropertiesToGenerate { get; set; }

        public ICollection<InjectionToGenerate> InjectionsToGenerate { get; set; } = new List<InjectionToGenerate>();

        public ViewModelFactoryToGenerate? ViewModelFactoryToGenerate { get; set; }
    }
}
