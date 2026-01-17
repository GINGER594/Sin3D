using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sin3D._Camera3D;
using Sin3D._Model3D;

namespace Sin3D._Renderer3D;

/// <summary>
/// A 3D renderer class used for drawing Model3D objects and handling ambient lighting, directional lighting and fog.
/// </summary>
public class Renderer3D
{
    GraphicsDevice _graphicsDevice;

    float effectAlpha;
    /// <summary>
    /// The alpha value that will be used in rendering.
    /// </summary>
    public float EffectAlpha { get => effectAlpha; set {effectAlpha = value;} }

    bool defaultLightingEnabled;
    /// <summary>
    /// Whether or not default lighting is enabled for rendering (overrides all other lighting settings).
    /// </summary>
    public bool DefaultLightingEnabled { get => defaultLightingEnabled; set {defaultLightingEnabled = value;} }

    bool lightingEnabled;
    /// <summary>
    /// Whether or not lighting is enabled for rendering.
    /// </summary>
    public bool LightingEnabled { get => lightingEnabled; set {lightingEnabled = value;} }

    Vector3 ambientLightColor;
    /// <summary>
    /// The floating point ambient light color for rendering (lighting must be enabled).
    /// </summary>
    public Vector3 AmbientLightColor { get => ambientLightColor; set {ambientLightColor = value;} }

    DirectionalLightPropertyGroup directionalLight0;
    /// <summary>
    /// The 1st directional light that can be used in rendering (lighting must be enabled).
    /// </summary>
    public DirectionalLightPropertyGroup DirectionalLight0 { get => directionalLight0; set {directionalLight0 = value;} }

    DirectionalLightPropertyGroup directionalLight1;
    /// <summary>
    /// The 2nd directional light that can be used in rendering (lighting must be enabled).
    /// </summary>
    public DirectionalLightPropertyGroup DirectionalLight1 { get => directionalLight1; set {directionalLight1 = value;} }

    DirectionalLightPropertyGroup directionalLight2;
    /// <summary>
    /// The 3rd directional light that can be used in rendering (lighting must be enabled).
    /// </summary>
    public DirectionalLightPropertyGroup DirectionalLight2 { get => directionalLight2; set {directionalLight2 = value;} }

    bool fogEnabled;
    /// <summary>
    /// Whether or not fog is enabled.
    /// </summary>
    public bool FogEnabled { get => fogEnabled; set {fogEnabled = value;} }

    Vector3 fogColor;
    /// <summary>
    /// The floating point fog color.
    /// </summary>
    public Vector3 FogColor { get => fogColor; set {fogColor = value;} }

    float fogStart;
    /// <summary>
    /// The distance that fog rendering will start at.
    /// </summary>
    public float FogStart { get => fogStart; set {fogStart = value;} }

    float fogEnd;
    /// <summary>
    /// The distance that fog rendering will end at.
    /// </summary>
    public float FogEnd { get => fogEnd; set {fogEnd = value;} }

    /// <summary>
    /// Creates a new Renderer3D object.
    /// </summary>
    /// <param name="_graphicsDevice">The graphics device that the renderer will target.</param>
    public Renderer3D(GraphicsDevice _graphicsDevice)
    {
        this._graphicsDevice = _graphicsDevice;
        ResetRenderingSettings();
    }

    /// <summary>
    /// Resets the rendering settings (alpha = 1f, all lighting disabled, fog disabled) (should be called before/after drawing a new object (unless you want settings to carry over)).
    /// </summary>
    public void ResetRenderingSettings()
    {
        //resetting depth stencil/blend state
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.BlendState = BlendState.Opaque;

        //resetting light fields
        effectAlpha = 1f;
        defaultLightingEnabled = false;
        lightingEnabled = false;
        ambientLightColor = Vector3.Zero;

        //resetting directional light fields
        directionalLight0 = new DirectionalLightPropertyGroup();
        directionalLight1 = new DirectionalLightPropertyGroup();
        directionalLight2 = new DirectionalLightPropertyGroup();

        //resetting fog fields
        fogEnabled = false;
        fogColor = Vector3.Zero;
        fogStart = 0f;
        fogEnd = 1f;
    }

    /// <summary>
    /// Draws a Model3D object (all opaque objects should be drawn before transparent objects).
    /// </summary>
    /// <param name="model">The model that will be drawn.</param>
    /// <param name="camera">The camera that will be used as the viewpoint from which to draw from.</param>
    public void DrawModel3D(Model3D model, Camera3D camera)
    {
        //handling transparency
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        if (effectAlpha != 1f)
        {
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        }

        //drawing
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
                effect.TextureEnabled = false; //ensuring texture is reset
                if (model.MeshTextures is not null && i < model.MeshTextures.Count)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = model.MeshTextures[i];
                }

                //handling effect lighting
                effect.Alpha = effectAlpha;
                if (defaultLightingEnabled)
                {
                    effect.EnableDefaultLighting();
                }
                else //if default lighting is not enabled - set up manual lighting
                {
                    effect.LightingEnabled = lightingEnabled;
                    effect.AmbientLightColor = ambientLightColor;

                    //handling directional effect lighting (and resetting the directional lights if EnableDefaultLighting() modified them)
                    effect.DirectionalLight0.Enabled = directionalLight0.Enabled;
                    effect.DirectionalLight0.Direction = directionalLight0.Direction;
                    effect.DirectionalLight0.DiffuseColor = directionalLight0.DiffuseColor;
                    effect.DirectionalLight0.SpecularColor = directionalLight0.SpecularColor;

                    effect.DirectionalLight1.Enabled = directionalLight1.Enabled;
                    effect.DirectionalLight1.Direction = directionalLight1.Direction;
                    effect.DirectionalLight1.DiffuseColor = directionalLight1.DiffuseColor;
                    effect.DirectionalLight1.SpecularColor = directionalLight1.SpecularColor;
                    
                    effect.DirectionalLight2.Enabled = directionalLight2.Enabled;
                    effect.DirectionalLight2.Direction = directionalLight2.Direction;
                    effect.DirectionalLight2.DiffuseColor = directionalLight2.DiffuseColor;
                    effect.DirectionalLight2.SpecularColor = directionalLight2.SpecularColor;
                }

                //handling effect fog
                effect.FogEnabled = fogEnabled;
                effect.FogColor = fogColor;
                effect.FogStart = fogStart;
                effect.FogEnd = fogEnd;
            }
            mesh.Draw();
        }
    }
}



/// <summary>
/// A group of properties for a directional light - used to set the DirectionalLight fields of a Renderer3D object.
/// </summary>
public struct DirectionalLightPropertyGroup
{
    bool enabled;
    /// <summary>
    /// Whether or not the directional light is enabled.
    /// </summary>
    public bool Enabled { get => enabled; set {enabled = value;} }

    Vector3 direction;
    /// <summary>
    /// The direction of the directional light.
    /// </summary>
    public Vector3 Direction { get => direction; set {direction = value;} }

    Vector3 diffuseColor;
    /// <summary>
    /// The diffuse color of the directional light.
    /// </summary>
    public Vector3 DiffuseColor { get => diffuseColor; set {diffuseColor = value;} }

    Vector3 specularColor;
    /// <summary>
    /// The specular color of the directional light.
    /// </summary>
    public Vector3 SpecularColor { get => specularColor; set {specularColor = value;} }

    /// <summary>
    /// Creates a new DirectionalLightPropertyGroup
    /// </summary>
    /// <param name="enabled">Whether or not the directional light is enabled.</param>
    /// <param name="direction">The direction of the directional light.</param>
    /// <param name="diffuseColor">The diffuse color of the directional light.</param>
    /// <param name="specularColor">The specular color of the directional light.</param>
    public DirectionalLightPropertyGroup(bool enabled, Vector3 direction, Vector3 diffuseColor, Vector3 specularColor)
    {
        this.enabled = enabled;
        this.direction = direction;
        this.diffuseColor = diffuseColor;
        this.specularColor = specularColor;
    }
}
