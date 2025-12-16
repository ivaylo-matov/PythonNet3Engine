using System;
using System.Collections;
using System.Reflection;
using Dynamo.Utilities;
using Python.Runtime;

namespace DSPythonNet3.Encoders
{
    /// <summary>
    /// Prevents Python.NET from auto-encoding certain IEnumerable-based host objects
    /// (notably Dynamo for Revit "ConnectionNode") into Python iterables, which
    /// strips away callable .NET members like SubNodesOfSize / ExistingConnections.
    /// </summary>
    internal sealed class ConnectionNodeObjectEncoder : IPyObjectEncoder
    {
        private static readonly string[] methodNames = new[]
        {
            "SubNodesOfSize",
            "ExistingConnections"
        };

        public bool CanEncode(Type type)
        {
            // Don't intercept common primitives / containers.
            if (type == typeof(string))
            {
                return false;
            }

            if (!typeof(IEnumerable).IsAssignableFrom(type))
            {
                return false;
            }

            // Let our existing List encoder/decoder handle IList.
            if (typeof(IList).IsAssignableFrom(type))
            {
                return false;
            }

            // Don't interfere with dictionary encoding.
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return false;
            }

            // Target by type name (no hard reference to Revit/DynamoRevit assemblies).
            if (type.Name.Contains("ConnectionNode", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Target by known member names used in graphs.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (var name in methodNames)
            {
                if (type.GetMethod(name, flags) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public PyObject TryEncode(object value)
        {
            // Wrap as CLR object so methods remain callable in Python.
            return PyObject.FromManagedObject(value);
        }
    }
}

