using Microsoft.Xna.Framework;

namespace Sin3D._OBB3D;

/// <summary>
/// An oriented bounding box class for collision detection.
/// </summary>
public class OrientedBoundingBox
{
    private Vector3[] vertices;
    /// <summary>
    /// The vertices of the box.
    /// </summary>
    public Vector3[] Vertices => vertices;

    /// <summary>
    /// Creates a new oriented bounding box from an axis-aligned bounding box.
    /// </summary>
    /// <param name="AABB">The axis-aligned bounding box that will be used to create the oriented bounding box.</param>
    public OrientedBoundingBox (BoundingBox AABB)
    {
        vertices = AABB.GetCorners();
    }

    /// <summary>
    /// Transforms all the points of the oriented bounding box by the given transformation matrix.
    /// </summary>
    /// <param name="transform">The transformation matrix.</param>
    public void TransformPoints(Matrix transform)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Transform(vertices[i], transform);
        }
    }

    /// <summary>
    /// Returns whether or not 2 oriented bounding boxes intersect.
    /// </summary>
    /// <param name="box2">The other box.</param>
    /// <returns>Boolean: whether or not intersection is detected.</returns>
    public bool Intersects(OrientedBoundingBox box2)
    {
        Vector3[] box1LocalAxes = GetLocalAxes(vertices);
        Vector3[] box2LocalAxes = GetLocalAxes(box2.Vertices);
        Vector3[] crossAxes = GetCrossAxes(box1LocalAxes, box2LocalAxes);
        
        //merging the three axis arrays, normalizing the axes, and removing any axes with a length that tends to zero
        List<Vector3> normalizedAxes = MergeAndNormalizeAxisArrays(box1LocalAxes, box2LocalAxes, crossAxes);

        //running the collision checks using SAT (separating axis theorem)
        foreach (Vector3 axis in normalizedAxes)
        {
            //projecting boxes
            float[] projectedBoxA = GetProjectedPoints(vertices, axis);
            float projectedMinA = projectedBoxA.Min();
            float projectedMaxA = projectedBoxA.Max();

            float[] projectedBoxB = GetProjectedPoints(box2.Vertices, axis);
            float projectedMinB = projectedBoxB.Min();
            float projectedMaxB = projectedBoxB.Max();

            //checking if there is no overlap between the two boxes projected onto the axis
            if (
                (projectedMaxA < projectedMinB) ||
                (projectedMaxB < projectedMinA)
            )
            {
                //returning false if the ranges do not overlap (there is an axis on which the boxes do not intersect)
                return false;
            }
        }
        return true;
    }

    //method for getting the local axes from a series of vertices from an OBB
    Vector3[] GetLocalAxes(Vector3[] verts)
    {
        Vector3[] localAxes =
        [
            verts[1] - verts[0],
            verts[2] - verts[0],
            verts[4] - verts[0]
        ];
        return localAxes;
    }

    //method for getting the cross axes of the local box1 & box2 axes
    Vector3[] GetCrossAxes(Vector3[] box1LocalAxes, Vector3[] box2LocalAxes)
    {
        Vector3[] crossAxes = new Vector3[9];
        for (int i = 0; i < box1LocalAxes.Length; i++)
        {
            for (int j = 0; j < box2LocalAxes.Length; j++)
            {
                Vector3 crossAxis = Vector3.Cross(box1LocalAxes[i], box2LocalAxes[j]);
                crossAxes[(i * 3) + j] = crossAxis;
            }
        }
        return crossAxes;
    }

    //method for getting a list of normalized axes (excluding any that tend to zero)
    List<Vector3> MergeAndNormalizeAxisArrays(Vector3[] arr1, Vector3[] arr2, Vector3[] arr3)
    {
        List<Vector3> normalizedAxes = new ();

        //iterating over all axes
        Vector3[] axes = new Vector3[arr1.Length + arr2.Length + arr3.Length];
        arr1.CopyTo(axes, 0);
        arr2.CopyTo(axes, arr1.Length);
        arr3.CopyTo(axes, arr1.Length + arr2.Length);
        foreach (Vector3 axis in axes)
        {
            if (axis.LengthSquared() > 0.000001f) //not including any axes that tend toward zero
            {
                normalizedAxes.Add(Vector3.Normalize(axis));
            }
        }
        return normalizedAxes;
    }

    //method for getting the projected points of a given series of vertices projected onto a given axis
    float[] GetProjectedPoints(Vector3[] verts, Vector3 axis)
    {
        float[] projectedPoints = new float[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            float projectedPoint = Vector3.Dot(verts[i], axis);
            projectedPoints[i] = projectedPoint;
        }
        return projectedPoints;
    }
}
