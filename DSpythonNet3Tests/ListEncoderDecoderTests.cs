using System.Collections;
using DSPythonNet3;
using DSPythonNet3.Encoders;
using NUnit.Framework;
using Python.Runtime;

namespace DSPythonNet3Tests
{
    public class ListEncoderDecoderTests
    {
        [Test]
        public void TryDecode_ConvertsNestedPythonListsToClrLists()
        {
            DSPythonNet3Evaluator.InitializePython();

            using (Py.GIL())
            using (var scope = Py.CreateScope())
            {
                scope.Exec("value = [[1, [2, 3]], ['a', ['b']]]");
                using var pyList = scope.Get("value");

                var decoder = new ListEncoderDecoder();
                var success = decoder.TryDecode(pyList, out IList result);

                Assert.That(success, Is.True);
                Assert.That(result, Is.InstanceOf<IList>());

                var first = result[0] as IList;
                Assert.That(first, Is.Not.Null);
                Assert.That(first[0], Is.EqualTo(1));

                var secondLevel = first?[1] as IList;
                Assert.That(secondLevel, Is.Not.Null);
                Assert.That(secondLevel?[0], Is.EqualTo(2));
                Assert.That(secondLevel?[1], Is.EqualTo(3));

                var second = result[1] as IList;
                Assert.That(second, Is.Not.Null);
                Assert.That(second?[0], Is.EqualTo("a"));

                var thirdLevel = second?[1] as IList;
                Assert.That(thirdLevel, Is.Not.Null);
                Assert.That(thirdLevel?[0], Is.EqualTo("b"));
            }
        }
    }
}
