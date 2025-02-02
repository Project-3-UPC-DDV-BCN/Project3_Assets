              shader_text             #version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoords;
out vec2 TexCoords;
uniform mat4 Model;
uniform mat4 view;
uniform mat4 projection;
void main()
{
	TexCoords = aTexCoords.xy;
	gl_Position = projection * view * Model * vec4(aPos, 1.0f);
}
