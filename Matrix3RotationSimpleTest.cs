using BlazorThreeJS.Maths;
using FoundryRulesAndUnits.Extensions;

namespace Three2025
{
    public class Matrix3RotationSimpleTest
    {
        public static void Run()
        {
            TestRotateX90();
            TestRotateY90();
            TestRotateZ90();
        }

        private static void TestRotateX90()
        {
            var t = new Transform3("TestX");
            t.Rotation = Euler.FromDegrees(90, 0, 0); // degrees
            var m = t.ToMatrix3();

            // Expected matrix for 90° rotation about X axis (right-handed system):
            double[] expected = new double[]
            {
                1, 0, 0, 0,
                0, 0, 1, 0,
                0, -1, 0, 0,
                0, 0, 0, 1
            };

            CheckMatrix("Transform3.RotateX(90)", m.Elements, expected);
        }

        private static void TestRotateY90()
        {
            var t = new Transform3("TestY");
            t.Rotation = Euler.FromDegrees(0, 90, 0); // degrees
            var m = t.ToMatrix3();

            // Expected matrix for 90° rotation about Y axis (right-handed system):
            double[] expected = new double[]
            {
                0, 0, -1, 0,
                0, 1, 0, 0,
                1, 0, 0, 0,
                0, 0, 0, 1
            };

            CheckMatrix("Transform3.RotateY(90)", m.Elements, expected);
        }

        private static void TestRotateZ90()
        {
            var t = new Transform3("TestZ");
            t.Rotation = Euler.FromDegrees(0, 0, 90); // degrees
            var m = t.ToMatrix3();

            // Expected matrix for 90° rotation about Z axis (right-handed system):
            double[] expected = new double[]
            {
                0, 1, 0, 0,
                -1, 0, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };

            CheckMatrix("Transform3.RotateZ(90)", m.Elements, expected);
        }

        private static void CheckMatrix(string testName, double[] actual, double[] expected)
        {
            bool success = true;
            for (int i = 0; i < expected.Length; i++)
            {
                if (Math.Abs(actual[i] - expected[i]) > 1e-6)
                {
                    $"{testName}: Matrix element {i} mismatch (actual={actual[i]:F2}, expected={expected[i]:F2})".WriteError();
                    success = false;
                }
            }
            if (success)
            {
                $"{testName}: Success".WriteSuccess();
            }
            else
            {
                $"{testName}: Failed".WriteError();
            }
        }
    }
}
