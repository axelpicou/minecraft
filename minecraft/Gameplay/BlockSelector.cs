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

        public Vector3Int HitVoxel { get; private set; }
        public Vector3 HitBlockCenter { get; private set; }
        public Vector3Int FaceNormal { get; private set; }
        public bool HasHit { get; private set; }

        private const float MAX_DISTANCE = 4f;

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
            HasHit = VoxelRaycast.Raycast(
                world,
                camera.Position,
                camera.Front,
                MAX_DISTANCE,
                out Vector3 hitBlock,
                out Vector3Int normal
            );

            if (!HasHit)
                return;

            HitVoxel = new Vector3Int(
                (int)hitBlock.X,
                (int)hitBlock.Y,
                (int)hitBlock.Z
            );

            // ✅ Le centre du bloc est maintenant à (X+0.5, Y+0.5, Z+0.5)
            HitBlockCenter = new Vector3(
                hitBlock.X + 0.5f,
                hitBlock.Y + 0.5f,
                hitBlock.Z + 0.5f
            );

            FaceNormal = normal;

            outlineRenderer.Draw(
                view,
                projection,
                HitBlockCenter,
                lineWidth: 3f,
                color: new Vector3(0f, 0f, 0f)
            );
        }
    }
}