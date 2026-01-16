using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sin3D._Camera3D;
using Sin3D._Renderer3D;
using Sin3D._Model3D;
using planet;

namespace Sin3DPlanetarium;

public class Game1 : Game
{
    //renderer+camera settings
    Camera3D cam;
    float moveSpeed = 0.025f;
    float rotationSpeed = MathHelper.ToRadians(0.005f);
    Renderer3D renderer;

    //skybox/sun settings
    Model3D skybox;
    Model3D sun;
    float sunRotateSpeed = 12f;
    Vector3 ambientLightColor = new Vector3(0.95f, 0.95f, 0.95f);
    Vector3 sunLightColor = new Vector3(0.004f, 0.0015f, 0f);

    //moon settings
    Moon moon;
    float moonRotateSpeed = 0.001f;
    float moonOrbitSpeed = 0.25f;
    Vector3 relativeMoonPos = new Vector3(0f, 30f, 100f); //the position of the moon relative to the earth

    //planet settings
    Planet[] planets;
    float[] dists = [300f, 500f, 800f, 1100f, 1500f, 2000f, 2500f, 3000f]; //distance from the sun
    float[] sizes = [10f, 35f, 40f, 22f, 80f, 75f, 60f, 50f]; //planet scale factors
    float[] rotateSpeeds = [0.000688f, 0.00414f, 0.01f, 0.005f, 0.0029f, 0.0034f, 19.4f, 16.17f]; //planet rotation speeds
    float[] orbitSpeeds = [0.00161f, 0.00117f, 0.001f, 0.000809f, 0.000440f, 0.000326f, 0.000228f, 0.000181f]; //planet orbit speeds

    //trail settings
    Model trailModel;
    Texture2D trailTexture;
    float trailParticleScale = 3f;
    float minDistanceBetweenTrailParticles = 1f;
    float[] trailLengths = [400f, 650f, 900f, 1150f, 1400f, 1650f, 1900f, 2150f];

    private GraphicsDeviceManager _graphics;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;

        // _graphics.PreferredBackBufferWidth = 1280;
        // _graphics.PreferredBackBufferHeight = 720;
        // _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        
        Window.Title = "Sin3D Planetarium";

        TargetElapsedTime = TimeSpan.FromMilliseconds(60f / 1000f);
    }

    protected override void Initialize()
    {
        //cam set up
        cam = new Camera3D(
            new Vector3(-1450f, 900f, -2050f),
            Vector3.Zero,
            MathHelper.ToRadians(60f),
            1f,
            30000f,
            GraphicsDevice
        );
        cam.Yaw = MathHelper.ToRadians(215);
        cam.Pitch = MathHelper.ToRadians(-21);

        //renderer set up
        renderer = new Renderer3D(
            GraphicsDevice
        );

        base.Initialize();
    }

    protected override void LoadContent()
    {
        //loading models
        Model skyBoxModel = Content.Load<Model>("skybox");
        Model sphereModel = Content.Load<Model>("smoothSphere");
        Model saturnModel = Content.Load<Model>("saturn");

        //creating skybox
        skybox = new Model3D(
            Vector3.Zero,
            Quaternion.Identity,
            5000f,
            skyBoxModel,
            [Content.Load<Texture2D>("Inside Galaxy 4k HDRI_0")]
        );

        //creating sun
        sun = new Model3D(
            Vector3.Zero,
            new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0, 0, (float)Math.Cos(MathHelper.PiOver4)),
            100f,
            sphereModel,
            [Content.Load<Texture2D>("2k_sun")]
        );

        //creating/loading trail assets
        trailModel = sphereModel;
        trailTexture = new Texture2D(GraphicsDevice, 1, 1);
        trailTexture.SetData([ new Color(253, 221, 142) ]);

        //creating moon
        Model3D moonModel = new Model3D(
            new Vector3(0f, 80f, 950f),
            new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)),
            5f,
            sphereModel,
            [Content.Load<Texture2D>("2k_moon")]
        );
        moon = new Moon(moonRotateSpeed, moonOrbitSpeed, moonModel, trailModel, trailTexture, trailParticleScale, minDistanceBetweenTrailParticles, 0f, relativeMoonPos);

        //creating planets
        Model3D[] planetModels = [
            new Model3D(new Vector3(0f, -20f, dists[0]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[0], sphereModel, [Content.Load<Texture2D>("2k_mercury")]),
            new Model3D(new Vector3(0f, 10f, dists[1]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[1], sphereModel, [Content.Load<Texture2D>("2k_venus_atmosphere")]),
            new Model3D(new Vector3(0f, 0f, dists[2]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[2], sphereModel, [Content.Load<Texture2D>("EarthComposited_2k")]),
            new Model3D(new Vector3(0f, 20f, dists[3]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[3], sphereModel, [Content.Load<Texture2D>("2k_mars")]),
            new Model3D(new Vector3(0f, -10f, dists[4]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[4], sphereModel, [Content.Load<Texture2D>("2k_jupiter")]),
            new Model3D(new Vector3(0f, -25f, dists[5]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[5], saturnModel, [Content.Load<Texture2D>("saturn_rings"), Content.Load<Texture2D>("2k_saturn")]),
            new Model3D(new Vector3(0f, 0f, dists[6]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[6], sphereModel, [Content.Load<Texture2D>("2k_uranus")]),
            new Model3D(new Vector3(0f, 5f, dists[7]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[7], sphereModel, [Content.Load<Texture2D>("2k_neptune")])
        ];
        //creating an array of planets from the data
        planets = new Planet[planetModels.Length];
        for (int i = 0; i < planetModels.Length; i++)
        {
            if (i == 1)
            {
                planets[i] = new Venus(rotateSpeeds[i], orbitSpeeds[i], planetModels[i], trailModel, trailTexture, trailParticleScale, minDistanceBetweenTrailParticles, trailLengths[i]);
            }
            else if (i == 5)
            {
                planets[i] = new Saturn(rotateSpeeds[i], orbitSpeeds[i], planetModels[i], trailModel, trailTexture, trailParticleScale, minDistanceBetweenTrailParticles, trailLengths[i]);
            }
            else
            {
                planets[i] = new Planet(rotateSpeeds[i], orbitSpeeds[i], planetModels[i], trailModel, trailTexture, trailParticleScale, minDistanceBetweenTrailParticles, trailLengths[i]);
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keystate = Keyboard.GetState();
        HandleCameraInputs(keystate);

        //rotating sun
        sun.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.UnitZ), MathHelper.ToRadians(sunRotateSpeed));
        sun.UpdateWorldMatrix();

        //rotating+orbiting moon
        moon.Rotate();
        moon.OrbitPlanet(planets[2]);

        //rotating+orbitng planets
        foreach (Planet planet in planets)
        {
            planet.Rotate();
            planet.Orbit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        renderer.ResetRenderingSettings();

        //drawing skybox
        renderer.DrawModel3D(skybox, cam);

        //drawing sun
        renderer.DrawModel3D(sun, cam);

        //drawing moon
        renderer.LightingEnabled = true;
        renderer.AmbientLightColor = ambientLightColor;
        DirectionalLightPropertyGroup moonLight = new DirectionalLightPropertyGroup(true, moon.PlanetModel.Position, sunLightColor);
        renderer.DirectionalLight0 = moonLight;
        renderer.DrawModel3D(moon.PlanetModel, cam);
        renderer.ResetRenderingSettings();

        //drawing planets
        foreach (Planet planet in planets)
        {
            //drawing planet
            renderer.LightingEnabled = true;
            renderer.AmbientLightColor = ambientLightColor;
            DirectionalLightPropertyGroup dirLight = new DirectionalLightPropertyGroup(true, planet.PlanetModel.Position, sunLightColor);
            renderer.DirectionalLight0 = dirLight;
            renderer.DrawModel3D(planet.PlanetModel, cam);
            renderer.ResetRenderingSettings();

            //drawing trail
            for (int i = 0; i < planet.Trail.Count; i++)
            {
                renderer.EffectAlpha = 1f - (Math.Abs(i - (planet.Trail.Count / 2f)) / (planet.Trail.Count / 2f));
                renderer.DrawModel3D(planet.Trail[i], cam);
                renderer.ResetRenderingSettings();
            }
        }
        base.Draw(gameTime);
    }

    void HandleCameraInputs(KeyboardState keystate)
    {
        //cam wasd movement
        Vector3 localForward = Vector3.Transform(new Vector3(0f, 0f, -moveSpeed), Matrix.CreateFromYawPitchRoll(cam.Yaw, cam.Pitch, 0f));
        Vector3 localRight = Vector3.Transform(new Vector3(moveSpeed, 0f, 0f), Matrix.CreateFromYawPitchRoll(cam.Yaw, 0f, 0f));
        if (keystate.IsKeyDown(Keys.W))
        {
            cam.Position += localForward;
        }
        if (keystate.IsKeyDown(Keys.S))
        {
            cam.Position -= localForward;
        }
        if (keystate.IsKeyDown(Keys.A))
        {
            cam.Position -= localRight;
        }
        if (keystate.IsKeyDown(Keys.D))
        {
            cam.Position += localRight;
        }
        //cam up/down movement
        if (keystate.IsKeyDown(Keys.OemPlus))
        {
            cam.Position += new Vector3(0, moveSpeed, 0);
        }
        if (keystate.IsKeyDown(Keys.OemMinus))
        {
            cam.Position -= new Vector3(0, moveSpeed, 0);
        }

        //cam rotation
        if (keystate.IsKeyDown(Keys.Left))
        {
            cam.Yaw += rotationSpeed;
        }
        if (keystate.IsKeyDown(Keys.Right))
        {
            cam.Yaw -= rotationSpeed;
        }
        if (keystate.IsKeyDown(Keys.Up))
        {
            cam.Pitch += rotationSpeed;
        }
        if (keystate.IsKeyDown(Keys.Down))
        {
            cam.Pitch -= rotationSpeed;
        }

        cam.Pitch = Math.Clamp(cam.Pitch, -1.55f, 1.55f);
        cam.UpdateViewMatrix();
    }
}
