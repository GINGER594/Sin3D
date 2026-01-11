using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sin3D._OBB3D;

namespace Sin3D._Model3D;

/// <summary>
/// A model class for handling position, rotation, scale, texture-mapping and collision-detection.
/// </summary>
public class Model3D
{
    Vector3 position;
    /// <summary>
    /// The (x, y, z) position.
    /// </summary>
    public Vector3 Position { get => position; set {position = value;} }

    Quaternion rotation;
    /// <summary>
    /// The rotation of the model, represented by a quaternion.
    /// </summary>
    public Quaternion Rotation { get => rotation; set {rotation = value;} }

    float scale;
    /// <summary>
    /// The scale of the model.
    /// </summary>
    public float Scale { get => scale; set {scale = value;} }

    Model baseModel;
    /// <summary>
    /// The model - a collection of meshes.
    /// </summary>
    public Model BaseModel => baseModel;

    bool textureEnabled;
    /// <summary>
    /// Whether or not the model will render with textures.
    /// </summary>
    public bool TextureEnabled { get => textureEnabled; set {textureEnabled = value; }}

    List<Texture2D?> meshTextures = new ();
    /// <summary>
    /// A list of (nullable) 2D textures to be mapped onto the model's meshes in order.
    /// </summary>
    public List<Texture2D?> MeshTextures
    {
        get => meshTextures;
        set
        {
            if (value is not null)
            {
                textureEnabled = true;
                meshTextures = value;
            }
        }
    }
    
    Matrix worldMatrix;
    /// <summary>
    /// The world matrix of the model.
    /// </summary>
    public Matrix WorldMatrix => worldMatrix;

    List<BoundingBox> localAxisAlignedBoundingBoxList = new ();
    /// <summary>
    /// The list of local axis-aligned bounding boxes for a model's meshes.
    /// </summary>
    public List<BoundingBox> LocalAxisAlignedBoundingBoxList => localAxisAlignedBoundingBoxList;

    /// <summary>
    /// Creates a new Model3D object with position, rotation and scale settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position.</param>
    /// <param name="rotation">The initial quaternion rotation.</param>
    /// <param name="scale">The initial scale.</param>
    /// <param name="baseModel">The model.</param>
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
    /// Creates a new Model3D object with position, rotation, scale and texture settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position.</param>
    /// <param name="rotation">The initial quaternion rotation.</param>
    /// <param name="scale">The initial scale.</param>
    /// <param name="baseModel">The model.</param>
    /// <param name="meshTextures">The list of 2D textures to be mapped onto the model's meshes.</param>
    public Model3D(Vector3 position, Quaternion rotation, float scale, Model baseModel, List<Texture2D?> meshTextures)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.baseModel = baseModel;
        textureEnabled = true;
        this.meshTextures = meshTextures;

        UpdateWorldMatrix();
        CreateLocalAxisAlignedBoundingBoxList();
    }

    /// <summary>
    /// Updates the world matrix (to be used after changing scale, rotation or position).
    /// </summary>
    public void UpdateWorldMatrix()
    {
        worldMatrix = (
            Matrix.CreateScale(scale) *
            Matrix.CreateFromQuaternion(rotation) *
            Matrix.CreateTranslation(position)
        );
    }

    //creates a list of axis-aligned bounding boxes from the meshes of the baseModel
    void CreateLocalAxisAlignedBoundingBoxList()
    {
        localAxisAlignedBoundingBoxList = new();
        //Creating an AABB for each mesh
        foreach (ModelMesh mesh in baseModel.Meshes)
        {
            localAxisAlignedBoundingBoxList.Add(CreateBoundingBox(mesh));
        }
    }

    //Creates an axis-aligned bounding box around a mesh
    BoundingBox CreateBoundingBox(ModelMesh mesh)
    {
        Vector3 minVert = new Vector3(float.MaxValue);
        Vector3 maxVert = new Vector3(float.MinValue);
        
        foreach (ModelMeshPart meshPart in mesh.MeshParts)
        {
            int stride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData(meshPart.VertexOffset * stride, vertices, 0, meshPart.NumVertices, stride);
            foreach (VertexPositionNormalTexture vert in vertices)
            {
                Vector3 vertPoint = vert.Position;
                minVert = Vector3.Min(minVert, vertPoint);
                maxVert = Vector3.Max(maxVert, vertPoint);
            }
        }
        return new BoundingBox(minVert, maxVert);
    }

    /// <summary>
    /// Checks if the bounding spheres of 2 Model3D objects intersect.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean - whether or not an intersection was detected.</returns>
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
    /// Checks if the axis-aligned bounding boxes of 2 Model3D objects intersect.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean - whether or not an intersection was detected.</returns>
    public bool AxisAlignedBoundingBoxIntersects(Model3D model2)
    {
        //creating the transformed axis-aligned bounding boxes and checking if they collide
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

    //method for getting a transformed axis-aligned bounding box given a local AABB and a transformation matrix
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
    /// Checks if the oriented bounding boxes of 2 Model3D objects intersect.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean - whether or not an intersection was detected.</returns>
    public bool OrientedBoundingBoxIntersects(Model3D model2)
    {
        //creating the oriented bounding boxes (from the local AABBs) and checking if they collide
        foreach (BoundingBox box1 in localAxisAlignedBoundingBoxList)
        {
            OrientedBoundingBox3D OBB1 = new OrientedBoundingBox3D(box1);
            OBB1.TransformVertices(worldMatrix);

            foreach (BoundingBox box2 in model2.LocalAxisAlignedBoundingBoxList)
            {
                OrientedBoundingBox3D OBB2 = new OrientedBoundingBox3D(box2);
                OBB2.TransformVertices(model2.WorldMatrix);

                if (OBB1.Intersects(OBB2))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if 2 Model3D objects intersect using the hierarchy of methods: bounding spheres -> AABB -> OBB.
    /// </summary>
    /// <param name="model2">The other model.</param>
    /// <returns>Boolean - whether or not an intersection was detected.</returns>
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
