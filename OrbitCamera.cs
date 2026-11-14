using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

/// <summary>Cameră orbitală: se rotește în jurul unei ținte, păstrând privirea spre ea.</summary>
public class OrbitCamera : Camera
{
    public Vector3 Target { get; private set; } = new(0, 1, -2);
    public float Distance { get; private set; } = 8f;
    public float Azimuth { get; private set; } = 90f;
    public float Elevation { get; private set; } = 15f;
    public float MouseSensitivity = 0.2f;
    public float RotationSpeed = 75f;
    public float ZoomSpeed = 0.8f;
    public override string Name => "Orbită";

    public override Vector3 Position
    {
        get
        {
            float a = MathHelper.DegreesToRadians(Azimuth);
            float e = MathHelper.DegreesToRadians(Elevation);
            return Target + Distance * new Vector3(MathF.Cos(e) * MathF.Cos(a),
                MathF.Sin(e), MathF.Cos(e) * MathF.Sin(a));
        }
    }
    public override Vector3 Front => Vector3.Normalize(Target - Position);
    public override Matrix4 ViewMatrix => Matrix4.LookAt(Position, Target, Vector3.UnitY);

    public void SetFromFps(FpsCamera fps, float distance = 5f)
    {
        Distance = distance;
        Target = fps.Position + fps.Front * distance;
        Vector3 outward = -fps.Front;
        Elevation = Math.Clamp(MathHelper.RadiansToDegrees(MathF.Asin(outward.Y)), -89f, 89f);
        Azimuth = MathHelper.RadiansToDegrees(MathF.Atan2(outward.Z, outward.X));
        FieldOfView = fps.FieldOfView;
    }

    public override void Update(KeyboardState keyboard, MouseState mouse, Vector2 mouseDelta, float dt)
    {
        if (mouse.IsButtonDown(MouseButton.Left))
        {
            Azimuth -= mouseDelta.X * MouseSensitivity;
            Elevation += mouseDelta.Y * MouseSensitivity;
        }
        if (keyboard.IsKeyDown(Keys.Left)) Azimuth += RotationSpeed * dt;
        if (keyboard.IsKeyDown(Keys.Right)) Azimuth -= RotationSpeed * dt;
        if (keyboard.IsKeyDown(Keys.Up)) Elevation += RotationSpeed * dt;
        if (keyboard.IsKeyDown(Keys.Down)) Elevation -= RotationSpeed * dt;
        Elevation = Math.Clamp(Elevation, -89f, 89f);
        Distance = Math.Clamp(Distance - mouse.ScrollDelta.Y * ZoomSpeed, 1f, 60f);

        // Bonus: deplasarea țintei pe orizontală cu WASD.
        Vector3 forward = Front;
        forward.Y = 0;
        forward = Vector3.Normalize(forward);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        Vector3 move = Vector3.Zero;
        if (keyboard.IsKeyDown(Keys.W)) move += forward;
        if (keyboard.IsKeyDown(Keys.S)) move -= forward;
        if (keyboard.IsKeyDown(Keys.D)) move += right;
        if (keyboard.IsKeyDown(Keys.A)) move -= right;
        if (move.LengthSquared > 0) Target += Vector3.Normalize(move) * 3f * dt;
    }

    public override void Reset()
    {
        Target = new Vector3(0, 1, -2);
        Distance = 8f;
        Azimuth = 90f;
        Elevation = 15f;
        FieldOfView = 60f;
    }

    public override IEnumerable<string> Describe()
    {
        Vector3 p = Position;
        yield return $"Țintă          : ({Target.X:F2}, {Target.Y:F2}, {Target.Z:F2})";
        yield return $"Distanță       : {Distance:F2}";
        yield return $"Azimut         : {Azimuth:F1}°";
        yield return $"Elevație       : {Elevation:F1}°";
        yield return $"Poziție        : ({p.X:F2}, {p.Y:F2}, {p.Z:F2})";
    }
}
