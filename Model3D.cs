using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sin3D._OBB3D;

namespace Sin3D._Model3D;


/// <summary>
/// A model class that handles position, rotation, scale, texture and collision detection for a 3D model.
/// </summary>
public class Model3D
{
    Vector3 position;
    /// <summary>
    /// The (x, y, z) position of the model.
    /// </summary>
    public Vector3 Position { get => position; set {position = value;} }

    Quaternion rotation;
    /// <summary>
    /// The rotation of the model (stored as a quaternion).
    /// </summary>
    public Quaternion Rotation { get => rotation; set {rotation = value;} }

    float scale;
    /// <summary>
    /// The scale of the model.
    /// </summary>
    public float Scale { get => scale; set {scale = value;} }

    Model baseModel;
    /// <summary>
    /// The 3D model itself.
    /// </summary>
    public Model BaseModel => baseModel;

    bool textureEnabled;
    /// <summary>
    /// Boolean representing whether or not the model will be draw with a texture.
    /// </summary>
    public bool TextureEnabled { get => textureEnabled; set {textureEnabled = value; }}

    Texture2D? texture;
    /// <summary>
    /// The texture of the model (nullable).
    /// </summary>
    public Texture2D? Texture
    {
        get => texture;
        set
        {
            if (value is not null)
            {
                textureEnabled = true;
                texture = value;
            }
        }
    }
    
    Matrix worldMatrix;
    /// <summary>
    /// The world matrix of the model, representing scale, rotation and position.
    /// </summary>
    public Matrix WorldMatrix => worldMatrix;

    List<BoundingBox> localAxisAlignedBoundingBoxList = new ();
    /// <summary>
    /// The list of local axis-aligned boxes for the model.
    /// </summary>
    public List<BoundingBox> LocalAxisAlignedBoundingBoxList => localAxisAlignedBoundingBoxList;

    /// <summary>
    /// Creates a new Model3D object with position, rotation, scale and model settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position.</param>
    /// <param name="rotation">The initial quaternion rotation.</param>
    /// <param name="scale">The initial scale.</param>
    /// <param name="baseModel">The 3D model itself.</param>
    public Model3D(Vector3 position, Quaternion rotation, float scale, Model baseModel)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.baseModel = baseModel;

        UpdateWorldMatrix();
        CreateLocalAxisAlignedBoundingBoxList();
    }

    /// <summary>
    /// Creates a new Model3D object with position, rotation, scale, model and texture settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position.</param>
    /// <param name="rotation">The initial quaternion rotation.</param>
    /// <param name="scale">The initial scale.</param>
    /// <param name="baseModel">The 3D model itself.</param>
    /// <param name="texture">The texture that will be draw onto the 3D model.</param>
    public Model3D(Vector3 position, Quaternion rotation, float scale, Model baseModel, Texture2D texture)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.baseModel = baseModel;
        textureEnabled = true;
        this.texture = texture;

        UpdateWorldMatrix();
        CreateLocalAxisAlignedBoundingBoxList();
    }

    /// <summary>
    /// Updates the world matrix of the model (to be used after changing scale, rotation or position).
    /// </summary>
    public void UpdateWorldMatrix()
    {
        worldMatrix = (
            Matrix.CreateScale(scale) *
            Matrix.CreateFromQuaternion(rotation) *
            Matrix.CreateTranslation(position)
        );
    }

    //creates a list of axis-aligned bounding boxes from the models meshes
    void CreateLocalAxisAlignedBoundingBoxList()
    {
        localAxisAlignedBoundingBoxList = new();
        //finding the boxes of each mesh
        foreach (ModelMesh mesh in baseModel.Meshes)
        {
            localAxisAlignedBoundingBoxList.Add(CreateBoundingBox(mesh));
        }
    
    }

    //creates a bounding box from a mesh
    BoundingBox CreateBoundingBox(ModelMesh mesh)
    {
        Vector3 minVert = Vector3.Zero;
        Vector3 maxVert = Vector3.Zero;
        foreach (ModelMeshPart meshPart in mesh.MeshParts)
        {
            VertexPosition[] vertices = new VertexPosition[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData(vertices);
            foreach (VertexPosition vert in vertices)
            {
                Vector3 coord = vert.Position;
                //min vert
                if (coord.X < minVert.X)
                {
                    minVert.X = coord.X;
                }
                if (coord.Y < minVert.Y)
                {
                    minVert.Y = coord.Y;
                }
                if (coord.Z < minVert.Z)
                {
                    minVert.Z = coord.Z;
                }
                //max vert
                if (coord.X > maxVert.X)
                {
                    maxVert.X = coord.X;
                }
                if (coord.Y > maxVert.Y)
                {
                    maxVert.Y = coord.Y;
                }
                if (coord.Z > maxVert.Z)
                {
                    maxVert.Z = coord.Z;
                }
            }
        }
        return new BoundingBox(minVert, maxVert);
    }

    /// <summary>
    /// Returns whether or not the bounding spheres of 2 models intersect.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean: whether or not intersection is detected.</returns>
    public bool BoundingSphereIntersects(Model3D model2)
    {
        foreach (ModelMesh mesh1 in baseModel.Meshes)
        {
            foreach (ModelMesh mesh2 in model2.BaseModel.Meshes)
            {
                if (mesh1.BoundingSphere.Transform(worldMatrix).Intersects(mesh2.BoundingSphere.Transform(model2.WorldMatrix)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns whether or not the axis-aligned bounding boxes of 2 models intersect.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean: whether or not intersection is detected.</returns>
    public bool AxisAlignedBoundingBoxIntersects(Model3D model2)
    {
        //creating the axis-aligned bounding boxes and checking if they collide
        foreach (BoundingBox box1 in localAxisAlignedBoundingBoxList)
        {
            BoundingBox transformedBox1 = GetTransformedAxisAlignedBoundingBox(box1, worldMatrix);
            foreach (BoundingBox box2 in model2.LocalAxisAlignedBoundingBoxList)
            {
                BoundingBox transformedBox2 = GetTransformedAxisAlignedBoundingBox(box2, model2.WorldMatrix);
                if (transformedBox1.Intersects(transformedBox2))
                {
                    return true;
                }
            }
        }
        return false;
    }

    //method for getting a transformed axis-aligned bounding box given a local box and a transformation matrix
    BoundingBox GetTransformedAxisAlignedBoundingBox(BoundingBox localBox, Matrix transform)
    {
        Vector3[] localBoxVertices = localBox.GetCorners();
        Vector3[] transformedBoxVertices = new Vector3[localBoxVertices.Length];
        for (int i = 0; i < localBoxVertices.Length; i++)
        {
            transformedBoxVertices[i] = Vector3.Transform(localBoxVertices[i], transform);
        }
        return BoundingBox.CreateFromPoints(transformedBoxVertices);
    }

    /// <summary>
    /// Returns whether or not the oriented bounding boxes of 2 models intersect.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean: whether or not intersection is detected.</returns>
    public bool OrientedBoundingBoxIntersects(Model3D model2)
    {
        //creating the oriented bounding boxes and checking if they collide
        foreach (BoundingBox box1 in localAxisAlignedBoundingBoxList)
        {
            OrientedBoundingBox OBB1 = new OrientedBoundingBox(box1);
            OBB1.TransformPoints(worldMatrix);

            foreach (BoundingBox box2 in model2.LocalAxisAlignedBoundingBoxList)
            {
                OrientedBoundingBox OBB2 = new OrientedBoundingBox(box2);
                OBB2.TransformPoints(model2.WorldMatrix);

                if (OBB1.Intersects(OBB2))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns whether or not 2 models intersect using the optimized hierarchy: bounding spheres -> AABB -> OBB.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean: whether or not intersection is detected.</returns>
    public bool Intersects(Model3D model2)
    {
        if (!BoundingSphereIntersects(model2))
        {
            return false;
        }
        if (!AxisAlignedBoundingBoxIntersects(model2))
        {
            return false;
        }
        if (!OrientedBoundingBoxIntersects(model2))
        {
            return false;
        }
        return true;
    }
}
