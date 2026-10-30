// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Defines APIs to determine whether specific features are supported by the common language runtime.
    /// </summary>
    public static partial class RuntimeFeature
    {
        /// <summary>
        /// Indicates that this version of runtime supports default interface method implementations.
        /// </summary>
        public const string DefaultImplementationsOfInterfaces = nameof(DefaultImplementationsOfInterfaces);

        /// <summary>
        /// Represents a runtime feature where byref-like types can be used in Generic parameters.
        /// </summary>
        public const string ByRefLikeGenerics = nameof(ByRefLikeGenerics);

        /// <summary>
        /// Identifies support for unmanaged calling conventions in function pointer signatures.
        /// </summary>
        /// <remarks>
        /// Enables compiler support for these signatures. Invoking unmanaged function pointers
        /// requires corresponding support in the native runtime.
        /// </remarks>
        public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);

        /// <summary>
        /// Checks whether a certain feature is supported by the Runtime.
        /// </summary>
        public static bool IsSupported(string feature)
        {
            return feature switch
            {
                ByRefLikeGenerics or
                UnmanagedSignatureCallingConvention or
                DefaultImplementationsOfInterfaces => true,
                _ => false,
            };
        }
    }
}
