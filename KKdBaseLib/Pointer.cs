﻿namespace KKdBaseLib
{
    public struct Pointer<T>
    {
        public int O;
        public T V;

        public override string ToString() => Extensions.ToS(V);

        public Pointer(T value)
        { O = 0; V = value; }

        public Pointer(int offset, T value)
        { O = offset; V = value; }
    }
    
    public struct CountPointer<T>
    {
        public int C { get => E != null ? E.Length : 0;
                       set => E = value > -1 ? new T [value] : null; }
        public int O;
        public T[] E;

        public T this[int index]
        {   get =>    E != null && index > -1 && index < E.LongLength ? E[index] : default;
            set { if (E != null && index > -1 && index < E.LongLength)  E[index] =   value; } }

        public override string ToString() => C < 1 ? "No Entries" :
            C == 1 ? E[0].ToString() : "Count: " + C;
    }
}