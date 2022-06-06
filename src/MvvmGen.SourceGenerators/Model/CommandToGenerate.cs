// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

namespace MvvmGen.Model
{
    internal class CommandToGenerate
    {
        public CommandToGenerate(MethodInfo executeMethod, string propertyName, bool isSafe)
        {
            ExecuteMethod = executeMethod;
            PropertyName = propertyName;
            IsSafeCommand = isSafe;
        }

        public MethodInfo ExecuteMethod { get; }

        public string PropertyName { get; }

        public MethodInfo? CanExecuteMethod { get; set; }

        public bool IsSafeCommand { get; set; } = false;
    }

    internal struct MethodInfo
    {
        public MethodInfo(string name) : this()
        {
            Name = name;
        }

        public string Name { get; set; }

        public bool IsAwaitable { get; set; }

        public bool HasParameter { get; set; }
        public string ParameterType { get; set; }
    }
}
