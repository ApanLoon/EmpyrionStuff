#!/bin/sh

## Convert OBJ file as exported from Realsof3D to C# code suitable for
## EPBLab/ViewModel/BlocksViewModel.cs. Object hierarchy must conform to
##
##  xx/Block/nn Variant/face
##
## where xx can be anything, nn is the two-digit variant index and face is
## the name of the SDS object, one for each "side".
##
## Settings for Realsoft3D OBJ exporter:
##
## Format: OBJ
## Collect External Files:       No
## Save all frames:              No
## Scale:                        3.280839895013123
## Resolution:                   10
## Export objects as:            group names
## Export selected objects only: No
## Supress invisible objects:    No
## Export object hierarchy:      Yes
## Unique object names:          Yes
## SDS objects as triangles:     Yes
## Export vertex normals:        Yes
## Export material definitions:  No
## Export uv coords:             Yes
## UV coords as:                 Pointwise uv coords

gawk '\
BEGIN \
{
    block="";
    variant="";
    variantIndex=0;
    face="";
    totalVCount=0;
    faceVCount=0;
    printf ("        #region MeshGenerators\n");
}
/^g/\
{
    newBlock = $4;
    split($5, a, "_");
    newVariantIndex = a[1] + 0; ## Add zero to force numeric variable ("00" => 0)
    newVariant = a[2];
    newFace = convertFace($6);

    if (newBlock != block)
    {
        block = newBlock;
        printf ("\n        //%s\n", strip(block));
    }

    if (newVariant != variant)
    {
        if (variant != "")
        {
            closeVariant();
        }
        variant = newVariant;
        variantIndex = newVariantIndex;
        face = "";               # Prevent close face
        methodName = sprintf("AddMesh_%s", strip(variant));
        meshGenerators[block][variantIndex] =  methodName;
        printf ("        private static int %s(EpBlueprint blueprint, MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)\n        {\n", methodName);
    }

    if (newFace != face)
    {
        if (face != "")
        {
            closeFace();
        }
        face = newFace;
        faceVCount = 0;
        printf ("            // %s\n", strip(face));
    }
}
/^v /\
{
    totalVCount = totalVCount + 1;
    faceVCount = faceVCount + 1;
    printf ("            mesh.Positions.Add(new Point3D(%f, %f, %f));\n", $2, $3, $4);
}
/^vt /\
{
    printf ("            mesh.TextureCoordinates.Add(GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.%s], %f, %f));\n", face, $2, $3);
}
/^vn /\
{
    printf ("            mesh.Normals.Add(new Vector3D(%f, %f, %f));\n", $2, $3, $4);
}
/^f /\
{
    i = vertexIndex($2) - (totalVCount - faceVCount + 1);
    j = vertexIndex($3) - (totalVCount - faceVCount + 1);
    k = vertexIndex($4) - (totalVCount - faceVCount + 1);
    printf ("            AddTriangle(mesh.TriangleIndices, faceIndex + %d, faceIndex + %d, faceIndex + %d);\n", i, j, k);
}
END \
{
    if (variant != "")
    {
        closeVariant();
    }
    printf ("        #endregion MeshGenerators\n");
    printf ("        #region MeshGeneratorLookups\n");
    for (key in meshGenerators)
    {
        variableName = sprintf ("%s_MeshGenerators", key);
        printf ("        private MeshGenerator[] %s = {", variableName);
        for (i = 0; i < 32; i++)
        {
            generator = meshGenerators[key][i];
            if (generator == "")
            {
                generator = "null";
            }
            printf ("%s, ", generator);
        }
        printf ("};\n");
    }
    printf ("        #endregion MeshGeneratorLookups\n");

}
function closeFace()
{
    printf ("            faceIndex += %d;\n", faceVCount);
}
function closeVariant()
{
    printf ("            return faceIndex;\n        }\n");
}
function vertexIndex(s)
{
    ## "vertex/texture/normal"
    split(s, a, "/");
    return a[1];
}
function convertFace(f) ## Needed because of lefthanded/right handed weirdness
{
    gsub(/\s/, "", f);
    if (f == "Front")
    {
        return "Back";
    }
    else if (f == "Back")
    {
        return "Front";
    }
    else if (f == "Left")
    {
        return "Right";
    }
    else if (f == "Right")
    {
        return "Left";
    }
    else
    {
        return f;
    }
}
function strip(s)
{
    gsub(/\s/, "", s);
    gsub(/_/, "", s);
    return s;
}
' "$1"

