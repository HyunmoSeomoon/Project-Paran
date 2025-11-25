using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	/// <summary>
	/// 2019-10-24: updated viseme (oo) bone rotation.
	/// 			updated for Izzy model.
	/// 2019-06-29: updated viseme (oo).
	/// </summary>
	public class OneClickIClone : OneClickBase
	{
		/// <summary>
		/// Setup and run OneClick operation on the supplied GameObject.
		/// </summary>
		/// <param name="gameObject">Root OneClick GameObject.</param>
		/// <param name="clip">AudioClip (can be null).</param>
		public static void Setup(GameObject gameObject)
		{
			////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//	SETUP Requirements:
			//		use NewViseme("viseme name") to start a new viseme.
			//		use AddShapeComponent to add blendshape configurations, passing:
			//			- string array of shape names to look for.
			//			- optional string name prefix for the component.
			//			- optional blend amount (default = 1.0f).

			Init();

			#region SALSA-Configuration
			NewConfiguration(OneClickConfiguration.ConfigType.Salsa);
			{
				////////////////////////////////////////////////////////
				////////////////////////////////////////////////////////
				// SMR regex searches (enable/disable/add as required).
				AddSmrSearch("^.*Body$");
				AddSmrSearch("^.*Tongue$");

				////////////////////////////////////////////////////////
				// Adjust SALSA settings to taste...
				// - data analysis settings
				autoAdjustAnalysis = true;
				autoAdjustMicrophone = false;
				audioUpdateDelay = 0.0875f;
				// - advanced dynamics settings
				loCutoff = 0.015f;
				hiCutoff = 0.75f;
				useAdvDyn = true;
				advDynPrimaryBias = 0.50f;
				useAdvDynJitter = true;
				advDynJitterAmount = 0.10f;
				advDynJitterProb = 0.20f;
				advDynSecondaryMix = 0.0f;
				emphasizerTrigger = 0.0f;


				////////////////////////////////////////////////////////
				// Viseme setup...

				NewExpression("w");
				AddBoneComponent("^.*Teeth02$",
								 new TformBase(new Vector3(-0.0865365f, 1.162291E-06f, 4.219168E-07f),
											   new Quaternion(0.9971721f, 0.07515167f, 3.431923E-07f, 2.961605E-07f),
											   new Vector3(1f, 1f, 1f)),
								 0.11f, 0f, 0.06f,
								 "RL_G6_Teeth02",
								 false, true, false);
				AddShapeComponent(new[] {"^Lips_Open.*$"}, 0.11f, 0f, 0.06f, "Lips_Open", 0.20f, true);
				AddShapeComponent(new[] {"^Tight-O.*$"}, 0.11f, 0f, 0.06f, "tight-o", 0.536f, true);


				NewExpression("t");
				AddBoneComponent("^.*Teeth02$",
								 new TformBase(new Vector3(-0.0865365f, 1.162291E-06f, 4.219168E-07f),
											   new Quaternion(0.9971721f, 0.07515167f, 3.431923E-07f, 2.961605E-07f),
											   new Vector3(1f, 1f, 1f)),
								 0.11f, 0f, 0.06f,
								 "RL_G6_Teeth02",
								 false, true, false);
				AddShapeComponent(new[] {"^Tongue_Curl-U.*$"}, 0.11f, 0f, 0.06f, "Tongue_Curl-U", 1f, true);
				AddShapeComponent(new[] {"^Lip Open.*$"}, 0.11f, 0f, 0.06f, "Lip_Open", 1f, true);
				AddShapeComponent(new[] {"^Tight-O.*$"}, 0.11f, 0f, 0.06f, "tight-o", 0.10f, true);


				NewExpression("f");
				AddBoneComponent("^.*Teeth02$",
								 new TformBase(new Vector3(-0.0865365f, 1.162291E-06f, 4.219168E-07f),
											   new Quaternion(0.9971721f, 0.07515167f, 3.431923E-07f, 2.961605E-07f),
											   new Vector3(1f, 1f, 1f)),
								 0.11f, 0f, 0.06f,
								 "RL_G6_Teeth02",
								 false, true, false);
				AddShapeComponent(new[] {"^Tongue_Curl-U.*$"}, 0.11f, 0f, 0.06f, "Tongue_Curl-U", 1f, true);
				AddShapeComponent(new[] {"^Lips_Open.*$"}, 0.11f, 0f, 0.06f, "Lips_Open", 0.406f, true);
				AddShapeComponent(new[] {"^Lips_Tuck.*$"}, 0.11f, 0f, 0.06f, "Lips_Tuck", 0.50f, true);


				NewExpression("th");
				AddBoneComponent("^.*Teeth02$",
								 new TformBase(new Vector3(-0.0865365f, 1.162291E-06f, 4.219168E-07f),
											   new Quaternion(0.9971721f, 0.07515167f, 3.431923E-07f, 2.961605E-07f),
											   new Vector3(1f, 1f, 1f)),
								 0.11f, 0f, 0.06f,
								 "RL_G6_Teeth02",
								 false, true, false);
				AddShapeComponent(new[] {"^Tongue_Raise.*$"}, 0.11f, 0f, 0.06f, "Tongue_Raise", 0.223f, true);
				AddShapeComponent(new[] {"^Lips_Open.*$"}, 0.11f, 0f, 0.06f, "Mouth_Lips_Open", 0.319f, true);
				AddShapeComponent(new[] {"^Affricate.*$"}, 0.11f, 0f, 0.06f, "Affricate", 0.21f, true);
				AddShapeComponent(new[] {"^Lips_Puckered_Open.*$"}, 0.11f, 0f, 0.06f, "Pucker_Open", 0.355f, true);
				AddShapeComponent(new[] {"^Lip_Raise_Top.*$"}, 0.11f, 0f, 0.06f, "Raise_Top", 0.3f, true);


				NewExpression("ow");
				AddBoneComponent("^.*Teeth02$",
								 new TformBase(new Vector3(-0.0865365f, 1.162291E-06f, 4.219168E-07f),
											   new Quaternion(0.9971721f, 0.07515167f, 3.431923E-07f, 2.961605E-07f),
											   new Vector3(1f, 1f, 1f)),
								 0.11f, 0f, 0.06f,
								 "RL_G6_Teeth02",
								 false, true, false);
				AddShapeComponent(new[] {"^Tongue_Narrow.*$"}, 0.11f, 0f, 0.06f, "Tongue_Narrow", 0.776f, true);
				AddShapeComponent(new[] {"^Tongue_Curl-U.*$"}, 0.11f, 0f, 0.06f, "Tongue_Curl-U", 0.20f, true);
				AddShapeComponent(new[] {"^Tongue_Lower.*$"}, 0.11f, 0f, 0.06f, "Tongue_Lower", 1f, true);
				AddShapeComponent(new[] {"^Lips_Open.*$"}, 0.11f, 0f, 0.06f, "Mouth_Lips_Open", 0.8f, true);
				AddShapeComponent(new[] {"^Tight-O.*$"}, 0.11f, 0f, 0.06f, "Tight-O", 0.65f, true);


				NewExpression("ee");
				AddBoneComponent("^.*Teeth02$",
								 new TformBase(new Vector3(-0.0865365f, 1.162291E-06f, 4.219168E-07f),
											   new Quaternion(0.991293f, 0.1316742f, 5.420296E-07f, 3.338791E-07f),
											   new Vector3(1f, 1f, 1f)),
								 0.11f, 0f, 0.06f,
								 "RL_G6_Teeth02",
								 false, true, false);
				AddShapeComponent(new[] {"^Lips_Open.*$"}, 0.11f, 0f, 0.06f, "Mouth_Lips_Open", 0.664f, true);
				AddShapeComponent(new[] {"^Lips_Widen.*$"}, 0.11f, 0f, 0.06f, "Mouth_Widen", 0.697f, true);
				AddShapeComponent(new[] {"^Lip Open.*$"}, 0.11f, 0f, 0.06f, "Lip_Open", 0.165f, true);


				NewExpression("oo");
				AddBoneComponent("^.*Teeth02$",
								 new TformBase(new Vector3(-0.0865365f, 1.162291E-06f, 4.219168E-07f),
											   new Quaternion(0.9985089f, 0.05458893f, 1.87545e-07f, 5.082691e-07f),
											   new Vector3(1f, 1f, 1f)),
								 0.11f, 0f, 0.06f,
								 "RL_G6_Teeth02",
								 false, true, false);
				AddShapeComponent(new[] {"^Tongue_Curl-U.*$"}, 0.11f, 0f, 0.06f, "Tongue_Curl-U", 0.20f, true);
				AddShapeComponent(new[] {"^Tongue_Lower.*$"}, 0.11f, 0f, 0.06f, "Tongue_Lower", 1f, true);
				AddShapeComponent(new[] {"^Tongue_Narrow.*$"}, 0.11f, 0f, 0.06f, "Tongue_Curl-U", 0.55f, true);
				AddShapeComponent(new[] {"^Lips_Open.*$"}, 0.11f, 0f, 0.06f, "Mouth_Lips_Open", 1f, true);
				AddShapeComponent(new[] {"^Tight.*$"}, 0.11f, 0f, 0.06f, "Tight", 0.6f, true);
				AddShapeComponent(new[] {"^Lip Open.*$"}, 0.11f, 0f, 0.06f, "Lip Open", 1f, true);

			}
			#endregion // SALSA-configuration

			#region EmoteR-Configuration
			NewConfiguration(OneClickConfiguration.ConfigType.Emoter);
			{
				////////////////////////////////////////////////////////
				// SMR regex searches (enable/disable/add as required).
				AddSmrSearch("^.*Body$");

				useRandomEmotes = false;
				isChancePerEmote = true;
				numRandomEmotesPerCycle = 0;
				randomEmoteMinTimer = 1f;
				randomEmoteMaxTimer = 2f;
				randomChance = 0.5f;
				useRandomFrac = false;
				randomFracBias = 0.5f;
				useRandomHoldDuration = false;
				randomHoldDurationMin = 0.1f;
				randomHoldDurationMax = 0.5f;


				NewExpression("exasper");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"^Cheek_Raise_L.*$"}, 0.2f, 0.1f, 0.15f, "cheek_L", 0.45f, true);
				AddShapeComponent(new[] {"^Cheek_Raise_R.*$"}, 0.2f, 0.1f, 0.15f, "cheek_R", 0.45f, true);
				AddShapeComponent(new[] {"^Brow_Raise_Inner_L.*$"}, 0.2f, 0.1f, 0.15f, "browUpInnerL", 0.45f, true);
				AddShapeComponent(new[] {"^Brow_Raise_Inner_R.*$"}, 0.2f, 0.1f, 0.15f, "browUpInnerR", 0.45f, true);


				NewExpression("soften");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"^Lips_Smirk.*$"}, 0.2f, 0.1f, 0.15f, "smile", 0.4f, true);
				AddShapeComponent(new[] {"^Eyes_Squint.*$"}, 0.2f, 0.1f, 0.15f, "squint", 0.45f, true);
				AddShapeComponent(new[] {"^Brow_Raise_Inner_L.*$"}, 0.2f, 0.1f, 0.15f, "browUpInnerL", 0.45f, true);
				AddShapeComponent(new[] {"^Brow_Raise_Inner_R.*$"}, 0.2f, 0.1f, 0.15f, "browUpInnerR", 0.45f, true);


				NewExpression("browsUp");
				AddEmoteFlags(false, true, false, 1f, true);
				AddShapeComponent(new[] {"^Brow_Raise_L.*$"}, 0.2f, 0.1f, 0.15f, "Brow_Raise_L", 0.252f, true);
				AddShapeComponent(new[] {"^Brow_Raise_R.*$"}, 0.25f, 0f, 0.1f, "Brow_Raise_R", 0.399f, true);


				NewExpression("browUp");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"^Brow_Raise_R.*$"}, 0.2f, 0.1f, 0.15f, "Brow_Raise_R", 0.375f, true);


				NewExpression("squint");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"^Brow_Drop_L.*$"}, 0.2f, 0.1f, 0.15f, "Brow_Drop_L", 0.45f, true);
				AddShapeComponent(new[] {"^Brow_Drop_R.*$"}, 0.2f, 0.1f, 0.15f, "Brow_Drop_R", 0.45f, true);
				AddShapeComponent(new[] {"^Eyes_Squint.*$"}, 0.2f, 0.1f, 0.15f, "Eye_Squint", 0.45f, true);


				NewExpression("focus");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"^Nose_Flank_Raise_L.*$"}, 0.2f, 0.1f, 0.15f, "noseflank_L", 0.40f, true);
				AddShapeComponent(new[] {"^Nose_Flank_Raise_R.*$"}, 0.2f, 0.05f, 0.1f, "noseflank_R", 0.50f, true);
				AddShapeComponent(new[] {"^Eyes_Squint.*$"}, 0.2f, 0.1f, 0.15f, "Eye_Squint", 0.35f, true);


				NewExpression("flare");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"^Nose_Flank_Raise.*$"}, 0.2f, 0.1f, 0.15f, "noseflank", 0.45f, true);


				NewExpression("scrunch");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"^Nose_Scrunch.*$"}, 0.2f, 0.1f, 0.15f, "nose_scrunch", 0.65f, true);

			}
			#endregion // EmoteR-configuration

			DoOneClickiness(gameObject);

			if (selectedObject.GetComponent<SalsaAdvancedDynamicsSilenceAnalyzer>() == null)
				selectedObject.AddComponent<SalsaAdvancedDynamicsSilenceAnalyzer>();
		}
	}
}