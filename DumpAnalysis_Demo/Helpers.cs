using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Helpers
{
    public static void PrintFields(this ClrObject obj)
    {
        Console.WriteLine($"Type {obj.Type} fields ({obj.Type.Fields.Count}):");
        foreach (ClrInstanceField field in obj.Type.Fields)
        {
            Console.Write($"    {field.Name} - {field.Type}");
            if (field.HasSimpleValue)
            {
                Console.WriteLine($" - {field.GetValue(obj.Address)}");
            }
            else
            {
                Console.WriteLine();
            }
        }
        if (obj.Type.StaticFields.Any())
        {
            Console.WriteLine($"  Static fields ({obj.Type.StaticFields.Count}):");
            foreach (ClrStaticField staticField in obj.Type.StaticFields)
            {
                Console.WriteLine($"    {staticField.Name} - {staticField.Type}");
            }
        }
        if (obj.Type.ThreadStaticFields.Any())
        {
            Console.WriteLine($"  ThreadStatic fields ({obj.Type.ThreadStaticFields.Count}):");
            foreach (ClrThreadStaticField thredStaticField in obj.Type.ThreadStaticFields)
            {
                Console.WriteLine($"    {thredStaticField.Name} - {thredStaticField.Type}");
            }
        }
    }




    public static IEnumerable<ClrObject> EnumerateObjectArrayItems(this ClrObject obj)
    {
        if (!obj.IsArray)
            throw new ArgumentException("Array expected");

        int arrayLength = obj.Type.GetArrayLength(obj.Address);
        if (!obj.Type.ComponentType.IsObjectReference)
            throw new ArgumentException("object[] expected");

        for (int i = 0; i < arrayLength; i++)
        {
            ulong itemAddress = (ulong)obj.Type.GetArrayElementValue(obj.Address, i);
            ClrType itemType = obj.Type.Heap.GetObjectType(itemAddress);
            yield return new ClrObject(itemAddress, itemType);
        }
    }
    public static IEnumerable<ClrObject> NonNull(this IEnumerable<ClrObject> objects)
    {
        return objects.Where(o => !o.IsNull);
    }

}
