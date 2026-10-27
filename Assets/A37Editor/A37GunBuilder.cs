using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

// Native firing/rearming system with a smaller-caliber game adaptation.
public static class A37GunBuilder {
 const string R=A37VisualBuilder.R+"/Weapons";
 static Object Clone(string name,string path){var source=A37VisualBuilder.Native(name,".asset");var asset=AssetDatabase.LoadMainAssetAtPath(path);if(!asset){asset=Object.Instantiate(source);AssetDatabase.CreateAsset(asset,path);}else EditorUtility.CopySerialized(source,asset);return asset;}
 static void N(SerializedObject s,string key,float v)=>s.FindProperty(key).floatValue=v;
 public static Object Build(){
  System.IO.Directory.CreateDirectory(R);var info=Clone("Gun12.7mm_Rotary",R+"/A37MinigunInfo.asset");var data=new SerializedObject(info);data.FindProperty("weaponName").stringValue="7.62 mm Minigun";data.FindProperty("shortName").stringValue="GUN 7.62MM";data.FindProperty("description").stringValue="Forward-firing A-37 minigun. Game adaptation: museum-published 1500 rpm; ballistics and damage are calibrated game parameters.";N(data,"muzzleVelocity",850);N(data,"massPerRound",.024f);N(data,"pierceDamage",45);N(data,"blastDamage",0);N(data,"costPerRound",.000001f);N(data,"armorTierEffectiveness",1);data.ApplyModifiedPropertiesWithoutUndo();
  var root=Object.Instantiate((GameObject)A37VisualBuilder.Native("gun_25mm_trainer_pod",".prefab"));root.name="A37Minigun";
  GameObject prefab;try{foreach(var collider in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);foreach(var m in root.GetComponentsInChildren<MeshFilter>(true))m.sharedMesh=null;var gun=A37VisualBuilder.Comp(root,"Gun");var g=new SerializedObject(gun);g.FindProperty("info").objectReferenceValue=info;N(g,"fireRate",1500);g.FindProperty("ammo").intValue=1500;g.FindProperty("magazineCapacity").intValue=1500;N(g,"recoilImpulse",8.1f);N(g,"tracerSize",.55f);N(g,"bulletSpread",3);var muzzles=g.FindProperty("muzzles");for(int i=0;i<muzzles.arraySize;i++){var t=(Transform)muzzles.GetArrayElementAtIndex(i).objectReferenceValue;t.position=root.transform.position;t.rotation=Quaternion.identity;}g.ApplyModifiedPropertiesWithoutUndo();foreach(var p in root.GetComponentsInChildren<ParticleSystem>(true)){p.transform.localScale*=.25f;}prefab=PrefabUtility.SaveAsPrefabAsset(root,R+"/A37Minigun.prefab");}finally{Object.DestroyImmediate(root);}
  var mount=Clone("gun_25mm_trainer_pod",R+"/A37MinigunMount.asset");var mnt=new SerializedObject(mount);mnt.FindProperty("jsonKey").stringValue="A37B_762_Internal";mnt.FindProperty("mountName").stringValue="7.62 mm Minigun (1500 rounds)";mnt.FindProperty("prefab").objectReferenceValue=prefab;mnt.FindProperty("info").objectReferenceValue=info;mnt.FindProperty("ammo").intValue=1500;N(mnt,"mass",36);N(mnt,"emptyMass",0);N(mnt,"emptyCost",0);N(mnt,"drag",0);N(mnt,"emptyDrag",0);N(mnt,"RCS",0);N(mnt,"emptyRCS",0);mnt.FindProperty("colorable").boolValue=false;mnt.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(info);EditorUtility.SetDirty(mount);return mount;
 }
}
