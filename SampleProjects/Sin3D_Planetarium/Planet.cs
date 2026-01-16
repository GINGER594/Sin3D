using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sin3D._Model3D;

namespace planet;

//planet class for handling rotations and orbits
class Planet
{
    //planet settings
    protected float rotateSpeed;
    protected float orbitSpeed;
    protected Model3D planetModel;
    public Model3D PlanetModel => planetModel;

    //trail settings
    Model trailModel;
    Texture2D trailTexture;
    float trailParticleScale;
    float minDistanceBetweenTrailParticles;
    float distanceSinceLastTrailParticle = 0f;
    float trailLength;
    List<Model3D> trail = new ();
    public List<Model3D> Trail => trail;
    public Planet(float rotateSpeed, float orbitSpeed, Model3D model, Model trailModel, Texture2D trailTexture, float trailParticleScale, float minDistanceBetweenTrailParticles, float trailLength)
    {
        this.rotateSpeed = rotateSpeed;
        this.orbitSpeed = orbitSpeed;
        planetModel = model;

        this.trailModel = trailModel;
        this.trailTexture = trailTexture;
        this.trailParticleScale = trailParticleScale;
        this.minDistanceBetweenTrailParticles = minDistanceBetweenTrailParticles;
        this.trailLength = trailLength;
    }

    //rotates the planet around the global y axis
    public virtual void Rotate()
    {
        planetModel.Rotation = new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(rotateSpeed / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(rotateSpeed / 2))) * planetModel.Rotation;
        planetModel.UpdateWorldMatrix();
    }

    //moves the planet along its orbit and updates its trail
    public virtual void Orbit()
    {
        Vector3 prevPos = planetModel.Position;

        planetModel.Position = Vector3.Transform(planetModel.Position, new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(orbitSpeed / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(orbitSpeed / 2))));
        planetModel.UpdateWorldMatrix();

        //trail
        Vector3 newPos = planetModel.Position;
        float dist = Vector3.Distance(prevPos, newPos);
        distanceSinceLastTrailParticle += dist;
        if (distanceSinceLastTrailParticle > minDistanceBetweenTrailParticles)
        {
            distanceSinceLastTrailParticle = 0f;
            trail.Insert(0, new Model3D(prevPos, Quaternion.Identity, trailParticleScale, trailModel, [trailTexture]));
            if (trail.Count >= trailLength)
            {
                trail.RemoveAt(trail.Count - 1);
            }
        }
    }
}



//venus child class for anti-clockwise rotations
class Venus : Planet
{
    public Venus(float rotateSpeed, float orbitSpeed, Model3D model, Model trailModel, Texture2D trailTexture, float trailParticleScale, float minDistanceBetweenTrailParticles, float trailLength) : base(rotateSpeed, orbitSpeed, model, trailModel, trailTexture, trailParticleScale, minDistanceBetweenTrailParticles, trailLength) {}

    public override void Rotate()
    {
        planetModel.Rotation = new Quaternion(0f, -(float)Math.Sin(MathHelper.ToRadians(rotateSpeed / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(rotateSpeed / 2))) * planetModel.Rotation;
        planetModel.UpdateWorldMatrix();
    }
}



//saturn child class for local axis rotation
class Saturn : Planet
{
    public Saturn(float rotateSpeed, float orbitSpeed, Model3D model,  Model trailModel, Texture2D trailTexture, float trailParticleScale, float minDistanceBetweenTrailParticles, float trailLength) : base(rotateSpeed, orbitSpeed, model, trailModel, trailTexture, trailParticleScale, minDistanceBetweenTrailParticles, trailLength)
    {
        planetModel.Rotation *= new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(20)), 0f, (float)Math.Cos(MathHelper.ToRadians(20))); //giving saturn its tilt
    }

    public override void Rotate()
    {
        planetModel.Rotation *= new Quaternion(0f, 0f, (float)Math.Sin(MathHelper.ToRadians(rotateSpeed / 2)), (float)Math.Cos(MathHelper.ToRadians(rotateSpeed / 2)));
        planetModel.UpdateWorldMatrix();
    }
}



//moon child class for orbiting a planet
class Moon : Planet
{
    float orbitAngle = 0f;
    Vector3 relativeMoonPos;
    public  Moon(float rotateSpeed, float orbitSpeed, Model3D model, Model trailModel, Texture2D trailTexture, float trailParticleScale, float minDistanceBetweenTrailParticles, float trailLength, Vector3 relativeMoonPos) : base(rotateSpeed, orbitSpeed, model, trailModel, trailTexture, trailParticleScale, minDistanceBetweenTrailParticles, trailLength)
    {
        this.relativeMoonPos = relativeMoonPos;
    }

    public void OrbitPlanet(Planet planet)
    {
        orbitAngle += MathHelper.ToRadians(orbitSpeed);
        planetModel.Position = Vector3.Transform(relativeMoonPos, new Quaternion(0f, (float)Math.Sin(MathHelper.ToRadians(orbitAngle / 2)), 0f, (float)Math.Cos(MathHelper.ToRadians(orbitAngle / 2)))) + planet.PlanetModel.Position;
        planetModel.UpdateWorldMatrix();
    }
}