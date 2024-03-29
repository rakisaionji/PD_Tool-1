﻿using System.Collections;
using System.Collections.Generic;

namespace KKdBaseLib
{
    public struct KKdList<T> : System.IDisposable, IEnumerator, IEnumerable, INull
    {
        public static KKdList<T> Null => new KKdList<T>();
        public static KKdList<T> New  => new KKdList<T>() { Count = 0, Capacity = 0 };
        public static KKdList<T> NewReserve(int Capacity) => new KKdList<T>() { Count = 0, Capacity = Capacity };

        private int index;
        private T[] array;

        public int Count { get; private set; }

        public bool  IsNull => array == null;
        public bool NotNull => array != null;

        public int Capacity { get => array != null ? array.Length : -1;
            set { if (array != null) System.Array.Resize(ref array, value); else array = new T[value];
                if (Count >= value) Count = value; } }


        public KKdList(T[] Array)
        { index = 0; Count = Array.Length; array = Array; }

        public T Current => index < Count ? array[index] : default;

        object IEnumerator.Current => Current;

        public T this[ int index]
        {   get =>    array != null && index > -1 && index < array.Length ? array[index] : default;
            set { if (array != null && index > -1 && index < array.Length)  array[index] =   value; } }

        public T this[uint index]
        {   get =>    array != null && index < array.Length ? array[index] : default;
            set { if (array != null && index < array.Length)  array[index] =   value; } }

        public bool MoveNext()
        { if (index == Count - 1) { index = 0; return false; }
          else                    { index++  ; return  true; } }

        public IEnumerator GetEnumerator() => this;

        public void Dispose() { array = null; Count = 0; index = 0; }

        public void Reset() => index = 0;

        public void Add(T item)
        {
            if (IsNull) return;

            Count++;
            if (array.Length < Count)
                System.Array.Resize(ref array, Count);
            array[Count - 1] = item;
        }

        public void RemoveAt(int index)
        {
            if (IsNull) return;

            for (int i = index + 1; i < Count; i++)
                array[i - 1] = array[i];
            Count--;
        }

        public void RemoveRange(int IndexStart, int IndexEnd)
        {
            if (IsNull) return;
            if (IndexEnd - IndexStart < 1) return;

            if (IndexEnd - IndexStart != Count)
                for (int i = IndexStart; i < Count; i++)
                    array[i] = array[i + IndexEnd - IndexStart];
            Count -= IndexEnd - IndexStart;
        }

        public T[] ToArray() => array;

        public bool Contains(T val)
        {
            if (IsNull) return false;
            for (int i = 0; i < Count; i++)
                     if (array[i] == null && val == null) return true;
                else if (array[i] == null || val == null)    continue;
                else if (array[i]    .Equals(val)       ) return true;
            return false;
        }

        public int IndexOf(T val)
        {
            if (IsNull) return -1;
            for (int i = 0; i < Count; i++)
                     if (array[i] == null && val == null) return i;
                else if (array[i] == null || val == null) continue;
                else if (array[i]    .Equals(val)       ) return i;
            return -1;
        }

        public void Sort()
        { List<T> List = this; List.Sort(); array = List.ToArray(); Count = List.Count; }

        public static implicit operator KKdList<T>(   List<T> List) =>
            new KKdList<T> { array = List.ToArray(), Count = List.Count };

        public static implicit operator    List<T>(KKdList<T> List)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < List.Count; i++) list.Add(List[i]);
            return list;
        }
    }
}
