# Marble and Granite Substances & Materials Docs

 **PolyLabs | version 2.0.0** | support@polylabs.co

## Package Overview

This package contains Substance3D materials *and* Unity materials. Substances can be used in Unity by installing the **Substance3D for Unity** package on the Unity Asset Store.

This documentation is also a reference guide for modifying the Susbtances via script, with the parameters and their ranges.

> Substances are procedural materials that allow developers to change the look and feel of materials at runtime, or even give them the ability to quickly customize materials before statically rendering the bitmaps.
>
> At the time of writing this documentation, the plugin is currently only in beta, so please take caution installing it, and see the next section if you're experiencing issues using the .sbsar files inside Unity.

### Preset Files

All .sbsar files have presets built-in, though at the time of writing, the Substance3D for Unity plugin does not support reading these presets. As an alternative, a `presets` folder is included with each .sbsar file that contains the various presets that can be applied to the Susbtances.

### URP and HDRP Support

This asset contains 2 package files, `Materials_HDRP.unitypackage` and `Materials_URP.unitypackage`. When using the URP or HDRP pipelines, make sure to uncheck imports for the `Materials` and `MaterialsExample` folder, and to include the unitypackage that matches your current rendering pipeline. Once imported, open the unitypackage to import the converted materials into the asset folder.

### Helper Consts File

This package contains a helper C# file under the namespace `PolyLabs.MarbleAndGranite`. The classes within the namespace correspond to the Substance names, and each const is a helper string for accessing a property of the Substance. To help you as you code, docstrings are provided detailing the property type, description, and value ranges where applicable.

### Baking Static Images from .sbsar Files

If you're having trouble utilizing Substances in Unity, or don't want to use the Substances in your project, you can still customize the materials and export bitmaps using Adobes free [Substance Player](https://substance3d.adobe.com/documentation/sp31/user-s-guide-for-substance-player-2294742.html).

# Sbsar Materials Reference:


## **Granite_01**
Consts: `PolyLabs.MarbleAndGranite.Granite_01`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_02**
Consts: `PolyLabs.MarbleAndGranite.Granite_02`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_03**
Consts: `PolyLabs.MarbleAndGranite.Granite_03`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_04**
Consts: `PolyLabs.MarbleAndGranite.Granite_04`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_05**
Consts: `PolyLabs.MarbleAndGranite.Granite_05`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_06**
Consts: `PolyLabs.MarbleAndGranite.Granite_06`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_07**
Consts: `PolyLabs.MarbleAndGranite.Granite_07`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_08**
Consts: `PolyLabs.MarbleAndGranite.Granite_08`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_09**
Consts: `PolyLabs.MarbleAndGranite.Granite_09`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Granite_10**
Consts: `PolyLabs.MarbleAndGranite.Granite_10`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_01**
Consts: `PolyLabs.MarbleAndGranite.Marble_01`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.15 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_02**
Consts: `PolyLabs.MarbleAndGranite.Marble_02`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.15 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_03**
Consts: `PolyLabs.MarbleAndGranite.Marble_03`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_04**
Consts: `PolyLabs.MarbleAndGranite.Marble_04`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_05**
Consts: `PolyLabs.MarbleAndGranite.Marble_05`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_06**
Consts: `PolyLabs.MarbleAndGranite.Marble_06`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_07**
Consts: `PolyLabs.MarbleAndGranite.Marble_07`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

## **Marble_08**
Consts: `PolyLabs.MarbleAndGranite.Marble_08`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Cracks Opacity | cracks_opacity | The <b>Opacity</b> of the cracks on the marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Amount | cracks_amount | Randomly adds cracks to the marble. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Cracks Depth | cracks_depth | The depth of the cracks on the marble. | Float | Default: 3.42 <br/> Min: 0 <br/> Max: 50 <br/> Clamped: True |
| Marble Damage Depth | marble_damage_depth | The depth of the marble damages. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Marble Damage Scale | marble_damage_scale | The scale of the damage pattern on the marble. | Int | Default: 8 <br/> Min: 2 <br/> Max: 32 <br/> Clamped: True |
| Marble Damage Thickness | marble_damage_thickness | The thickness of the damage lines. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rough Veins | rough_veins | Gives the veins a high roughness value to allow them to stand out from the marble base. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Roughness | roughness | The roughness of the marble surface. Increase for a more worn-down and aged effect. Decrease for a newer, polished look. | Float | Default: 0.2 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Height Variation | height_variation | Changes the height variation of the material to give a carved look and feel. Increase the roughness to give the presentation of carved marble. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Normal Intensity | normal_intensity | The <b>Intensity</b> parameter modifies the intensity of normal map. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |

