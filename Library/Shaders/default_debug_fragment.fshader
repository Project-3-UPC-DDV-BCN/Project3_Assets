              shader_text       c       #version 330 core
out vec4 color;

uniform vec4 debug_color;
void main()
{
		color = debug_color;
}