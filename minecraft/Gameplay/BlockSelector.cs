using OpenTK.Mathematics;
using minecraft.worldgen;
using OpenTK.Graphics.OpenGL4;

namespace minecraft.Gameplay
{
    public class BlockSelector
    {
        private World world;
        private Camera camera;
        private BlockOutlineRenderer outlineRenderer;
        private int shader;

        public Vector3 HitBlock { get; private set; }
        public Vector3Int FaceNormal { get; private set; }
        public bool HasHit { get; private set; }

        private const float MAX_DISTANCE = 5f;

        public BlockSelector(World world, Camera cam, int outlineShader)
        {
            this.world = world;
            camera = cam;
            shader = outlineShader;

            outlineRenderer = new BlockOutlineRenderer();
            outlineRenderer.Init(shader);
        }

        public void Update(Matrix4 view, Matrix4 projection)
        {
            // Raycast DDA centré sur la caméra
            HasHit = VoxelRaycast.Raycast(world, camera.Position, camera.Front, MAX_DISTANCE,
                                          out Vector3 hit, out Vector3Int normal);

            if (HasHit)
            {
                HitBlock = hit;
                FaceNormal = normal;

                // Dessine outline noir, épaissi
                outlineRenderer.Draw(view, projection, hit, lineWidth: 3f, color: new Vector3(0f, 0f, 0f));
            }
        }
    }
}
