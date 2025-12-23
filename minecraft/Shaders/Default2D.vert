#version 330 core

void main()
{
    vec2 positions[4] = vec2[](
        vec2(-0.02,  0.02),
        vec2( 0.02,  0.02),
        vec2( 0.02, -0.02),
        vec2(-0.02, -0.02)
    );

    gl_Position = vec4(positions[gl_VertexID], 0.0, 1.0);
}
