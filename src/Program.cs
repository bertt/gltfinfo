using CommandLine;
using gltfinfo;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

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
                Console.WriteLine("glTF copyright:" + glb.Asset.Copyright);
                Console.WriteLine("glTF primitives: " + glb.LogicalMeshes[0].Primitives.Count);
                var transform = glb.DefaultScene.VisualChildren.First().LocalTransform;
                Console.WriteLine("glTF localTransform: " + transform.ToString() + ", identity: " + transform.IsIdentity);

                var triangles = Toolkit.EvaluateTriangles(glb.DefaultScene).ToList();
                // triangles.First().A.
                Console.WriteLine("glTF triangles: " + triangles.Count);
                var print_max_vertices = 3;
                Console.WriteLine($"glTF vertices (first {print_max_vertices}): ");

                var positions = triangles.SelectMany(item => new[] { item.A.GetGeometry().GetPosition(), item.B.GetGeometry().GetPosition(), item.C.GetGeometry().GetPosition() }.Distinct().ToList());

                var i = 0;
                foreach (var p in positions)
                {
                    if (i < print_max_vertices)
                    {
                        Console.WriteLine($"{p.X}, {p.Y}, {p.Z}");
                        i++;
                    }
                }
                if (triangles.First().A.GetGeometry().TryGetNormal(out Vector3 n))
                {
                    Console.WriteLine("Vertices do contains normals");
                }
                else
                {
                    Console.WriteLine("Vertices do not contains normals");
                }


                var xmin = (from p in positions select p.X).Min();
                var xmax = (from p in positions select p.X).Max();
                var ymin = (from p in positions select p.Y).Min();
                var ymax = (from p in positions select p.Y).Max();
                var zmin = (from p in positions select p.Z).Min();
                var zmax = (from p in positions select p.Z).Max();

                Console.WriteLine($"Bounding box vertices (xmin, xmax, ymin, ymax, zmin, zmax): {xmin}, {xmax}, {ymin}, {ymax}, {zmin}, {zmax}");


                foreach (var primitive in glb.LogicalMeshes[0].Primitives)
                {
                    Console.WriteLine($"Primitive {primitive.LogicalIndex} ({primitive.DrawPrimitiveType}) ");

                    Console.WriteLine($"Material doubleSided: {primitive.Material.DoubleSided}");
                    Console.WriteLine($"Material unlit: {primitive.Material.Unlit}");
                    Console.WriteLine($"Material alpha: {primitive.Material.Alpha}");
                    Console.WriteLine($"Material alphacutoff: {primitive.Material.AlphaCutoff}");
                    foreach (var channel in primitive.Material.Channels)
                    {
                        Console.WriteLine($"Material channel: {channel.Key}");
                    }

                    if (primitive.GetVertexAccessor("_BATCHID") != null)
                    {
                        var batchIds = primitive.GetVertexAccessor("_BATCHID").AsScalarArray();
                        Console.WriteLine($"batch ids (unique): {string.Join(',', batchIds.Distinct())}");

                    }
                    else if (primitive.GetVertexAccessor("_FEATURE_ID_0") != null)
                    {
                        var featureIds = primitive.GetVertexAccessor("_FEATURE_ID_0").AsScalarArray();
                        Console.WriteLine($"FEATURE_ID_0 (unique): {string.Join(',', featureIds.Distinct())}");
                    }
                    else
                    {
                        Console.WriteLine($"No _BATCHID or _FEATURE_ID_0 attribute found...");
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
