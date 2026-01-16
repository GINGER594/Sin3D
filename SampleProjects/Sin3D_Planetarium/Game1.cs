using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sin3D._Camera3D;
using Sin3D._Renderer3D;
using Sin3D._Model3D;

namespace Sin3DPlanetarium;

public class Game1 : Game
{
    //renderer+camera settings
    Camera3D cam;
    float moveSpeed = 0.025f;
    float rotationSpeed = MathHelper.ToRadians(0.005f);
    Renderer3D renderer;

    //planetarium settings
    Model3D skybox;
    Model3D sun;
    float sunRotateSpeed = 12f;
    Model3D moon;
    float moonRotateSpeed = 0.001f;
    float moonOrbitSpeed = 0.25f;
    float moonOrbitAngle = 0f;
    Model3D[] planets;
    float[] dists = [300f, 500f, 800f, 1100f, 1500f, 2000f, 2500f, 3000f]; //distance from the sun
    float[] sizes = [10f, 35f, 40f, 22f, 80f, 75f, 60f, 50f]; //planet scale factors
    float[] rotateSpeeds = [0.0000688f, 0.0000414f, 0.01f, 10.5f, 0.0029f, 0.0034f, 19.4f, 16.17f]; //planet rotation speeds
    float[] orbitSpeeds = [0.00161f, 0.00117f, 0.001f, 0.000809f, 0.000440f, 0.000326f, 0.000228f, 0.000181f]; //planet orbit speeds

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
        
        Window.Title = "Sin3D Test";

        TargetElapsedTime = TimeSpan.FromMilliseconds(60f / 1000f);

        //setting depth stencil
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
    }

    protected override void Initialize()
    {
        //cam set up
        cam = new Camera3D(
            new Vector3(-1850f, 1200f, -2650f),
            Vector3.Zero,
            MathHelper.ToRadians(60f),
            0.01f,
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
        //creating skybox
        skybox = new Model3D(
            Vector3.Zero,
            Quaternion.Identity,
            5000f,
            Content.Load<Model>("skybox"),
            [Content.Load<Texture2D>("Inside Galaxy 4k HDRI_0")]
        );

        //creating planets/suns/moons
        Model sphereModel = Content.Load<Model>("smoothSphere");
        Model saturnModel = Content.Load<Model>("saturn");
        //creating sun
        sun = new Model3D(
            Vector3.Zero,
            new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0, 0, (float)Math.Cos(MathHelper.PiOver4)),
            100f,
            sphereModel,
            [Content.Load<Texture2D>("2k_sun")]
        );

        //creating moon
        moon = new Model3D(
            new Vector3(0f, 80f, 950f),
            new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)),
            5f,
            sphereModel,
            [Content.Load<Texture2D>("2k_moon")]
        );

        //creating planets
        planets = [
            new Model3D(new Vector3(0, 0, dists[0]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[0], sphereModel, [Content.Load<Texture2D>("2k_mercury")]),
            new Model3D(new Vector3(0, 0, dists[1]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[1], sphereModel, [Content.Load<Texture2D>("2k_venus_atmosphere")]),
            new Model3D(new Vector3(0, 0, dists[2]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[2], sphereModel, [Content.Load<Texture2D>("EarthComposited_2k")]),
            new Model3D(new Vector3(0, 0, dists[3]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[3], sphereModel, [Content.Load<Texture2D>("2k_mars")]),
            new Model3D(new Vector3(0, 0, dists[4]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[4], sphereModel, [Content.Load<Texture2D>("2k_jupiter")]),
            new Model3D(new Vector3(0, 0, dists[5]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[5], saturnModel, [Content.Load<Texture2D>("saturn_rings"), Content.Load<Texture2D>("2k_saturn")]),
            new Model3D(new Vector3(0, 0, dists[6]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[6], sphereModel, [Content.Load<Texture2D>("2k_uranus")]),
            new Model3D(new Vector3(0, 0, dists[7]), new Quaternion(-(float)Math.Sin(MathHelper.PiOver4), 0f, 0f, (float)Math.Cos(MathHelper.PiOver4)), sizes[7], sphereModel, [Content.Load<Texture2D>("2k_neptune")])
        ];
        planets[5].Rotation *= new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(20)), 0f, (float)Math.Cos(MathHelper.ToRadians(20))); //giving saturn its tilt
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
        moon.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.UnitZ), MathHelper.ToRadians(moonRotateSpeed));
        moonOrbitAngle += MathHelper.ToRadians(moonOrbitSpeed);
        moon.Position = Vector3.Transform(new Vector3(0f, 0f, 100f), new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(moonOrbitAngle / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(moonOrbitAngle / 2)))) + planets[2].Position;
        moon.UpdateWorldMatrix();

        //rotating+orbiting planets
        for (int i = 0; i < planets.Length; i++)
        {
            Model3D movedPlanet = planets[i];

            //rotating planet
            if (i == 5) //saturn local axial rotation
            {
                movedPlanet.Rotation *= new Quaternion(0f, 0f, (float)Math.Sin(MathHelper.ToRadians(rotateSpeeds[i] / 2)), (float)Math.Cos(MathHelper.ToRadians(rotateSpeeds[i] / 2)));
            }
            else
            {
                movedPlanet.Rotation = new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(rotateSpeeds[i] / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(rotateSpeeds[i] / 2))) * movedPlanet.Rotation;
            }

            //moving planet
            if (i == 1) //venus orbits backward
            {
                movedPlanet.Position = Vector3.Transform(movedPlanet.Position, new Quaternion(0f, -(float)Math.Sin(MathHelper.ToRadians(orbitSpeeds[i] / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(orbitSpeeds[i] / 2))));
            }
            else
            {
                movedPlanet.Position = Vector3.Transform(movedPlanet.Position, new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(orbitSpeeds[i] / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(orbitSpeeds[i] / 2))));
            }
            movedPlanet.UpdateWorldMatrix();
            planets[i] = movedPlanet;
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        //skybox
        renderer.DrawModel3D(skybox, cam);

        //sun
        renderer.DrawModel3D(sun, cam);

        //moon
        renderer.LightingEnabled = true;
        renderer.AmbientLightColor = Vector3.One;
        DirectionalLightPropertyGroup moonLight = new DirectionalLightPropertyGroup(true, moon.Position, new Vector3(0.004f, 0.0015f, 0f));
        renderer.DirectionalLight0 = moonLight;
        renderer.DrawModel3D(moon, cam);

        //planets
        foreach (Model3D planet in planets)
        {
            DirectionalLightPropertyGroup dirLight = new DirectionalLightPropertyGroup(true, planet.Position, new Vector3(0.004f, 0.0015f, 0f));
            renderer.DirectionalLight0 = dirLight;
            renderer.DrawModel3D(planet, cam);
        }

        renderer.ResetRenderingSettings();
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
