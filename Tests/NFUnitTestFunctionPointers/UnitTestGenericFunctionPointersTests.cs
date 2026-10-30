// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;

namespace NFUnitTestFunctionPointers
{
    [TestClass]
    public unsafe class UnitTestGenericFunctionPointersTests
    {
        private static T Identity<T>(T value) => value;
        private static object BoxValue<T>(T value) => value;
        private static T[] Wrap<T>(T value) => new T[] { value };
        private static bool IsOfType<T>(object value) => value is T;
        private static T DefaultOf<T>() => default(T);
        private static GenericBox<T> MakeBox<T>(T value) => new GenericBox<T>(value);
        private static string Pair<T1, T2>(T1 first, T2 second) => first.ToString() + "," + second.ToString();
        private static int AreaOf<T>(T shape) where T : IShape => shape.Area();

        private static T OpenIdentity<T>(T value)
        {
            delegate*<T, T> identity = &Identity<T>;
            return identity(value);
        }

        private static T Apply<T>(delegate*<T, T> function, T value) => function(value);
        private static T ApplyNested<T>(delegate*<delegate*<T, T>, T, T> outer, delegate*<T, T> inner, T value) => outer(inner, value);
        private static void ApplyRef<T>(delegate*<ref T, void> function, ref T value) => function(ref value);
        private static int ApplyArray<T>(delegate*<T[], int> function, T[] values) => function(values);
        private static void Sink<T>(T value) { }
        private static delegate*<T, T> MakeIdentity<T>() => &Identity<T>;
        private static delegate*<GenericBox<T>, GenericBox<T>> MakeBoxIdentity<T>() => &Identity<GenericBox<T>>;
        private static GenericBox<T> ApplyBox<T>(delegate*<GenericBox<T>, GenericBox<T>> function, GenericBox<T> value) => function(value);
        private static GenericBox<int> IntBoxIdentity(GenericBox<int> value) => value;
        private static int NestedBoxes(GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<int>>>>>>>>>> value, int other) => other;
        private static void NestedBoxesString(GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<int>>>>>>>>>> value, string other) { }
        private static int Twice(int value) => value * 2;
        private static string Exclaim(string value) => value + "!";
        private static Point SwapPoint(Point point) => new Point(point.Y, point.X);
        private static int CallInt(delegate*<int, int> function, int value) => function(value);
        private static int IgnoreLongPointer(delegate*<long, long> function, int value) => value;
        private static void Increment(ref int value) => value++;
        private static int Length(int[] values) => values.Length;

        private static bool Throws(delegate*<void> action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        private static void CallForgedGenericInstantiation()
        {
            delegate*<int, void> forged = (delegate*<int, void>)(void*)(delegate*<int[], void>)&Sink<int[]>;
            forged(1);
        }

        private static void CallForgedThroughGenericMethod()
        {
            Apply<string>((delegate*<string, string>)(void*)(delegate*<int, int>)&Twice, "x");
        }

        private static void CallForgedThroughGenericType()
        {
            GenericStatics<string>.Call((delegate*<string, string>)(void*)(delegate*<int, int>)&Twice, "x");
        }

        private static int NestedSevenLevels(delegate*<delegate*<delegate*<delegate*<delegate*<delegate*<delegate*<int>>>>>>> function) => 7;
        private static int ApplyToNested<T>(delegate*<delegate*<T, int>, int> function) => function(null);
        private static int AcceptNested<T>(delegate*<T, int> function) => 5;

        private static void CallForgedNestedGenericContext()
        {
            // same nested signature blob, resolved with T = int on the call site and with T = string on the target
            ApplyToNested<int>((delegate*<delegate*<int, int>, int>)(void*)(delegate*<delegate*<string, int>, int>)&AcceptNested<string>);
        }

        private static void CallForgedOpenInstantiation()
        {
            // the target is Identity<!!0> taken in MakeIdentity<string>: T is string
            delegate*<int, string> forged = (delegate*<int, string>)(void*)MakeIdentity<string>();
            forged(0x41414141);
        }

        private static void CallForgedOpenNestedInstantiation()
        {
            ApplyBox<int>((delegate*<GenericBox<int>, GenericBox<int>>)(void*)MakeBoxIdentity<string>(), new GenericBox<int>(1));
        }

        private static void CallForgedGenericArgument()
        {
            ApplyBox<long>((delegate*<GenericBox<long>, GenericBox<long>>)(void*)(delegate*<GenericBox<int>, GenericBox<int>>)&IntBoxIdentity, new GenericBox<long>(1));
        }

        private static void CallForgedDeeplyNestedSignature()
        {
            delegate*<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<int>>>>>>>>>>, int, void> forged =
                (delegate*<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<int>>>>>>>>>>, int, void>)(void*)
                (delegate*<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<int>>>>>>>>>>, string, void>)&NestedBoxesString;
            forged(null, 0x41414141);
        }

        private static void CallForgedNestedSignature()
        {
            ApplyNested<int>(
                (delegate*<delegate*<int, int>, int, int>)(void*)(delegate*<delegate*<long, long>, int, int>)&IgnoreLongPointer,
                &Twice,
                1);
        }

        private static T[] OpenGenericType<T>(T value)
        {
            delegate*<T, int, T[]> fill = &GenericStatics<T>.Fill;
            return fill(value, 2);
        }

        [TestMethod]
        public void GenericFunctionPointer_GenericMethod()
        {
            delegate*<int, int> identityInt = &Identity<int>;
            delegate*<string, string> identityString = &Identity<string>;

            Assert.AreEqual(5, identityInt(5));
            Assert.AreEqual("x", identityString("x"));
        }

        [TestMethod]
        public void GenericFunctionPointer_MethodTypeArgumentIsResolved()
        {
            delegate*<long, object> box = &BoxValue<long>;
            delegate*<Point, Point[]> wrap = &Wrap<Point>;
            delegate*<object, bool> isInt = &IsOfType<int>;
            delegate*<int> defaultInt = &DefaultOf<int>;
            delegate*<string> defaultString = &DefaultOf<string>;

            object boxed = box(1L << 40);
            Point[] wrapped = wrap(new Point(3, 4));

            Assert.IsTrue(boxed is long);
            Assert.AreEqual(1L << 40, (long)boxed);
            Assert.AreEqual(1, wrapped.Length);
            Assert.AreEqual(4, wrapped[0].Y);
            Assert.IsTrue(isInt(1));
            Assert.IsFalse(isInt("s"));
            Assert.AreEqual(0, defaultInt());
            Assert.IsNull(defaultString());
        }

        [TestMethod]
        public void GenericFunctionPointer_MultipleTypeArgumentsAndGenericReturn()
        {
            delegate*<int, string, string> pair = &Pair<int, string>;
            delegate*<double, GenericBox<double>> makeBox = &MakeBox<double>;

            Assert.AreEqual("1,a", pair(1, "a"));
            Assert.AreEqual(2.5, makeBox(2.5).Value);
        }

        [TestMethod]
        public void GenericFunctionPointer_ConstrainedMethod()
        {
            delegate*<Square, int> area = &AreaOf<Square>;

            Assert.AreEqual(49, area(new Square { Side = 7 }));
        }

        [TestMethod]
        public void GenericFunctionPointer_GenericTypeStaticMethod()
        {
            delegate*<int, int> storeInt = &GenericStatics<int>.Store;
            delegate*<string, string> storeString = &GenericStatics<string>.Store;
            delegate*<byte, int, byte[]> fill = &GenericStatics<byte>.Fill;
            delegate*<short, object> box = &GenericStatics<short>.BoxValue;

            Assert.AreEqual(8, storeInt(8));
            storeString("str");
            byte[] filled = fill(7, 3);
            object boxed = box(-3);

            // static fields are per instantiation
            Assert.AreEqual(8, GenericStatics<int>.Last);
            Assert.AreEqual("str", GenericStatics<string>.Last);
            Assert.AreEqual(3, filled.Length);
            Assert.AreEqual((byte)7, filled[2]);
            Assert.IsTrue(boxed is short);
            Assert.AreEqual((short)-3, (short)boxed);
        }

        [TestMethod]
        public void GenericFunctionPointer_GenericTypeStaticConstructorRuns()
        {
            delegate*<int> touch = &GenericCctor<long>.Touch;

            Assert.AreEqual(1, touch());
            Assert.AreEqual(1, touch());
        }

        [TestMethod]
        public void GenericFunctionPointer_GenericStructStaticMethod()
        {
            delegate*<long, GenericStruct<long>> make = &GenericStruct<long>.Make;

            Assert.AreEqual(9L, make(9).Item);
        }

        [TestMethod]
        public void GenericFunctionPointer_TypedWithTypeParameter()
        {
            Assert.AreEqual(12, GenericStatics<int>.ViaField(12));
            Assert.AreEqual(13, GenericStatics<int>.ViaLocal(13));
        }

        [TestMethod]
        public void GenericFunctionPointer_OpenGenericContext()
        {
            int[] filled = OpenGenericType(4);

            Assert.AreEqual(21, OpenIdentity(21));
            Assert.AreEqual(2, filled.Length);
            Assert.AreEqual(4, filled[1]);
        }

        [TestMethod]
        public void GenericFunctionPointer_CallSiteUsingTypeParameters()
        {
            int value = 1;

            ApplyRef<int>(&Increment, ref value);

            Assert.AreEqual(8, Apply<int>(&Twice, 4));
            Assert.AreEqual("a!", Apply<string>(&Exclaim, "a"));
            Assert.AreEqual(3, Apply<Point>(&SwapPoint, new Point(4, 3)).X);
            Assert.AreEqual(7, Apply<long>(&Identity<long>, 7L));
            Assert.AreEqual(2, value);
            Assert.AreEqual(3, ApplyArray<int>(&Length, new int[3]));
            Assert.AreEqual(6, ApplyNested<int>(&CallInt, &Twice, 3));
            Assert.AreEqual("q", ApplyNested<string>(&Apply<string>, &Identity<string>, "q"));
            Assert.AreEqual(10, GenericStatics<int>.Call(&Twice, 5));
            Assert.AreEqual("b!", GenericStatics<string>.Call(&Exclaim, "b"));
        }

        [TestMethod]
        public void GenericFunctionPointer_CallSiteUsingGenericTypesOfTypeParameters()
        {
            delegate*<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<GenericBox<int>>>>>>>>>>, int, int> nested = &NestedBoxes;

            Assert.AreEqual(4, ApplyBox<int>(&IntBoxIdentity, new GenericBox<int>(4)).Value);
            Assert.AreEqual("s", MakeIdentity<string>()("s"));
            Assert.AreEqual("n", MakeBoxIdentity<string>()(new GenericBox<string>("n")).Value);
            Assert.AreEqual("m", ApplyBox<string>(MakeBoxIdentity<string>(), new GenericBox<string>("m")).Value);
            Assert.AreEqual(3, nested(null, 3));
        }

        [TestMethod]
        public void GenericFunctionPointer_NestedFunctionPointerSignatures()
        {
            delegate*<delegate*<delegate*<delegate*<delegate*<delegate*<delegate*<delegate*<int>>>>>>>, int> sevenLevels = &NestedSevenLevels;

            Assert.AreEqual(7, sevenLevels(null));
            Assert.AreEqual(5, ApplyToNested<string>(&AcceptNested<string>));
            Assert.IsTrue(Throws(&CallForgedNestedGenericContext));
        }

        [TestMethod]
        public void GenericFunctionPointer_ForgedSignatureIsRejected()
        {
            // run the valid instantiations first so that the validation of the call sites is cached
            Assert.AreEqual(8, Apply<int>(&Twice, 4));
            Assert.AreEqual(10, GenericStatics<int>.Call(&Twice, 5));

            Assert.IsTrue(Throws(&CallForgedGenericInstantiation));
            Assert.IsTrue(Throws(&CallForgedThroughGenericMethod));
            Assert.IsTrue(Throws(&CallForgedThroughGenericType));
            Assert.IsTrue(Throws(&CallForgedNestedSignature));
            Assert.IsTrue(Throws(&CallForgedOpenInstantiation));
            Assert.IsTrue(Throws(&CallForgedOpenNestedInstantiation));
            Assert.IsTrue(Throws(&CallForgedGenericArgument));
            Assert.IsTrue(Throws(&CallForgedDeeplyNestedSignature));
        }

        [TestMethod]
        public void GenericFunctionPointer_ReferenceTypeVariance()
        {
            delegate*<GenericBox<int>, string> toObject = &Describe;
            Assert.AreEqual("object", toObject(new GenericBox<int>(1)));

            delegate*<VarianceBox<int>, int> toInterface = &Measure;
            Assert.AreEqual(11, toInterface(new VarianceBox<int>()));

            Assert.AreEqual("s:z", ClassToObject<string>("z"));
            Assert.IsTrue(GenericReturnToObject<int>() is VarianceBox<int>);

            // generic variance isn't a reference conversion of generic classes: forged pointers are rejected
            delegate*<VarianceBox<string>, int> forged = (delegate*<VarianceBox<string>, int>)(delegate*<VarianceBox<object>, int>)&MeasureObjects;
            bool rejected = false;

            try
            {
                forged(new VarianceBox<string>());
            }
            catch (Exception)
            {
                rejected = true;
            }

            Assert.IsTrue(rejected);
        }

        private static string Describe(object value) => value == null ? "null" : "object";

        private static int Measure(IVarianceThing thing) => thing.Size;

        private static int MeasureObjects(VarianceBox<object> box) => box.Size;

        private static string Prefix(object value) => "s:" + value.ToString();

        private static string ClassToObject<T>(T value)
            where T : class
        {
            delegate*<T, object> prefix = &Prefix;

            return (string)prefix(value);
        }

        private static object GenericReturnToObject<T>()
        {
            delegate*<object> make = &MakeVarianceBox<T>;

            return make();
        }

        private static VarianceBox<T> MakeVarianceBox<T>() => new VarianceBox<T>();

        [TestMethod]
        public void GenericFunctionPointer_Comparisons()
        {
            delegate*<int, int> first = &Identity<int>;
            delegate*<int, int> second = &Identity<int>;
            delegate*<long, long> other = &Identity<long>;

            Assert.IsTrue(first == second);
            Assert.IsTrue((void*)first != (void*)other);
        }
    }

    public class GenericBox<T>
    {
        public T Value;

        public GenericBox(T value)
        {
            Value = value;
        }
    }

    public static unsafe class GenericStatics<T>
    {
        public static T Last;

        private static delegate*<T, T> s_store = &Store;

        public static T Store(T value)
        {
            Last = value;
            return value;
        }

        public static T[] Fill(T value, int count)
        {
            T[] values = new T[count];

            for (int i = 0; i < count; i++)
            {
                values[i] = value;
            }

            return values;
        }

        public static object BoxValue(T value) => value;

        public static T ViaField(T value) => s_store(value);

        public static T Call(delegate*<T, T> function, T value) => function(value);

        public static T ViaLocal(T value)
        {
            delegate*<T, object> box = &BoxValue;
            return (T)box(value);
        }
    }

    public static class GenericCctor<T>
    {
        private static int s_runs;

        static GenericCctor()
        {
            s_runs++;
        }

        public static int Touch() => s_runs;
    }

    public struct GenericStruct<T>
    {
        public T Item;

        public static GenericStruct<T> Make(T value) => new GenericStruct<T> { Item = value };
    }

    public interface IShape
    {
        int Area();
    }

    public struct Square : IShape
    {
        public int Side;

        public int Area() => Side * Side;
    }

    public interface IVarianceThing
    {
        int Size { get; }
    }

    public class VarianceBox<T> : IVarianceThing
    {
        public T Value;

        public int Size => 11;
    }
}
