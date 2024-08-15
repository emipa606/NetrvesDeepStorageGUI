using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DSGUI;

public static class DSGUI_Functions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OptimizedNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            return true;
        }

        if (enumerable is not ICollection collection)
        {
            return !enumerable.Any();
        }

        return collection.Count == 0;
    }
}