#version 330 core

in vec2 TexCoord;
in vec3 Color;
in float SkyFactor;

out vec4 FragColor;

uniform sampler2D ourTexture;

void main()
{
    vec4 texColor = texture(ourTexture, TexCoord);


    vec3 ambient = vec3(0.15);


    vec3 skyLight = vec3(0.25) * SkyFactor;

    vec3 lighting = max(Color + skyLight, ambient);

    FragColor = texColor * vec4(lighting, 1.0);
}
