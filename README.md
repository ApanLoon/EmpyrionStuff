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
* General
    * Create structures such as: Box, box frame, pyramid, sphere, and some for debugging
* Summary
    * EPB file version
    * Blueprint type (Voxel, Base, Hover vessel, Small vessel or Capital vessel)
    * Dimensions
    * Counters
    * Block type counts
    * Meta tags
    * Device groups
* 3D view of blocks
* Tree view of blocks
    * Categories
    * Displays content of LCD screens
    * Displays block tags
    * Identifies selected block in the 3D view
* Graph of logic nodes
    * Drag nodes around
* Lists of signal sources, signal targets and circuits

## EPBLib
A library for parsed EmPyrion Blueprints.
* Read EPB files up to version 23
* Write EPB files as version 20
* Get/set blueprint type ("Voxel", "Base", "Small vessel", "Capital vessel" or "Hover vessel")
* Get/set blueprint dimensions
* Compute dimensions from the current blocks
* Add/modify/remove meta tags (Such as "Creator name", "Group name", "Spawn name", "Ground offset", etc)
* Get/set various counters
* Manually access block type counts (Add/modify/remove)
* Count blocks, attempts to group types that should be grouped but this is not 100% correct yet
* Device groups
    * Add/modify/remove device groups and device group entries
    * Get/set device group names
    * Get/set shortcut connected to the control panel
    * Get/set the device entry block reference
* Blocks
    * Access block by position or iterate over all blocks
    * Add/modify/remove blocks (Adding can be done with single or a list)
    * Get/set block type and block variant (child shape)
    * Lookups for block type id and names as found in Config_Example.ecf (And some other sources)
    * Lookups for block variants as found in BlockShapesWindow.ecf (And some other sources)
    * Incomplete groupings for which block types are allowed in which blueprint types
    * Incomplete groupings for counting block types
    * Get/set block position
    * Get/set block rotation
    * Get/set damage state
    * Get/set colour indices for each side of the block
    * Get/set texture indices for all sides of the block
    * Get/set texture flip state for all sides of the block
    * Get/set symbol indices for all sides of the block
    * Get/set symbol rotation for all sides of the block
    * Add/modify/remove block tags
* Logic
    * Add/modify/remove signal sources, signal targets and signal operators
    * Add/modify/remove custom control panel switch names
* Modify custom colour palette entries

## ecf
A command line tool that parses ECF configuration files. Currently the only real use for this is to extract block types for use in EPBLib.

## ECFLib
A library for parsed Empyrion Configuration Files.
