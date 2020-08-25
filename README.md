# gltfinfo

Tool for getting glTF information like number of triangles, vertice positions, boundingbox, custom vertex attribute _BATCHID (used in 3D Tiles)

## Installation

Requirement: Install .NET Core SDK 3.1 https://dotnet.microsoft.com/download

- Install from NuGet

https://www.nuget.org/packages/gltfinfo/

```
$ dotnet tool install -g gltfinfo
```

or update:

```
$ dotnet tool update -g gltfinfo
```

## Running

```
$ gltfinfo -i tree.glb
glb file: tree.glb
glTF model is loaded
glTF generator: THREE.GLTFExporter
glTF version:2.0
glTF primitives: 1
glTF triangles: 4916
glTF vertices (first 3):
-0.2173369, 0.5012188, 0.20359802
-0.20620537, 0.5298042, 0.21289062
-0.18243027, 0.5423565, 0.21696472
Bounding box vertices: -2.1264381, 1.9909191, -3, 3.3465657, -2.1014228, 2.4548907
Primitive 0 (TRIANGLES) No _BATCHID attribute found...
glTF: no extensions used.
glTF: no extensions required.
```

Sample when glb contains batch information (3 primitives in this case):

```
Primitive 0 (TRIANGLES) batch ids (unique): 0,1,2,3,4,5,6,7,8,9,10,11,12,13
Primitive 1 (TRIANGLES) batch ids (unique): 0,1,2,3,4,5,6,8,9,11
Primitive 2 (TRIANGLES) batch ids (unique): 0,1,2,3,4,7,9,10,11,12,13
```
