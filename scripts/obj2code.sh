#!/bin/sh

## Convert OBJ file as exported from Realsof3D to C# code suitable for
## EPBLab/ViewModel/BlocksViewModel.cs. Object hierarchy must conform to
##
##  xx/Block/Variant/face
##
## where xx can be anything and face is the name of the SDS object, one for
## each "side".
##
## Settings for Realsoft3D OBJ exporter:
##
## Format: OBJ
## Collect External Files:       No
## Save all frames:              No
## Scale:                        39.37007874015748
## Resolution:                   10
## Export objects as:            group names
## Export selected objects only: No
## Supress invisible objects:    No
## Export object hierarchy:      Yes
## Unique object names:          Yes
## SDS objects as triangles:     Yes
## Export vertex normals:        Yes
## Export material definitions:  No
## Export uv coords:             No

gawk '\
BEGIN \
{
    block="";
    variant="";
    face="";
    totalVCount=0;
    faceVCount=0;
    printf ("        #region AddGeometry\n");
}
/^g/\
{
    newBlock = $4;
    newVariant = $5;
    newFace = $6;

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
        face = "";               # Prevent close face
        printf ("        private int AddGeometry_%s(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)\n        {\n", strip(variant));
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
    printf ("            vertices.Add(new Point3D(%f, %f, %f));\n", $2, $3, $4);
}
/^vn /\
{
    printf ("            normals.Add(new Vector3D(%f, %f, %f));\n", $2, $3, $4);
}
/^f /\
{
    i = vertexIndex($2) - (totalVCount - faceVCount + 1);
    j = vertexIndex($3) - (totalVCount - faceVCount + 1);
    k = vertexIndex($4) - (totalVCount - faceVCount + 1);
    printf ("            AddTriangle(triangles, faceIndex + %d, faceIndex + %d, faceIndex + %d);\n", i, j, k);
}
END \
{
    if (variant != "")
    {
        closeVariant();
    }
    printf ("        #endregion AddGeometry\n");
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
    ## "vertex/normal/texture"
    split(s, a, "/");
    return a[1];
}
function strip(s)
{
    gsub(/_/, "", s);
    return s;
}
' "$1"

