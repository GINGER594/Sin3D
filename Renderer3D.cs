using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sin3D._Camera3D;
using Sin3D._Model3D;

namespace Sin3D._Renderer3D;

/// <summary>
/// A renderer class for drawing Sin3D Model3D objects.
/// </summary>
public class Renderer3D
{
    GraphicsDevice _graphicsDevice;

    bool fogEnabled;
    /// <summary>
    /// Whether or not fog is enabled for rendering.
    /// </summary>
    public bool FogEnabled { get => fogEnabled; set {fogEnabled = value;} }

    float fogStart;
    /// <summary>
    /// The distance at which fog rendering will start.
    /// </summary>
    public float FogStart { get => fogStart; set {fogStart = value;} }

    float fogEnd;
    /// <summary>
    /// The distance at which fog rendering will end.
    /// </summary>
    public float FogEnd { get => fogEnd; set {fogEnd = value;} }

    Vector3 fogColor;
    /// <summary>
    /// The 3D vector of values between 0 and 1 representing the fog color.
    /// </summary>
    public Vector3 FogColor { get => fogColor; set {fogColor = value;} }

    /// <summary>
    /// Creates a new Renderer3D object with the GraphicsDevice it will target.
    /// </summary>
    /// <param name="_graphicsDevice">The GraphicsDevice that the renderer will target.</param>
    public Renderer3D(GraphicsDevice _graphicsDevice)
    {
        this._graphicsDevice = _graphicsDevice;
    }

    /// <summary>
    /// Draws a Sin3D Model3D object.
    /// </summary>
    /// <param name="model">The model to be drawn.</param>
    /// <param name="camera">The camera.</param>
    public void DrawSin3DModel(Model3D model, Camera3D camera)
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

                //handling effect texture
                effect.TextureEnabled = model.TextureEnabled;
                if (effect.TextureEnabled)
                {
                    effect.Texture = model.Texture;
                }

                //handling fog
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
