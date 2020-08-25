using CommandLine;
using gltfinfo;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using System;
using System.IO;
using System.Linq;

namespace src
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                Info(o);
            });
        }

        private static void Info(Options options)
        {
            Console.WriteLine($"Action: Info");
            Console.WriteLine("glb file: " + options.Input);

            try
            {

                var glb = ModelRoot.Load("." + Path.DirectorySeparatorChar + options.Input);

                Console.WriteLine("glTF model is loaded");
                Console.WriteLine("glTF generator: " + glb.Asset.Generator);
                Console.WriteLine("glTF version:" + glb.Asset.Version);
                Console.WriteLine("glTF primitives: " + glb.LogicalMeshes[0].Primitives.Count);
                var triangles = Schema2Toolkit.EvaluateTriangles(glb.DefaultScene).ToList();
                Console.WriteLine("glTF triangles: " + triangles.Count);
                var print_max_vertices = 3;
                Console.WriteLine($"glTF vertices (first {print_max_vertices}): ");

                var points = triangles.SelectMany(item => new[] { item.A.GetGeometry().GetPosition(), item.B.GetGeometry().GetPosition(), item.C.GetGeometry().GetPosition() }.Distinct().ToList());

                var i = 0;
                foreach (var p in points)
                {
                    if (i < print_max_vertices)
                    {
                        Console.WriteLine($"{p.X}, {p.Y}, {p.Z}");
                        i++;
                    }
                }


                var xmin = (from p in points select p.X).Min();
                var xmax = (from p in points select p.X).Max();
                var ymin = (from p in points select p.Y).Min();
                var ymax = (from p in points select p.Y).Max();
                var zmin = (from p in points select p.Z).Min();
                var zmax = (from p in points select p.Z).Max();

                Console.WriteLine($"Bounding box vertices: {xmin}, {xmax}, {ymin}, {ymax}, {zmin}, {zmax}");
                foreach (var primitive in glb.LogicalMeshes[0].Primitives)
                {
                    Console.Write($"Primitive {primitive.LogicalIndex} ({primitive.DrawPrimitiveType}) ");

                    if (primitive.GetVertexAccessor("_BATCHID") != null)
                    {
                        var batchIds = primitive.GetVertexAccessor("_BATCHID").AsScalarArray();
                        Console.WriteLine($"batch ids (unique): {string.Join(',', batchIds.Distinct())}");

                    }
                    else
                    {
                        Console.WriteLine($"No BATCH_ID attribute found...");
                    }
                }

                if (glb.ExtensionsUsed.Count() > 0)
                {
                    Console.WriteLine("glTF extensions used: " + string.Join(',', glb.ExtensionsUsed));
                }
                else
                {
                    Console.WriteLine("glTF: no extensions used.");
                }
                if (glb.ExtensionsRequired.Count() > 0)
                {
                    Console.WriteLine("glTF extensions required: " + string.Join(',', glb.ExtensionsRequired));
                }
                else
                {
                    Console.WriteLine("glTF: no extensions required.");
                }
            }
            catch (SchemaException ex)
            {
                Console.WriteLine("glTF schema exception");
                Console.WriteLine(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine("Invalid data exception");
                Console.WriteLine(ex.Message);
            }
            catch (LinkException ex)
            {
                Console.WriteLine("glTF Link exception");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
