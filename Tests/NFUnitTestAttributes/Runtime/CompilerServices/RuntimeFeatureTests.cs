// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using nanoFramework.TestFramework;

namespace NFUnitTestAttributes.Runtime.CompilerServices
{
    [TestClass]
    public class RuntimeFeatureTests
    {
        [TestMethod]
        public void UnmanagedSignatureCallingConventionIsSupported()
        {
            Assert.AreEqual("UnmanagedSignatureCallingConvention", RuntimeFeature.UnmanagedSignatureCallingConvention);
            Assert.IsTrue(RuntimeFeature.IsSupported(RuntimeFeature.UnmanagedSignatureCallingConvention));
        }

        [TestMethod]
        public void ExistingFeaturesRemainSupported()
        {
            Assert.IsTrue(RuntimeFeature.IsSupported(RuntimeFeature.DefaultImplementationsOfInterfaces));
            Assert.IsTrue(RuntimeFeature.IsSupported(RuntimeFeature.ByRefLikeGenerics));
        }

        [TestMethod]
        public void UnknownFeaturesAreNotSupported()
        {
            Assert.IsFalse(RuntimeFeature.IsSupported("UnknownFeature"));
            Assert.IsFalse(RuntimeFeature.IsSupported(string.Empty));
            Assert.IsFalse(RuntimeFeature.IsSupported(null));
        }
    }
}
