using CommandLine;
using gltfinfo;
using SharpGLTF.Geometry.VertexTypes;
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

                var readSettings = new ReadSettings();
                readSettings.Validation = ValidationMode.Skip;
                var glb = ModelRoot.Load(options.Input, readSettings);

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
                var print_max_vertices = 100;
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
                    Console.WriteLine($"glTF normals (first): {n} ");
                }
                else
                {
                    Console.WriteLine("Vertices do not contains normals");
                }

                var material = triangles.First().A.GetMaterial();

                if(material is VertexTexture1)
                {
                    Console.WriteLine($"Texcoord first material {material.GetTexCoord(0)}");
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

                    if (glb.ExtensionsUsed.Contains("CESIUM_primitive_outline"))
                    {
                        Console.WriteLine("CESIUM_primitive_outline is used");
                        //var cesiumOutlineExtension = (CesiumPrimitiveOutline)glb.LogicalMeshes[0].Primitives[0].Extensions.FirstOrDefault();
                        // Assert.NotNull(cesiumOutlineExtension.Indices);
                        // CollectionAssert.AreEqual(outlines, cesiumOutlineExtension.Indices.AsIndicesArray());

                        // var extension = glb.GetExtension<CesiumPrimitiveOutline>();
                    }
                    if (glb.ExtensionsUsed.Contains("EXT_mesh_features"))
                    {
                        Console.WriteLine("EXT_mesh_features is used");
                    }
                    if (glb.ExtensionsUsed.Contains("EXT_structural_metadata"))
                    {
                        Console.WriteLine("EXT_structural_metadata is used");
                    }
                    if(glb.ExtensionsUsed.Contains("EXT_mesh_gpu_instancing"))
                    {
                        Console.WriteLine("EXT_mesh_gpu_instancing");
                    }
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
