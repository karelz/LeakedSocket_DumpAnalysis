using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        DataTarget dataTarget = DataTarget.LoadCrashDump(@"MyDump.dmp");

        ClrInfo runtimeInfo = dataTarget.ClrVersions.Single();  // just using the first runtime
        string dacFile = @"mscordacwks_amd64_amd64_4.7.2117.00.dll";
        ClrRuntime runtime = runtimeInfo.CreateRuntime(dacFile);

        if (!runtime.Heap.CanWalkHeap)
        {
            Console.WriteLine("Cannot walk heap");
            return;
        }

        ClrType connectionGroupType = runtime.Heap.GetTypeByName("System.Net.ConnectionGroup");

        foreach (ClrObject connectionGroup in runtime.Heap.EnumerateObjects().Where(o => o.Type == connectionGroupType))
        {
            Console.WriteLine($"Found {connectionGroup}");

            //connectionGroup.PrintFields();
            
            int connectionLimit = connectionGroup.GetField<int>("m_ConnectionLimit");
            Console.WriteLine($"ConnectionGroup 0x{connectionGroup.Address:x16} - m_ConnectionLimit: {connectionLimit}");
            
            List<ClrObject> connections = connectionGroup.GetObjectField("m_ConnectionList").GetObjectField("_items")
                .EnumerateObjectArrayItems().Where(o => !o.IsNull).ToList();
            Console.WriteLine($"    Connections: {connections.Count}");
            foreach (ClrObject c in connections)
            {
                Console.WriteLine(c);
            }
            
            List<ClrObject> connectionsStuck = connections.Where(c => c.GetField<bool>("m_NonKeepAliveRequestPipelined")).ToList();
            Console.WriteLine($"        m_NonKeepAliveRequestPipelined: true = {connectionsStuck.Count} / false = {connections.Count - connectionsStuck.Count}");
            Console.WriteLine($"            m_WriteDone: {connectionsStuck.Where(c => c.GetField<bool>("m_WriteDone")).Count()}");
            Console.WriteLine($"            m_ReadDone: {connectionsStuck.Where(c => c.GetField<bool>("m_ReadDone")).Count()}");
            Console.WriteLine($"            m_Free: {connectionsStuck.Where(c => c.GetField<bool>("m_Free")).Count()}");
            int connections_KeepAlive = connections.Where(c => c.GetField<bool>("m_KeepAlive")).Count();
            Console.WriteLine($"        m_KeepAlive: true = {connections_KeepAlive} / false = {connections.Count - connections_KeepAlive}");

            int connectionIndex = -1;
            foreach (ClrObject connection in connections)
            {
                connectionIndex++;
                //Debug.Assert(connection.Type.Name == "System.Net.Connection");
                //connection.PrintFields();
                bool m_NonKeepAliveRequestPipelined = connection.GetField<bool>("m_NonKeepAliveRequestPipelined");
                List<ClrObject> writeList = connection.GetObjectField("m_WriteList").GetObjectField("_items").EnumerateObjectArrayItems().NonNull().ToList();
                List<ClrObject> waitList = connection.GetObjectField("m_WaitList").GetObjectField("_items").EnumerateObjectArrayItems().NonNull().ToList();

                /*
                if (writeList.Count != 0 || waitList.Count != 0)
                {
                    continue;
                }
                */
                /*
                if (writeList.Count + waitList.Count != 1)
                {
                    continue;
                }
                */
                /*
                if (writeList.Count == 0 || writeList[0].GetField<bool>("m_KeepAlive"))
                {
                    continue;
                }
                */

                Console.WriteLine($"    Connection[{connectionIndex++}] 0x{connection.Address:x16}");
                Console.WriteLine("        m_NonKeepAliveRequestPipelined = {0}", connection.GetField<bool>("m_NonKeepAliveRequestPipelined"));
                Console.WriteLine("        m_KeepAlive = {0}", connection.GetField<bool>("m_KeepAlive"));
                Console.WriteLine("        m_ReadDone = {0}", connection.GetField<bool>("m_ReadDone"));
                Console.WriteLine("        m_WriteDone = {0}", connection.GetField<bool>("m_WriteDone"));
                Console.WriteLine("        m_Free = {0}", connection.GetField<bool>("m_Free"));
                Console.WriteLine($"        m_WriteList ({writeList.Count})");
                int writeListIndex = 0;
                foreach (ClrObject httpWebRequest in writeList)
                {
                    Console.WriteLine($"            HWR[{writeListIndex++}] 0x{httpWebRequest.Address:x16}");
                    PrintHttpWebRequestFields(httpWebRequest, "                ");
                }
                Console.WriteLine($"        m_WaitList ({waitList.Count})");
            }
        }
    }

    static void PrintHttpWebRequestFields(ClrObject o, string prefix)
    {
        Console.WriteLine($"{prefix}m_KeepAlive = {o.GetField<bool>("m_KeepAlive")}");
        //Console.WriteLine($"{prefix}m_NtlmKeepAlive = {o.GetField<bool>("m_NtlmKeepAlive")}");
    }
}

