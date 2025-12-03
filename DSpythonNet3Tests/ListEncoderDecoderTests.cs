using System.Collections;
using NUnit.Framework;
using DSPythonNet3;

namespace DSPythonNet3Tests
{
    public class ListEncoderDecoderTests
    {
        [Test]
        public void TryDecode_ConvertsNestedPythonListsToClrLists()
        {
            DSPythonNet3Evaluator.InitializePython();

            string code = @"
import clr
clr.AddReference('DSCoreNodes')
from DSCore import List

data = [[[1, 2], [3]], [[4, 5], [6]]]
OUT = data, List.Flatten(data, 1), List.Flatten(data, 2), List.Flatten(data, -1)
";
            var empty = new ArrayList();
            var expected = new ArrayList
            {
                new ArrayList
                {
                    new ArrayList
                    {
                        new ArrayList { 1, 2 },
                        new ArrayList { 3 }
                    },
                    new ArrayList
                    {
                        new ArrayList { 4, 5 },
                        new ArrayList { 6 }
                    }
                },
                new ArrayList
                {
                    new ArrayList { 1, 2 },
                    new ArrayList { 3 },
                    new ArrayList { 4, 5 },
                    new ArrayList { 6 }
                },
                new ArrayList { 1, 2, 3, 4, 5, 6 },
                new ArrayList { 1, 2, 3, 4, 5, 6 }
            };

            var result = DSPythonNet3Evaluator.EvaluatePythonScript(code, empty, empty);
            Assert.That(result, Is.InstanceOf<IEnumerable>());

            var normalizedResult = NormalizeResult(result);
            CollectionAssert.AreEqual(expected, normalizedResult as IEnumerable);
        }

        private static object NormalizeResult(object value)
        {
            if (value is string || value is not IEnumerable enumerable)
            {
                return value;
            }

            var list = new ArrayList();
            foreach (var item in enumerable)
            {
                list.Add(NormalizeResult(item));
            }

            return list;
        }
    }
}
