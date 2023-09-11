# mesh2obj
Converts BA2-.mesh and .nif files into .obj files, that can be importet into any 3D Software, e.g. Blender.

How to use:
- Use the BAE-tool to extract the .nif or .mesh you are interested in
- Use `mesh2obj -i <filename>` to generate an obj file in the same location or use `mesh2obj -i <filename> -o <filename>` for custom location.
- When converting .nif-Files, add the `--starfield` parameter with the path to the data directory to let the tool automagically extract all necessary submeshes. They will then be combined into a single final mesh
- To use textures, find the fitting texture in one of the `Starfield - TexturesXX.ba2` files, probably under the same folder as the initial mesh and apply it after importing the model into your software