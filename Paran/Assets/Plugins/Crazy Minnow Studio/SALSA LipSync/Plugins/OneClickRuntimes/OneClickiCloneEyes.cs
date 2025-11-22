using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	public class OneClickiCloneEyes : MonoBehaviour
	{
		public static void Setup(GameObject go)
		{
			string head = "head$";
			string body = "body$";
			string eyeL = "l_eye$";
			string eyeR = "r_eye$";
			string blinkL = "blink_l";
			string blinkR = "blink_r";

			if (go)
			{
				Eyes eyes = go.GetComponent<Eyes>();
				if (eyes == null)
				{
					eyes = go.AddComponent<Eyes>();
				}
				else
				{
					DestroyImmediate(eyes);
					eyes = go.AddComponent<Eyes>();
				}

				// System Properties
                eyes.characterRoot = go.transform;

                // Heads - Bone_Rotation
                eyes.BuildHeadTemplate(Eyes.HeadTemplates.Bone_Rotation_XY);
                eyes.heads[0].expData.controllerVars[0].bone = Eyes.FindTransform(eyes.characterRoot, head);
                eyes.headTargetOffset.y = 0.065f;
                eyes.CaptureMin(ref eyes.heads);
                eyes.CaptureMax(ref eyes.heads);

                // Eyes - Bone_Rotation
                eyes.BuildEyeTemplate(Eyes.EyeTemplates.Bone_Rotation);
                eyes.eyes[0].expData.controllerVars[0].bone = Eyes.FindTransform(eyes.characterRoot, eyeL);
                eyes.eyes[1].expData.controllerVars[0].bone = Eyes.FindTransform(eyes.characterRoot, eyeR);
                eyes.CaptureMin(ref eyes.eyes);
                eyes.CaptureMax(ref eyes.eyes);

                // Eyelids - Bone_Rotation
                eyes.BuildEyelidTemplate(Eyes.EyelidTemplates.BlendShapes, Eyes.EyelidSelection.Upper); // includes left/right eyelid
                float blinkMax = 1f;
                // Left eyelid
                eyes.blinklids[0].expData.controllerVars[0].smr = Eyes.FindTransform(eyes.characterRoot,  body).GetComponent<SkinnedMeshRenderer>();
                eyes.blinklids[0].expData.controllerVars[0].blendIndex = Eyes.FindBlendIndex(eyes.blinklids[0].expData.controllerVars[0].smr, blinkL);
                eyes.blinklids[0].expData.controllerVars[0].maxShape = blinkMax;
                eyes.blinklids[0].expData.name = "eyelidL";
                // Right eyelid
                eyes.blinklids[1].expData.controllerVars[0].smr = eyes.blinklids[0].expData.controllerVars[0].smr;
                eyes.blinklids[1].expData.controllerVars[0].blendIndex = Eyes.FindBlendIndex(eyes.blinklids[0].expData.controllerVars[0].smr, blinkR);
                eyes.blinklids[1].expData.controllerVars[0].maxShape = blinkMax;
                eyes.blinklids[1].expData.name = "eyelidR";
                
                // Tracklids
                eyes.CopyBlinkToTrack();
                eyes.tracklids[0].referenceIdx = 0; // left eye
                eyes.tracklids[1].referenceIdx = 1; // right eye

                // Initialize the Eyes module
                eyes.Initialize();
			}
		}
	}
}