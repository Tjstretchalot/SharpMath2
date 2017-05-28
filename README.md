# SharpMath2

This is a C# math library. It is built as the bare minimum to get up and running with a 2D game in C#. It
is compatible with or without monogame. To compile with monogame, use the compiler directive "NOT_MONOGAME".
This will provide versions of Microsoft.XNA.Framework.Vector2 and Microsoft.XNA.Framework.Point that do not
require any external libraries (with reduced functionality).

## Examples

### Import tags

```cs
using SharpMath2;
using Microsoft.XNA.Framework;
```

### Polygon construction

```cs
var triangle1 = new Polygon2(new[] { new Vector2(0, 0), new Vector2(1, 1), new Vector2(2, 0) });
var triangle2 = ShapeUtils.CreateCircle(1, segments=3); // this is not the same triangle as triangle1, this will be equilateral
var octogon = ShapeUtils.CreateCircle(1, segments=8);
```

### Polygon intersection

```cs
// Check intersection of two entities, both of which have the same triangle bounds but one is
// rotated at rotation1 and located at position1, where the other is rotated at rotation2 and 
// located at position2

var triangle = new Polygon2(new[] { new Vector2(0, 0), new Vector2(1, 1), new Vector2(2, 0) });

// Rotation2 caches Math.Sin and Math.Cos of the given angle, so if you know you are going to reuse
// rotations often (like 0) they should be cached (Rotation2.ZERO is provided)
var rotation1 = Rotation2.ZERO;
var rotation2 = Rotation2.ZERO; // new Rotation2((float)(Math.PI / 6)) would be 30degrees
var position1 = new Vector2(5, 3);
var position2 = new Vector2(6, 3);

// Determine if the polygons overlap or touch: 
Polygon2.Intersects(triangle, triangle, position1, position2, rotation1, rotation2, false); // True

// Determine if the polygons overlap
Polygon2.Intersects(triangle, triangle, position1, position2, rotation1, rotation2, true); // False

// Note that in the special case of no rotation (rotation1 == rotation2 == Rotation2.ZERO) we can
// use the shorter function definition by omitting the rotation parameters
Polygon2.Intersects(triangle, triangle, position1, position2, true); // False
```

### Polygon collision detection + handling

```cs
// Suppose we have two entities, entity1 and entity2, both of which use the polygon "triangle" and are at position1, rotation1 and 
// position2, rotation2 respectively. If we are updating entity1 and we want to detect and handle collision with entity2 we would
// do:

// note we do not check for intersection first - while intersection is faster to check than intersection + MTV, it is not 
// faster to check intersection then intersection + MTV if you will need the MTV.

// Note strict is not an option for MTV - if two triangles are touching but not overlapping then
// it doesn't make sense to try and get an MTV
Tuple<Vector2, float> mtv = Polygon2.IntersectMTV(triangle, triangle, position1, position2, rotation1, rotation2);
if(mtv != null)
{
  // The two entites are colliding.
  position1 += mtv.Item1 * mtv.Item2;
  
  // Polygon2.Intersects(triangle, triangle, position1, position2, rotation1, rotation2, true); -> False
  // Polygon2.Intersects(triangle, triangle, position1, position2, rotation1, rotation2, false); -> True
}
```

### Polygon -> AABB collision

It is very common to need to check polygons against unrotated rectangles in square-grid systems. In this case 
there are functions in Shape2 that provide these comparisons that is slightly faster than complete polygon to 
polygon collision that you would get from `ShapeUtils.CreateRectangle(width, height)` rather than `new Rect(minx, miny, maxx, maxy)`

```cs
var triangle = ShapeUtils.CreateCircle(1, segments=3);
var tile = new Rect2(0, 0, 1, 1); // minX, minY, maxX, maxY NOT x, y, w, h.

var triPos = new Vector2(3.3, 4.1);
var triRot = new Rotation2((float)(Math.PI / 6));

Vector2 tmp = Vector2.ZERO; // Vector2 is a struct so this is safe
int xMin = (int)triPos.x;
int xMax = (int)Math.Ceiling(triPos.x + triangle.LongestAxisLength);
int yMin = (int)triPos.y;
int yMax = (int)Math.Ceiling(triPos.y + triangle.LongestAxisLength);
for(int y = yMin; y <= yMax; y++)
{
  tmp.Y = y;
  for(int x = xMin; x <= xMax; x++) 
  {
     tmp.X = x;
     var intersectsTileAtXY = Shape2.Intersects(triangle, tile, triPos, tmp, triRot, true);
     Console.Write($"({x},{y})={intersectsTileAtXY}")
     if(intersectsTileAtXY)
       Console.Write("  "); // true is 1 letter shorter than false
     else
       Console.Write(" ");
  }
  Console.WriteLine();
}
```

### Polygon AABB checking

Note that this is only faster for fairly complicated polygons (theoretical breakeven at 6 unique normals each).
Further note that it's almost *never* faster for rotated polygons - finding the AABB for rotated polygons is not
supported (though not complicated).

The provided AABB is most often used in UI elements which do not anticipate rotation and can have somewhat complicated
polygons but don't have rotation, which is where AABBs shine.

```cs
var complicatedShape = ShapeUtils.CreateCircle(5); // radius 5, 32 segments

// Note we are not providing rotation - rect2 does not support rotation 
// (use ShapeUtils.CreateRectangle for that, which returns a Polygon2)
Rect2.Intersects(complicatedShape.AABB, complicatedShape.AABB, Vector2.ZERO, new Vector2(3, 0), true); // True
````

## Performance notes

This library is designed for when:

1. You have only a few different polygon types
2. You need to check collision on those polygon types when they are in rapidly changing positions and rotations.

For example in a 2D game where everything is either a triangle or hexagon, in this library you would only need 
to construct two polygons, then reuse those two polygons everywhere else. This allows the library to cache certain
operations.

The library is designed such that changing rotations or position is fast, but the downside is when rotation
or position does *not* change there is only a minor improvement in performance.