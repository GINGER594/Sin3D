using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sin3D._Camera3D;

/// <summary>
/// A 3D camera class that handles view and projection matrices.
/// </summary>
public class Camera3D
{
    Vector3 position;
    /// <summary>
    /// The (x, y, z) position.
    /// </summary>
    public Vector3 Position { get => position; set {position = value;}} 

    float yaw;
    /// <summary>
    /// The yaw (in radians).
    /// </summary>
    public float Yaw { get => yaw; set {yaw = value; }}

    float pitch;
    /// <summary>
    /// The pitch (in radians).
    /// </summary>
    public float Pitch { get => pitch; set {pitch = value; }}

    float roll;
    /// <summary>
    /// The roll (in radians).
    /// </summary>
    public float Roll { get => roll; set {roll = value; }}

    float fov;
    /// <summary>
    /// The field of view (in radians), (the setter will only assign fov values between 0 and PI).
    /// </summary>
    public float Fov
    {
        get => fov;
        set
        {
            if (value > 0 && value < Math.PI)
            {
                fov = value;
            }
        }
    }

    float nearPlaneDist;
    /// <summary>
    /// The near plane render distance.
    /// </summary>
    public float NearPlaneDist { get => nearPlaneDist; set {nearPlaneDist = value;} }

    float farPlaneDist;
    /// <summary>
    /// The far plane render distance.
    /// </summary>
    public float FarPlaneDist { get => farPlaneDist; set {farPlaneDist = value;} }

    Matrix viewMatrix;
    /// <summary>
    /// The view matrix.
    /// </summary>
    public Matrix ViewMatrix => viewMatrix;

    Matrix projectionMatrix;
    /// <summary>
    /// The projection matrix.
    /// </summary>
    public Matrix ProjectionMatrix => projectionMatrix;

    /// <summary>
    /// Creates a new Camera3D object with position, rotation, fov and near/far plane render distance settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position.</param>
    /// <param name="rotation">The initial (yaw, pitch, roll) rotation.</param>
    /// <param name="fov">The initial field of view.</param>
    /// <param name="nearPlaneDist">The initial near plane render distance.</param>
    /// <param name="farPlaneDist">The initial far plane render distance.</param>
    /// <param name="_graphicsDevice">The graphics device, used in creating the projection matrix.</param>
    public Camera3D(Vector3 position, Vector3 rotation, float fov, float nearPlaneDist, float farPlaneDist, GraphicsDevice _graphicsDevice)
    {
        this.position = position;

        yaw = rotation.X;
        pitch = rotation.Y;
        roll = rotation.Z;

        this.fov = fov;
        this.nearPlaneDist = nearPlaneDist;
        this.farPlaneDist = farPlaneDist;

        //setting up the view and projection matrices
        UpdateViewMatrix();
        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fov, _graphicsDevice.Viewport.AspectRatio, nearPlaneDist, farPlaneDist);
    }

    /// <summary>
    /// Updates the camera's view matrix (to be used after changing position or rotation).
    /// </summary>
    public void UpdateViewMatrix()
    {
        //getting cam target
        Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
        Vector3 direction = Vector3.Transform(Vector3.Forward, rotationMatrix);
        Vector3 target = direction + position;

        viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
    }

    /// <summary>
    /// Updates the camera's projection matrix (to be used after changing fov or the near/far plane distance).
    /// </summary>
    /// <param name="_graphicsDevice">The graphics device used in the creation of the projection matrix.</param>
    public void UpdateProjectionMatrix(GraphicsDevice _graphicsDevice)
    {
        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fov, _graphicsDevice.Viewport.AspectRatio, nearPlaneDist, farPlaneDist);
    }
}

