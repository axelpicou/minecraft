#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aColor; 

out vec2 TexCoord;
out vec3 Color;
out float SkyFactor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    TexCoord = aTexCoord;
    Color = aColor;

    SkyFactor = clamp(aPosition.y / 256.0, 0.3, 1.0);
}
