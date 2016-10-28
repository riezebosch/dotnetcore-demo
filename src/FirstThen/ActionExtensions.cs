using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirstThen
{
    public static class ActionExtensions
    {
        public static Func<T, T> ToFunc<T>(this Action action)
        {
            return new Func<T, T>(input => { action(); return input; });
        }

        public static Func<T, T> ToFunc<T>(this Action<T> action)
        {
            return new Func<T, T>(input => { action(input); return input; });
        }
    }
}
