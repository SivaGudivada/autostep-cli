﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoStep.CommandLine {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AutoStep.CommandLine.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Step binding failed with one or more errors..
        /// </summary>
        internal static string BindingFailed {
            get {
                return ResourceManager.GetString("BindingFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Binding Steps....
        /// </summary>
        internal static string BindingSteps {
            get {
                return ResourceManager.GetString("BindingSteps", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All steps bound successfully..
        /// </summary>
        internal static string BindingStepsSuccess {
            get {
                return ResourceManager.GetString("BindingStepsSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compilation failed with one or more errors..
        /// </summary>
        internal static string CompilationFailed {
            get {
                return ResourceManager.GetString("CompilationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compiled successfully..
        /// </summary>
        internal static string CompiledSuccessfully {
            get {
                return ResourceManager.GetString("CompiledSuccessfully", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compiling Project....
        /// </summary>
        internal static string CompilingProject {
            get {
                return ResourceManager.GetString("CompilingProject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot have an empty key value for an option argument: &apos;{0}&apos;..
        /// </summary>
        internal static string EmptyKeyValueForOptionArgument {
            get {
                return ResourceManager.GetString("EmptyKeyValueForOptionArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Extensions must have a &apos;package&apos; value containing the package ID..
        /// </summary>
        internal static string ExtensionConfigPackageRequired {
            get {
                return ResourceManager.GetString("ExtensionConfigPackageRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Extensions could not be loaded..
        /// </summary>
        internal static string ExtensionsCouldNotBeLoaded {
            get {
                return ResourceManager.GetString("ExtensionsCouldNotBeLoaded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Local Extensions must have a &apos;folder&apos; value containing the name or path of the extension&apos;s folder..
        /// </summary>
        internal static string LocalExtensionsFolderRequired {
            get {
                return ResourceManager.GetString("LocalExtensionsFolderRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Project Configuration Error: {0}.
        /// </summary>
        internal static string ProjectConfigurationError {
            get {
                return ResourceManager.GetString("ProjectConfigurationError", resourceCulture);
            }
        }
    }
}
