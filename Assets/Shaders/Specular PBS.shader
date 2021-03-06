﻿Shader "Custom/Specular PBS" {
    Properties {
        _Color("Albedo", Color)           = (1,1,1,1)
		_SpecularTint ("Specular", Color) = (0.5, 0.5, 0.5)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader {
        Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM

			#pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

			#include "MyLighting.cginc"

            ENDCG
        }

        Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

            CGPROGRAM

			#pragma target 3.0

            #pragma multi_compile_fwdadd_fullshadows

            #pragma vertex vert
            #pragma fragment frag

			#include "MyLighting.cginc"
            
            ENDCG
        }

		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0
			#pragma multi_compile_shadowcaster

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#include "MyShadows.cginc"

			ENDCG
		}
    }
}