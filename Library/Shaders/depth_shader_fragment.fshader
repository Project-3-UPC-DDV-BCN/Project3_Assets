              shader_text       D       	#version 330 core
	void main()
{
	gl_FragDepth = gl_FragCoord.z;
}
