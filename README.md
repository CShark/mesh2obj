# mesh2obj
Converts BA2-.mesh files into .obj files, that can be importet into any 3D Software, e.g. Blender.

How to use:
- Use the BAE-tool to extract the .nif you are interested in
- Open nif in Notepad, look for garbled letters and numbers with a slash at the end of the file (e.g. `0a5df91b99bd0c18a55a\365c69451480e92bbf4f`)
- (Probably) in the same .ba2 file, extract the geometry file with that number
- Use `mesh2obj -i <filename>` to generate an obj file in the same location or use `mesh2obj -i <filename> -o <filename>` for custom location
- To use textures, find the fitting texture in one of the `Starfield - TexturesXX.ba2` files, probably under the same folder as the initial mesh and apply it after importing the model into your software