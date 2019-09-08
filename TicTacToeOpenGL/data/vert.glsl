#version 440 core

uniform mat4 projection;

uniform vec2 offset;

in vec2 pos;
in vec2 uv;

out vec2 frag_uv;

void main() {
	frag_uv = uv;
	gl_Position = projection * vec4(offset + pos, 0.0, 1.0);
}