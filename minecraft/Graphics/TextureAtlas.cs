using System.IO;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace minecraft.Graphics
{
    public class TextureAtlas
    {
        public int TextureID { get; private set; }
        public int TileSize { get; private set; }
        public int AtlasSize { get; private set; }

        public TextureAtlas(string path, int tileSize)
        {
            TileSize = tileSize;

            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            StbImage.stbi_set_flip_vertically_on_load(1);
            using var stream = File.OpenRead(path);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            AtlasSize = image.Width;

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public float GetUV(int tileX) =>
            (float)(tileX * TileSize) / AtlasSize;
    }
}
