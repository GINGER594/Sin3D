using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sin3D._Camera3D;
using Sin3D._Model3D;

namespace Sin3D._Renderer3D;

/// <summary>
/// A renderer class for drawing Model3D objects.
/// </summary>
public class Renderer3D
{
    GraphicsDevice _graphicsDevice;

    bool fogEnabled;
    /// <summary>
    /// Whether or not fog is enabled.
    /// </summary>
    public bool FogEnabled { get => fogEnabled; set {fogEnabled = value;} }

    float fogStart;
    /// <summary>
    /// The distance fog rendering will start at.
    /// </summary>
    public float FogStart { get => fogStart; set {fogStart = value;} }

    float fogEnd;
    /// <summary>
    /// The distance fog rendering will end at.
    /// </summary>
    public float FogEnd { get => fogEnd; set {fogEnd = value;} }

    Vector3 fogColor;
    /// <summary>
    /// The color of the fog (represented by a a Vector3 of RGB values between 0 and 1).
    /// </summary>
    public Vector3 FogColor { get => fogColor; set {fogColor = value;} }

    /// <summary>
    /// Creates a new Renderer3D object.
    /// </summary>
    /// <param name="_graphicsDevice">The GraphicsDevice that the renderer will target.</param>
    public Renderer3D(GraphicsDevice _graphicsDevice)
    {
        this._graphicsDevice = _graphicsDevice;
    }

    /// <summary>
    /// Draws a Model3D object.
    /// </summary>
    /// <param name="model">The model that will be drawn.</param>
    /// <param name="camera">The camera that the provides the view and projection matrices for rendering.</param>
    public void DrawModel3D(Model3D model, Camera3D camera)
    {
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        for (int i = 0; i < model.BaseModel.Meshes.Count; i++)
        {
            ModelMesh mesh = model.BaseModel.Meshes[i];
            foreach (BasicEffect effect in mesh.Effects.Cast<BasicEffect>())
            {
                //setting up the effect to draw the model
                effect.World = model.WorldMatrix;
                effect.View = camera.ViewMatrix;
                effect.Projection = camera.ProjectionMatrix;

                //handling effect texture
                effect.TextureEnabled = model.TextureEnabled;
                effect.Texture = model.MeshTextures[i];

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
