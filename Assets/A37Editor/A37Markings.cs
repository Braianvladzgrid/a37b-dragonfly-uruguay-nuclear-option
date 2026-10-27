using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

// Original vector geometry, not pixels copied from reference photographs.
// Simplified FAU 282 markings based on the FAU museum's A-37 photographs.
public static class A37Markings {
 const string R=A37VisualBuilder.R+"/Markings";
 static Material Material(string name,Color color){string path=R+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Cull",0);m.SetFloat("_Smoothness",.15f);EditorUtility.SetDirty(m);return m;}
 static void Patch(GameObject visual,MeshFilter surface,int side,string name,Material material,Vector2[] outline,float z,float y,float layer=0){
  var trianglesSource=surface.sharedMesh.triangles;var points=surface.sharedMesh.vertices.Select(surface.transform.TransformPoint).ToArray();
  var vertices=new List<Vector3>();foreach(var p in outline){float py=y+p.y,pz=z+side*p.x,best=-100;bool found=false;
   for(int i=0;i<trianglesSource.Length;i+=3){var a=points[trianglesSource[i]];var b=points[trianglesSource[i+1]];var c=points[trianglesSource[i+2]];float d=(b.z-c.z)*(a.y-c.y)+(c.y-b.y)*(a.z-c.z);if(Mathf.Abs(d)<1e-9f)continue;float u=((b.z-c.z)*(py-c.y)+(c.y-b.y)*(pz-c.z))/d,v=((c.z-a.z)*(py-c.y)+(a.y-c.y)*(pz-c.z))/d;if(u<0||v<0||u+v>1)continue;float px=u*a.x+v*b.x+(1-u-v)*c.x;if(px*side>best){best=px*side;found=true;}}
   if(!found)return;vertices.Add(new Vector3(side*(best+.010f+layer*.001f),py,pz));}
  string path=R+"/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.vertices=vertices.ToArray();var triangles=new List<int>();for(int i=1;i<vertices.Count-1;i++){triangles.Add(0);triangles.Add(i);triangles.Add(i+1);}mesh.triangles=triangles.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);var go=new GameObject(name);go.transform.SetParent(visual.transform,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
 static Vector2[] Rect(float x,float y,float w,float h)=>new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)};
 public static void Apply(GameObject visual){
  System.IO.Directory.CreateDirectory(R);var body=visual.GetComponentsInChildren<MeshFilter>(true).First(m=>m.name=="fuselage");var surface=body;
  var blue=Material("FAU_Blue",new Color(.08f,.17f,.30f));var white=Material("FAU_White",new Color(.78f,.78f,.72f));var red=Material("FAU_Red",new Color(.60f,.05f,.06f));var black=Material("FAU_Stencil",new Color(.025f,.03f,.025f));
  foreach(int side in new[]{-1,1}){
   string suffix=side<0?"L":"R";
   Patch(visual,surface,side,"FAU_Flash_"+suffix,blue,Rect(-.15f,-.10f,.30f,.20f),-3.58f,1.08f);
   Patch(visual,surface,side,"FAU_FlashWhite_"+suffix,white,Rect(-.15f,-.0333f,.30f,.0666f),-3.58f,1.08f,1);
   Patch(visual,surface,side,"FAU_FlashRed_"+suffix,red,new[]{new Vector2(-.15f,.10f),new Vector2(-.10f,.10f),new Vector2(.15f,-.066f),new Vector2(.15f,-.10f),new Vector2(.10f,-.10f),new Vector2(-.15f,.066f)},-3.58f,1.08f,2);
   int[][] digits={new[]{0,2,3,4,6},new[]{0,1,2,3,4,5,6},new[]{0,2,3,4,6}};
   for(int d=0;d<3;d++)foreach(int segment in digits[d]){float x=-.255f+d*.18f,w=.14f,h=.24f,t=.022f;Vector2[] r=segment==0?Rect(x,h-t,w,t):segment==3?Rect(x,h/2-t/2,w,t):segment==6?Rect(x,0,w,t):segment==1?Rect(x,h/2,t,h/2):segment==2?Rect(x+w-t,h/2,t,h/2):segment==4?Rect(x,0,t,h/2):Rect(x+w-t,0,t,h/2);Patch(visual,surface,side,"FAU_282_"+suffix+"_"+d+"_"+segment,black,r,-2.0f,-.30f);}
  }
  int count=visual.GetComponentsInChildren<MeshFilter>(true).Count(m=>m.name.StartsWith("FAU_"));if(count<40)throw new Exception("FAU markings incomplete: "+count);Debug.Log("[A37] Projected FAU marking meshes: "+count);
 }
}
