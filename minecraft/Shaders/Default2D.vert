#version 330 core

layout (location = 0) in vec2 aPos;       // Position 2D sur l'écran [-1,1]
layout (location = 1) in vec2 aTexCoord; // UV de la texture

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPos, 0.0, 1.0); // Z=0 pour overlay 2D
    TexCoord = aTexCoord;
}
