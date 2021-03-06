# gridventory
Functions for making a simple grid inventory in Unity

Provides both static functions with which you can build your own inventory, and an easy to use class that wraps those functions and keeps track of added items

![Gridventory](~Documentation/gridventory.gif)

This repo contains a sample with a test scene, but Gridventory.cs is just the core script. See [TestGridventory.cs](Assets/Sample/TestGridventory.cs) for how to use it.

The static functions provide you with ability to:
* Raycast into the inventory and get a point within it
* Get a tile from the point within the inventory
* Find correct root tile for any size of the rotated item rect
* Get a world position from any inventory's tile or rect

The Gridventory class instance provides you with the ability to:
* Insert or remove items
* Find items by tile
* Check if a tile or rect is occupied

Inventory pivot is in lower left corner:

![Gridventory](~Documentation/pivot_orientation.png)

Limitations:
* Gridventory is planar (as in, not curved)
* Gridventory's tiles have equal width and height separation distance
* Gridventory doesn't support non-rectangular items
