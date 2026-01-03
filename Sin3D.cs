using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sin3D;



/// <summary>
/// A camera class that handles view and projection matrices - used for rendering with a Sin3DRenderer object.
/// </summary>
public class Sin3DCamera
{
    Vector3 position;
    /// <summary>
    /// The (x, y, z) position of the camera.
    /// </summary>
    public Vector3 Position { get => position; set {position = value;} }

    Vector3 rotation;
    /// <summary>
    /// A 3D vector representing the (yaw, pitch, roll) rotation of the camera.
    /// </summary>
    public Vector3 Rotation { get => rotation; set {rotation = value;} }

    Vector3 target = Vector3.Zero;

    Matrix viewMatrix;
    /// <summary>
    /// The view matrix of the camera.
    /// </summary>
    public Matrix ViewMatrix => viewMatrix;

    Matrix projectionMatrix;
    /// <summary>
    /// The projection matrix of the camera.
    /// </summary>
    public Matrix ProjectionMatrix => projectionMatrix;

    /// <summary>
    /// Creates new Sin3DCamera with position, rotation, projection settings.
    /// </summary>
    /// <param name="position">Initial (x, y, z) Position of the Camera.</param>
    /// <param name="rotation">Initial (yaw, pitch, roll) rotation of the camera.</param>
    /// <param name="fov">Field of view (in radians).</param>
    /// <param name="nearPlaneDist">Distance of the near clipping plane.</param>
    /// <param name="farPlaneDist">Distance of the far clipping plane.</param>
    /// <param name="_graphicsDevice">The GraphicsDevice for aspect ratio calculation.</param>
    public Sin3DCamera(Vector3 position, Vector3 rotation, float fov, float nearPlaneDist, float farPlaneDist, GraphicsDevice _graphicsDevice)
    {
        this.position = position;
        this.rotation = rotation;

        //setting up the view and projection matrices
        UpdateViewMatrix();
        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fov, _graphicsDevice.Viewport.AspectRatio, nearPlaneDist, farPlaneDist);
    }

    /// <summary>
    /// Updates the cameras view matrix (to be used after moving/rotating the camera).
    /// </summary>
    public void UpdateViewMatrix()
    {
        Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
        Vector3 direction = Vector3.Transform(Vector3.Forward, rotationMatrix);
        target = position + direction;
        
        viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
    }
}



/// <summary>
/// A simple renderer class used to draw Sin3DModel objects.
/// </summary>
public class Sin3DRenderer
{
    GraphicsDevice _graphicsDevice;
    /// <summary>
    /// The GraphicsDevice used for handling depth stencil states.
    /// </summary>
    public GraphicsDevice _GraphicsDevice => _graphicsDevice;

    bool fogEnabled;
    /// <summary>
    /// Boolean representing if fog is enabled.
    /// </summary>
    public bool FogEnabled { get => fogEnabled; set {fogEnabled = value;} }

    float fogStart;
    /// <summary>
    /// The floating point value that fog rendering on models will start at.
    /// </summary>
    public float FogStart { get => fogStart; set {fogStart = value;} }

    float fogEnd;
    /// <summary>
    /// The floating point value that fog rendering on models will end at.
    /// </summary>
    public float FogEnd { get => fogEnd; set {fogEnd = value;} }

    Vector3 fogColor;
    /// <summary>
    /// A 3D vector of floating point values ranging from 0 to 1 represting the RGB color of the fog.
    /// </summary>
    public Vector3 FogColor { get => fogColor; set {fogColor = value;} }

    /// <summary>
    /// Creates a new Sin3D renderer.
    /// </summary>
    /// <param name="_graphicsDevice">The GraphicsDevice used for handling depth stencil states.</param>
    public Sin3DRenderer(GraphicsDevice _graphicsDevice)
    {
        this._graphicsDevice = _graphicsDevice;
    }

    /// <summary>
    /// Draws a Sin3DModel. Note - The baseModel field of the Sin3DModel must not be null.
    /// </summary>
    /// <param name="model">The Sin3DModel to be drawn.</param>
    /// <param name="camera">The Sin3DCamera used to display what the camera can see.</param>
    public void DrawSin3DModel(Sin3DModel model, Sin3DCamera camera)
    {
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        foreach (ModelMesh mesh in model.BaseModel.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects.Cast<BasicEffect>())
            {
                //setting up the effect to draw the model
                effect.World = model.WorldMatrix;
                effect.View = camera.ViewMatrix;
                effect.Projection = camera.ProjectionMatrix;

                effect.TextureEnabled = model.TextureEnabled;
                if (effect.TextureEnabled)
                {
                    effect.Texture = model.Texture;
                }

                //handling fog (if necessary)
                if (fogEnabled)
                {
                    effect.FogEnabled = true;
                    effect.FogStart = fogStart;
                    effect.FogEnd = fogEnd;
                    effect.FogColor = fogColor;
                }
            }
            mesh.Draw();
        }
    }
}



/// <summary>
/// Represents a 3D model in 3D space with built-in collision-detection methods.
/// </summary>
public class Sin3DModel
{
    Vector3 position;
    /// <summary>
    /// The (x, y, z) position of the model.
    /// </summary>
    public Vector3 Position { get => position; set {position = value;} }

    Vector3 rotation;
    /// <summary>
    /// A 3D vector representing the (yaw, pitch, roll) rotation of the model.
    /// </summary>
    public Vector3 Rotation { get => rotation; set {rotation = value;} }

    float scale;
    /// <summary>
    /// A floating point value representing the scale of the model.
    /// </summary>
    public float Scale { get => scale; set {scale = value;} }

    Model baseModel;
    /// <summary>
    /// The base model - the actual model used for rendering, collisions etc.
    /// </summary>
    public Model BaseModel => baseModel;

    bool textureEnabled;
    /// <summary>
    /// Whether or not the model is texture-enabled.
    /// </summary>
    public bool TextureEnabled => textureEnabled;

    Texture2D? texture;
    /// <summary>
    /// The texture that will be drawn onto the model.
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
    /// The world matrix of the model, dictating its scale, rotation, and position in 3d space.
    /// </summary>
    public Matrix WorldMatrix => worldMatrix;

    BoundingBox localAxisAlignedBoundingBox;
    /// <summary>
    /// The local axis-aligned bounding box of the model - used to derive the local OBB, the non-local AABB, and the non-local OBB for collision detection.
    /// </summary>
    public BoundingBox LocalAxisAlignedBoundingBox => localAxisAlignedBoundingBox;

    /// <summary>
    /// Creates a new Sin3DModel with position, rotation, scale, and model settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position of the model.</param>
    /// <param name="rotation">The initial (yaw, pitch, roll) of the model.</param>
    /// <param name="scale">The initial scale of the model.</param>
    /// <param name="baseModel">The base model - the actual model used for rendering, collisions etc.</param>
    public Sin3DModel(Vector3 position, Vector3 rotation, float scale, Model baseModel)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.baseModel = baseModel;

        UpdateWorldMatrix();
        CreateLocalAxisAlignedBoundingBox();
    }

    /// <summary>
    /// Creates a new Sin3DModel with position, rotation, scale, model and texture settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position of the model.</param>
    /// <param name="rotation">The initial (yaw, pitch, roll) of the model.</param>
    /// <param name="scale">The initial scale of the model.</param>
    /// <param name="baseModel">The base model - the actual model used for rendering, collisions etc.</param>
    /// <param name="texture">The texture that will be drawn onto the model.</param>
    public Sin3DModel(Vector3 position, Vector3 rotation, float scale, Model baseModel, Texture2D texture)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.baseModel = baseModel;
        textureEnabled = true;
        this.texture = texture;

        UpdateWorldMatrix();
        CreateLocalAxisAlignedBoundingBox();
    }

    /// <summary>
    /// Updates the world matrix of the model (to be used after a change in position, rotation or scale).
    /// </summary>
    public void UpdateWorldMatrix()
    {
        worldMatrix = (
            Matrix.CreateScale(scale) *
            Matrix.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) *
            Matrix.CreateTranslation(position)
        );
    }

    //method for creating a new local axis-aligned bounding box
    void CreateLocalAxisAlignedBoundingBox()
    {
        Vector3 minVert = Vector3.Zero;
        Vector3 maxVert = Vector3.Zero;
        //finding the minimum and maximum vertices of the model
        foreach (ModelMesh mesh in baseModel.Meshes)
        {
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
        }
        localAxisAlignedBoundingBox = new BoundingBox(minVert, maxVert);
    }

    /// <summary>
    /// Checks if the bounding sphere of another model intersects with the bounding sphere of this model.
    /// </summary>
    /// <param name="model2">The other model that will have collision checks run on it.</param>
    /// <returns>Boolean result of the collision checks.</returns>
    public bool BoundingSphereIntersects(Sin3DModel model2)
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
    /// Checks if the axis-aligned bounding box of another model intersects with the axis-aligned bounding box of this model.
    /// </summary>
    /// <param name="model2">The other model that will have collision checks run on it.</param>
    /// <returns>Boolean result of the collision checks.</returns>
    public bool AxisAlignedBoundingBoxIntersects(Sin3DModel model2)
    {
        //creating the axis-aligned bounding boxes
        BoundingBox transformedBox1 = GetTransformedAxisAlignedBoundingBox(localAxisAlignedBoundingBox, worldMatrix);
        BoundingBox transformedBox2 = GetTransformedAxisAlignedBoundingBox(model2.LocalAxisAlignedBoundingBox, model2.WorldMatrix);
        return transformedBox1.Intersects(transformedBox2);
    }

    //method for getting a transformed axis-aligned bounding box given a local box and a transformation matrix
    BoundingBox GetTransformedAxisAlignedBoundingBox(BoundingBox localBox, Matrix transformationMatrix)
    {
        Vector3[] localBoxVertices = localBox.GetCorners();
        Vector3[] transformedBoxVertices = new Vector3[localBoxVertices.Length];
        for (int i = 0; i < localBoxVertices.Length; i++)
        {
            transformedBoxVertices[i] = Vector3.Transform(localBoxVertices[i], transformationMatrix);
        }
        return BoundingBox.CreateFromPoints(transformedBoxVertices);
    }

    /// <summary>
    /// Checks if the oriented bounding box of another model intersects with the oriented bounding box of this model.
    /// </summary>
    /// <param name="model2">The other model that will have collision checks run on it.</param>
    /// <returns>Boolean result of the collision checks.</returns>
    public bool OrientedBoundingBoxIntersects(Sin3DModel model2)
    {
        Sin3DOrientedBoundingBox OBB1 = new Sin3DOrientedBoundingBox(localAxisAlignedBoundingBox);
        OBB1.TransformPoints(worldMatrix);

        Sin3DOrientedBoundingBox OBB2 = new Sin3DOrientedBoundingBox(model2.LocalAxisAlignedBoundingBox);
        OBB2.TransformPoints(model2.WorldMatrix);
        return OBB1.Intersects(OBB2);
    }

    /// <summary>
    /// Checks if another model intersects with this model using the optimized: bounding spheres -> AABB -> OBB hierarchy.
    /// </summary>
    /// <param name="model2">The other model that will have collision checks run on it.</param>
    /// <returns>Boolean result of the collision checks.</returns>
    public bool Intersects(Sin3DModel model2)
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



/// <summary>
/// Represents an oriented bounding box in 3D space.
/// </summary>
public class Sin3DOrientedBoundingBox
{
    private Vector3[] vertices;
    /// <summary>
    /// An array of the 8 vertices of the oriented bounding box.
    /// </summary>
    public Vector3[] Vertices => vertices;

    float minAxisLength = 0.01f;

    /// <summary>
    /// Creates a new oriented bounding box from an axis-aligned bounding box.
    /// </summary>
    /// <param name="AABB">The axis-aligned bounding box used in the creation of the oriented bounding box.</param>
    /// <returns>A new Sin£DOrientedBoundingBox object created from an axis-aligned bounding box.</returns>
    public Sin3DOrientedBoundingBox (BoundingBox AABB)
    {
        vertices = AABB.GetCorners();
    }

    /// <summary>
    /// Transforms the vertices of the oriented bounding box by a matrix.
    /// </summary>
    /// <param name="transformationMatrix">The matrix that the vertices will be transformed by.</param>
    public void TransformPoints(Matrix transformationMatrix)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Transform(vertices[i], transformationMatrix);
        }
    }

    /// <summary>
    /// Checks if 2 oriented bounding boxes intersect using SAT projection.
    /// </summary>
    /// <param name="box2">The other box that will have collision checks run on it.</param>
    /// <returns>Boolean result of collision checks.</returns>
    public bool Intersects(Sin3DOrientedBoundingBox box2)
    {
        //getting the local axes of this box and box 2
        Vector3[] box1LocalAxes = GetLocalAxes(vertices);
        Vector3[] box2LocalAxes = GetLocalAxes(box2.Vertices);
        //getting the cross axes of the local box1 & box2 axes
        Vector3[] crossAxes = GetCrossAxes(box1LocalAxes, box2LocalAxes);
        //merging the three axis arrays, normalizing the axes, and removing any axes with a length that tends to zero
        List<Vector3> normalizedAxes = MergeAndNormalizeAxisArrays(box1LocalAxes, box2LocalAxes, crossAxes);

        //running the collision checks
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
        List<Vector3> axes = new();
        //local axes 1
        for (int i = 0; i < arr1.Length; i++)
        {
            if (arr1[i].Length() > minAxisLength)
            {
                axes.Add(Vector3.Normalize(arr1[i]));
            }
        }
        //local axes 2
        for (int i = 0; i < arr2.Length; i++)
        {
            if (arr2[i].Length() > minAxisLength)
            {
                axes.Add(Vector3.Normalize(arr2[i]));
            }
        }
        //cross axes
        for (int i = 0; i < arr3.Length; i++)
        {
            if (arr3[i].Length() > minAxisLength)
            {
                axes.Add(Vector3.Normalize(arr3[i]));
            }
        }

        return axes;
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
