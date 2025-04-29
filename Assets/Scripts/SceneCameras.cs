using UnityEngine;

public abstract class SceneCamera
{
    public Vector3 position, forward, up;

    public abstract void Update(bool left, bool right, bool forward, bool backward);

    virtual public Matrix4x4 ComputeViewMatrix()
    {
        // Vector3 forward = (lookAt - position).normalized;
        Vector3 right = -Vector3.Cross(forward, up).normalized;
        Matrix4x4 ViewMatrix = new Matrix4x4(
            new Vector4(right.x, right.y, right.z, -Vector3.Dot(right, position)),
            new Vector4(up.x, up.y, up.z, -Vector3.Dot(up, position)),
            new Vector4(-forward.x, -forward.y, -forward.z, Vector3.Dot(forward, position)),
            new Vector4(0, 0, 0, 1)
        );

        return ViewMatrix.transpose;
    }

    public Matrix4x4 ComputeProjectionMatrix(float sceneFOV)
    {
        float trueAspectRatio = (Screen.width > Screen.height)
                                ? (Screen.width / Screen.height)
                                : (Screen.height / Screen.width);
        float nearClipPlane = 0.1f, farClipPlane = 1000.0f;
        Matrix4x4 ProjectionMatrix = CalculatePerspectiveProjectionMatrix(sceneFOV, trueAspectRatio, nearClipPlane, farClipPlane);
        return ProjectionMatrix;
    }

    /*
    private Matrix4x4 CalculatePerspectiveProjectionMatrixUnity(float fov, float aspectRatio, float near, float far)
    {
        return Matrix4x4.Perspective(fov, aspectRatio, near, far);
    }
    */

    private Matrix4x4 CalculatePerspectiveProjectionMatrix(float fov, float aspectRatio, float near, float far)
    {
        /*
         * https://www.scratchapixel.com/lessons/3d-basic-rendering/perspective-and-orthographic-projection-matrix/building-basic-perspective-projection-matrix.html
         * https://www.scratchapixel.com/lessons/3d-basic-rendering/perspective-and-orthographic-projection-matrix/opengl-perspective-projection-matrix.html
         * https://stackoverflow.com/questions/46008171/transform-the-modelmatrix/46008573#46008573
         * https://www.youtube.com/watch?v=EqNcqBdrNyI
         * https://www.songho.ca/opengl/gl_projectionmatrix.html
         * https://www.ogldev.org/www/tutorial12/tutorial12.html
         * https://learn.microsoft.com/en-us/windows/win32/direct3d9/projection-transform
         * https://www.youtube.com/watch?v=U0_ONQQ5ZNM
         */
        float n = near;
        float scale = Mathf.Tan((fov / 2.0f) * Mathf.PI / 180.0f) * n;
        float r = aspectRatio * scale;
        float l = -r;
        float t = scale;
        float b = -t;
        float f = far;
        Matrix4x4 OpenGLProjectionMatrix = new Matrix4x4(
            new Vector4(((2 * n) / (r - l)), 0, (r + l) / (r - l), 0),
            new Vector4(0, ((2 * n) / (t - b)), (t + b) / (t - b), 0),
            new Vector4(0, 0, -(f + n) / (f - n), (-(2 * f * n) / (f - n))),
            new Vector4(0, 0, -1, 0)
        );
        return OpenGLProjectionMatrix.transpose;
    }
}

public class OrbitalCamera : SceneCamera
{
    // we store lookAt, distance, and forward
    public float distanceFromTarget;
    public OrbitalCamera(Vector3 position, Vector3 target, Vector3 up)
    {
        this.distanceFromTarget = (position - target).magnitude;
        Debug.Log("[ORBITAL CAM::Constructor/3] distanceFromTarget = " + distanceFromTarget);
        this.forward = (position - target).normalized; // not (target - position). the point is to have a vector point *from* target out somewhere into the conceptual sphere
        this.position = target; // store the lookAt point in position, because that will be the center of the sphere
        this.up = up; // -up for D3D11?
    }

    public float horizontalRotateSpeed = 90.0f; // "degrees per second"
    public float verticalRotateSpeed = 90.0f; // "degrees per second"

    private enum RotationDirection
    {
        LEFT = 1, RIGHT = -1,
        UP = 1, DOWN = -1,
        CLOCKWISE = 1, COUNTERCLOCKWISE = -1,
    };
    public override void Update(bool left, bool right, bool forward, bool backward)
    {
        if (left) { RotateLeftRight(RotationDirection.LEFT); }
        if (right) { RotateLeftRight(RotationDirection.RIGHT); }
        if (forward) { RotateUpDown(RotationDirection.UP); }
        if (backward) { RotateUpDown(RotationDirection.DOWN); }

        // Extra:
        if (Input.GetKey(KeyCode.O)) { distanceFromTarget -= 2.0f * Time.deltaTime; }
        if (Input.GetKey(KeyCode.L)) { distanceFromTarget += 2.0f * Time.deltaTime; }
        distanceFromTarget = Mathf.Clamp(distanceFromTarget, 0.0001f, 15.0f);

        if (Input.GetKey(KeyCode.Q)) { Roll(RotationDirection.CLOCKWISE); }
        if (Input.GetKey(KeyCode.E)) { Roll(RotationDirection.COUNTERCLOCKWISE); }

        if (Input.GetKey(KeyCode.R))
        {
            // Debug.LogWarning("[ORBITAL CAM] RESET");
            this.position = new Vector3(0, 1, 0);
            this.up = Vector3.up;
            Vector3 cameraPos = new Vector3(0, 1, 3);
            this.distanceFromTarget = 3;
            this.forward = (cameraPos - position).normalized;
        }
    }

    private void Roll(RotationDirection dir)
    {
        // rotate up and right around forward (but right isn't stored)
        float rotationAngleInRadians = ((int)dir) * Mathf.Deg2Rad * 90.0f * Time.deltaTime;
        Vector3 rotationAxis = forward;
        Vector3 up_rot = rotateAround(this.up, rotationAngleInRadians, rotationAxis);
        this.up = up_rot;
    }

    // Pitch
    private void RotateUpDown(RotationDirection dir)
    {
        // rotate forward and up around right
        float rotationAngleInRadians = ((int)dir) * Mathf.Deg2Rad * verticalRotateSpeed * Time.deltaTime;
        Vector3 rotationAxis = Vector3.Cross(forward, up).normalized;

        Vector3 forward_rot = rotateAround(this.forward, rotationAngleInRadians, rotationAxis);
        Vector3 up_rot = rotateAround(this.up, rotationAngleInRadians, rotationAxis);
        this.forward = forward_rot;
        // this.up = -(Vector3.Cross(forward_rot, rotationAxis)).normalized;
        this.up = up_rot;
    }

    // Yaw
    private void RotateLeftRight(RotationDirection dir)
    {
        // rotate forward and right around up (but right isn't stored)
        float rotationAngleInRadians = ((int)dir) * Mathf.Deg2Rad * verticalRotateSpeed * Time.deltaTime;
        Vector3 forward_rot = rotateAround(this.forward, rotationAngleInRadians, this.up);
        this.forward = forward_rot;
    }

    private Vector3 rotateAround(Vector3 V, float angleInRadians, Vector3 rotationAxis)
    {
        Vector3 K = rotationAxis;
        // https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
        Vector3 rotatedV = V * Mathf.Cos(angleInRadians)
                     + (Vector3.Cross(K, V) * Mathf.Sin(angleInRadians))
                     + K * (Vector3.Dot(K, V) * (1 - Mathf.Cos(angleInRadians)));
        return rotatedV;
    }

    // When it comes to ComputeViewMatrix, things need to be turned around
    // so as to look at things from the camera's POV
    public Vector3 cameraPosition => position + distanceFromTarget * forward;
    override public Matrix4x4 ComputeViewMatrix()
    {
        Vector3 pos = cameraPosition;
        Vector3 orbitalForward = (this.position - pos).normalized; // lookAt - position
        Vector3 left = Vector3.Cross(orbitalForward, up).normalized;
        Vector3 right = -left;

        Matrix4x4 ViewMatrix = new Matrix4x4(
            new Vector4(right.x, right.y, right.z, -Vector3.Dot(right, pos)),
            new Vector4(up.x, up.y, up.z, -Vector3.Dot(up, pos)),
            new Vector4(-orbitalForward.x, -orbitalForward.y, -orbitalForward.z, Vector3.Dot(orbitalForward, pos)),
            new Vector4(0, 0, 0, 1)
        );
        return ViewMatrix.transpose;

        //Vector3 unityFrom = position + distanceFromTarget * forward;
        //Vector3 unityTo = unityFrom + distanceFromTarget * forward;
        //Vector3 unityUp = Vector3.Cross(unityTo - unityFrom, right);
        //Matrix4x4 UnityViewMatrix = Matrix4x4.LookAt(unityFrom, unityTo, unityUp);
        //return UnityViewMatrix;
    }
}

public class FirstPersonCamera : SceneCamera
{
    public float horizontalAngle;
    public float cameraForwardSpeed = 2; // "units per second"
    public FirstPersonCamera(Vector3 pos, Vector3 lookAt, Vector3 up)
    {
        this.horizontalAngle = 0.0f;
        this.position = pos;
        this.forward = (lookAt - position).normalized;
        this.up = up;
    }

    public float horizontalRotateSpeed = 140.0f; // "degrees per second"
    public enum RotationDirection { LEFT = 1, RIGHT = -1 };
    public void RotateForward(RotationDirection dir)
    {
        float rotationAngleInRadians = ((int)dir) * Mathf.Deg2Rad * horizontalRotateSpeed * Time.deltaTime;
        Matrix4x4 rotationMatrixY = new Matrix4x4(
            new Vector4(Mathf.Cos(rotationAngleInRadians), 0.0f, Mathf.Sin(rotationAngleInRadians), 0.0f),
            new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            new Vector4(-Mathf.Sin(rotationAngleInRadians), 0.0f, Mathf.Cos(rotationAngleInRadians), 0.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
        );
        this.forward = rotationMatrixY * this.forward;
    }

    public override void Update(bool left, bool right, bool forward, bool backward)
    {
        // probably should be (left && !right), (right && !left), (forward && !backward), (backward && !forward)
        // just so matrices don't have to be recomputed twice for no result
        if (left)
        {
            this.RotateForward(FirstPersonCamera.RotationDirection.LEFT);
        }

        if (right)
        {
            this.RotateForward(FirstPersonCamera.RotationDirection.RIGHT);
        }

        if (forward)
        {
            this.position = this.position + (this.cameraForwardSpeed * Time.deltaTime * this.forward);
        }

        if (backward)
        {
            this.position = this.position + (-this.cameraForwardSpeed * Time.deltaTime * this.forward);
        }
    }
}
