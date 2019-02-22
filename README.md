# EmpyrionStuff

This is a collection of tools that can be used to work with some of the file formats used in Empyrion Galacitc Survival.

## epb
A command line tool that parses EPB blueprint files. It can also create rudimentarty blueprints.
* Dump textual content of an EPB file
* Create EPB files
    * Set blueprint type
    * Set Width, Height and Depth
    * Set block to use (For Box and BoxFrame)
    * Set block variant to use (For Box and BoxFrame)
    * Set CreatorId
    * Set CreatorName
    * Set OwnerId
    * Set OwnerName
    * BaseBox: Generate a box of a given size, solid or hollow
    * BoxFrame: Generate the "wireframe" of a box of a given size
    * BasePyramid: Generate a pyramid with sloped sides of a given size, solid or hollow 
    * BaseBlockTypes: Generate a series of blocks of increasing type ids. Most likely generates an EPB that can't be spawned. Debug only.

## EPBLab
A Windows application with a graphical user interface for examining EPB files.
* Summary
    * EPB file version
    * Blueprint type (Voxel, Base, Hover vessel, Small vessel or Capital vessel)
    * Dimensions
    * Meta tags
* 3D view of blocks
* Tree view of blocks
    * Categories
    * Displays content of LCD screens
    * Identifies selected block in the 3D view
* Graph of logic nodes
    * Drag nodes around
* Lists of signal sources, signal targets and circuits

## EPBLib
A library for parsed EmPyrion Blueprints.

## ecf
A command line tool that parses ECF configuration files. Currently the only real use for this is to extract block types for use in EPBLib.

## ECFLib
A library for parsed Empyrion Configuration Files.
