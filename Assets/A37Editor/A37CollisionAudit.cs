using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
public static class A37CollisionAudit {
 static Component Owner(Collider c)=>c.GetComponentsInParent<Component>(true).FirstOrDefault(p=>p&&p.GetType().Name=="AeroPart");
 static bool Linked(Component a,Component b){var joints=new SerializedObject(a).FindProperty("joints");for(int i=0;i<joints.arraySize;i++)if(joints.GetArrayElementAtIndex(i).FindPropertyRelative("connectedPart").objectReferenceValue==b)return true;return false;}
 public static void Run(){var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(A37AircraftBuilder.R+"/A37.prefab"));try{Physics.SyncTransforms();var report=new List<string>();var colliders=root.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.enabled&&!c.isTrigger&&Owner(c)).ToArray();for(int i=0;i<colliders.Length;i++)for(int j=i+1;j<colliders.Length;j++){var a=colliders[i];var b=colliders[j];var pa=Owner(a);var pb=Owner(b);if(pa==pb||Linked(pa,pb)||Linked(pb,pa))continue;if(Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out var dir,out float depth)&&depth>.001f)report.Add(a.name+" / "+b.name+" depth="+depth.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));}report.Insert(0,"Unlinked hull penetrations="+report.Count);report.AddRange(colliders.Select(c=>c.name+" bounds="+c.bounds+" rot="+c.transform.eulerAngles));File.WriteAllLines("../../work/a37/collision-audit.txt",report);Debug.Log("[A37Hull] "+report[0]);}finally{Object.DestroyImmediate(root);}}
}

