#version 440 core

uniform sampler2D maintexture;

in vec2 frag_uv;

out vec4 out_color;

void main() {
	out_color = texture(maintexture, frag_uv);
}