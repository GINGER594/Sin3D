using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sin3D._Camera;

/// <summary>
/// A camera class with position, yaw, pitch, roll and view/projection matrix fields. Uses Euler angles for 3D rotation.
/// </summary>
public class Camera3D
{
    //world data fields:
    Vector3 position;
    /// <summary>
    /// The (x, y, z) position of the camera.
    /// </summary>
    public Vector3 Position { get => position; set {position = value;}} 

    float yaw;
    /// <summary>
    /// Yaw (in radians).
    /// </summary>
    public float Yaw { get => yaw; set {yaw = value; }}

    float pitch;
    /// <summary>
    /// Pitch (in radians).
    /// </summary>
    public float Pitch { get => pitch; set {pitch = value; }}

    float roll;
    /// <summary>
    /// Roll (in radians).
    /// </summary>
    public float Roll { get => roll; set {roll = value; }}


    //projection matrix fields:
    float fov;
    /// <summary>
    /// Field of view (in radians) (setter will only assign fov values between 0 and PI).
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
    /// The near plane distance.
    /// </summary>
    public float NearPlaneDist { get => nearPlaneDist; set {nearPlaneDist = value;} }

    float farPlaneDist;
    /// <summary>
    /// The far plane distance.
    /// </summary>
    public float FarPlaneDist { get => farPlaneDist; set {farPlaneDist = value;} }


    //matrices:
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
    /// Creates a new Camera3D object with position, rotation, fov and near/far plane distance settings.
    /// </summary>
    /// <param name="position">The initial (x, y, z) position.</param>
    /// <param name="rotation">The initial (yaw, pitch, roll) rotation.</param>
    /// <param name="fov">The initial fov (in radians).</param>
    /// <param name="nearPlaneDist">The initial near plane distance.</param>
    /// <param name="farPlaneDist">The initial far plane distance.</param>
    /// <param name="_graphicsDevice">The GraphicsDevice (used in projection matrix creation).</param>
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
    /// Updates the view matrix of the camera (to be used after changing position or rotation).
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
    /// Updates the projection matrix of the camera (to be used after changing the fov, near plane distance, or far plane distance).
    /// </summary>
    /// <param name="_graphicsDevice">The GraphicsDevice (used in projection matrix creation).</param>
    public void UpdateProjectionMatrix(GraphicsDevice _graphicsDevice)
    {
        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fov, _graphicsDevice.Viewport.AspectRatio, nearPlaneDist, farPlaneDist);
    }
}
