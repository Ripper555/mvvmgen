﻿// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using Xunit;

namespace MvvmGen.SourceGenerators
{
    public class CommandInvalidateAttributeTests : ViewModelGeneratorTestsBase
    {
        [InlineData(true, true, false, "[CommandInvalidate(nameof(FirstName),nameof(LastName))]")]
        [InlineData(true, true, false, "[CommandInvalidate(\"FirstName\",\"LastName\")]")]
        [InlineData(true, true, false, "[CommandInvalidate(nameof(FirstName))]\r\n[CommandInvalidate(nameof(LastName))]")]
        [InlineData(true, true, false, "[CommandInvalidate(\"FirstName\")]\r\n[CommandInvalidate(\"LastName\"))]")]
        [InlineData(true, false, false, "[CommandInvalidate(nameof(FirstName))]")]
        [InlineData(true, false, false, "[CommandInvalidate(\"FirstName\")]")]
        [InlineData(true, false, true, "[CommandInvalidate(nameof(FirstName))]")]
        [InlineData(true, false, true, "[CommandInvalidate(\"FirstName\")]")]
        [Theory]
        public void RaiseCanExecuteChangedInFirstNameProperty(bool isCallInFirstNamePropExpected, bool isCallInLastNamePropExpected,
            bool putAttributeOnExecuteMethod, string commandInvalidateAttribute)
        {
            var commandCall = @"                    SaveCommand.RaiseCanExecuteChanged();
";

            var expectedCallInFirstNameProp = isCallInFirstNamePropExpected ? commandCall : "";
            var expectedCallInLastNameProp = isCallInLastNamePropExpected ? commandCall : "";

            ShouldGenerateExpectedCode(
      $@"using MvvmGen;

namespace MyCode
{{
  [ViewModel]
  public partial class EmployeeViewModel
  {{
    [Property] string _firstName;
    [Property] string _lastName;
    
    {(putAttributeOnExecuteMethod ? commandInvalidateAttribute : "")}
    [Command(nameof(CanSave))]
    public void Save() {{ }}

    {(putAttributeOnExecuteMethod ? "" : commandInvalidateAttribute)}
    public bool CanSave() => true;
  }}
}}",
      $@"{AutoGeneratedComment}
{AutoGeneratedUsings}

namespace MyCode
{{
    partial class EmployeeViewModel : ViewModelBase
    {{
        public EmployeeViewModel()
        {{
            this.InitializeCommands();
            this.OnInitialize();
        }}

        partial void OnInitialize();

        private void InitializeCommands()
        {{
            SaveCommand = new DelegateCommand(_ => Save(), _ => CanSave());
        }}

        public DelegateCommand SaveCommand {{ get; private set; }}

        public string FirstName
        {{
            get => _firstName;
            set
            {{
                if (_firstName != value)
                {{
                    _firstName = value;
                    OnPropertyChanged(""FirstName"");
{expectedCallInFirstNameProp}                }}
            }}
        }}

        public string LastName
        {{
            get => _lastName;
            set
            {{
                if (_lastName != value)
                {{
                    _lastName = value;
                    OnPropertyChanged(""LastName"");
{expectedCallInLastNameProp}                }}
            }}
        }}
    }}
}}
");
        }
    }
}
