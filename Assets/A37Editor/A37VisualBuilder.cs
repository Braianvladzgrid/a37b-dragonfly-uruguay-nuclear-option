using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

// Original integration code, MIT. Source-derived assets remain GPL-2.0.
public static class A37VisualBuilder {
 public const string R="Assets/Blueprinter/Mods/A37B";
 public const int Tier=1;
 public const float PriceMillions=13;
 static string source;
 static readonly List<string> imported=new List<string>();
 public static void Prepare(){Blueprinter.GameAssemblySync.Synchronize();SessionState.SetBool("A37.Resume",true);EditorApplication.delayCall+=()=>EditorUtility.RequestScriptReload();}
 public static void PrepareFlight(){SessionState.SetBool("A37.BuildFlight",true);Prepare();}
 public static void PrepareAircraftOnly(){SessionState.SetBool("A37.SkipVisual",true);PrepareFlight();}
 [InitializeOnLoadMethod] static void Resume(){if(!SessionState.GetBool("A37.Resume",false))return;SessionState.SetBool("A37.Resume",false);EditorApplication.delayCall+=()=>{try{if(!SessionState.GetBool("A37.SkipVisual",false))BuildAndInspect();SessionState.SetBool("A37.SkipVisual",false);if(SessionState.GetBool("A37.BuildFlight",false)){SessionState.SetBool("A37.BuildFlight",false);A37AircraftBuilder.Build();}EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}};}
 // Unity's OBJ importer mirrors the converted OBJ X axis. Match XML offsets
 // to that imported geometry, rather than to the intermediate OBJ coordinates.
 public static Vector3 FG(float x,float y,float z)=>new Vector3(y,z,-x);
 static float N(XElement e,string n,float fallback=0){var v=e?.Element(n);return v==null?fallback:float.Parse(v.Value,CultureInfo.InvariantCulture);}
 static string Project=>Directory.GetParent(Application.dataPath).FullName;
 static string Report=>Path.GetFullPath("../../work/a37");
 public static Object Native(string name,string ext)=>AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAllAssetPaths().Single(p=>p.StartsWith("Assets/Blueprinter/_donotship/")&&p.EndsWith("/"+name+"_PLACEHOLDER"+ext)));
 public static Component Comp(GameObject root,string type)=>root.GetComponentsInChildren<Component>(true).First(c=>c&&c.GetType().Name==type);
 public static void EnsureSource(){
  source=Path.Combine(Project,"Temp/A37Source/Cessna-A37-Dragonfly");if(Directory.Exists(source))return;
  var archive=Path.Combine(Project,"SourceAssets/Cessna-A37-Dragonfly-r21797-original.zip");if(!File.Exists(archive))archive=Path.Combine(Project,"A37SourceAssets/Cessna-A37-Dragonfly-r21797-original.zip");
  var folder=Path.GetDirectoryName(source);Directory.CreateDirectory(folder);
  using(var zip=ZipFile.OpenRead(archive))foreach(var entry in zip.Entries){var path=Path.GetFullPath(Path.Combine(folder,entry.FullName));if(!path.StartsWith(Path.GetFullPath(folder)+Path.DirectorySeparatorChar))throw new Exception("Unsafe source path");Directory.CreateDirectory(Path.GetDirectoryName(path));if(!entry.FullName.EndsWith("/"))entry.ExtractToFile(path,true);}
 }
 static void Materials(GameObject root){
  Directory.CreateDirectory(R+"/Materials");
  foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true)){
   if(renderer.name.StartsWith("HDR")){renderer.gameObject.SetActive(false);continue;}
   var slots=renderer.sharedMaterials;
   for(int i=0;i<slots.Length;i++){
    var original=slots[i];if(!original)continue;
    bool glass=renderer.name.IndexOf("vitre",StringComparison.OrdinalIgnoreCase)>=0||renderer.name.IndexOf("verriere",StringComparison.OrdinalIgnoreCase)>=0||renderer.name=="lunettes";
    if(renderer.name.IndexOf("tour",StringComparison.OrdinalIgnoreCase)>=0)glass=false;
    var key=original.name+(glass?"_glass":"");var path=R+"/Materials/"+key.Replace('/','_')+".mat";
    var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
    var color=original.HasProperty("_Color")?original.GetColor("_Color"):Color.white;
    mat.SetColor("_BaseColor",color);if(original.mainTexture)mat.SetTexture("_BaseMap",original.mainTexture);mat.SetFloat("_Smoothness",glass?.8f:.23f);
    if(glass){mat.SetColor("_BaseColor",new Color(.65f,.78f,.83f,.08f));mat.SetFloat("_Surface",1);mat.SetFloat("_ZWrite",0);mat.SetFloat("_SrcBlend",5);mat.SetFloat("_DstBlend",10);mat.SetFloat("_Cull",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;mat.SetShaderPassEnabled("ShadowCaster",false);}
    EditorUtility.SetDirty(mat);slots[i]=mat;
   }renderer.sharedMaterials=slots;
  }
 }
 static Transform Xml(string file,Transform parent,int depth=0){
  if(depth>20)throw new Exception("XML nesting limit");
  var full=Path.GetFullPath(file);if(!full.StartsWith(Path.GetFullPath(source)+Path.DirectorySeparatorChar))return null;
  if(full.IndexOf(Path.DirectorySeparatorChar+"Effects"+Path.DirectorySeparatorChar,StringComparison.Ordinal)>=0||full.IndexOf(Path.DirectorySeparatorChar+"lights"+Path.DirectorySeparatorChar,StringComparison.Ordinal)>=0)return null;
  var document=XDocument.Load(full).Root;var group=new GameObject(Path.GetFileNameWithoutExtension(file)).transform;group.SetParent(parent,false);
  string model=document.Element("path")?.Value.Trim();
  if(!string.IsNullOrEmpty(model)&&model.EndsWith(".ac",StringComparison.OrdinalIgnoreCase)){
   var ac=Path.GetFullPath(Path.Combine(Path.GetDirectoryName(full),model));var rel=ac.Substring(Path.Combine(source,"Models").Length+1).Replace('\\','/');var asset=R+"/Models/"+Path.ChangeExtension(rel,"obj");
   var mesh=AssetDatabase.LoadAssetAtPath<GameObject>(asset);if(!mesh)throw new Exception("Converted source model missing: "+asset);
   var instance=Object.Instantiate(mesh,group);instance.name=Path.GetFileNameWithoutExtension(model);Materials(instance);imported.Add(rel);
  }
  foreach(var child in document.Elements("model")){
   string path=child.Element("path")?.Value.Trim();if(string.IsNullOrEmpty(path)||!path.EndsWith(".xml"))continue;
   string next=Path.Combine(Path.GetDirectoryName(full),path);if(!File.Exists(next))continue;
   var node=Xml(next,group,depth+1);if(!node)continue;
   var offsets=child.Element("offsets");node.localPosition=FG(N(offsets,"x-m"),N(offsets,"y-m"),N(offsets,"z-m"));
   node.localRotation=Quaternion.AngleAxis(-N(offsets,"heading-deg"),Vector3.up)*Quaternion.AngleAxis(N(offsets,"pitch-deg"),Vector3.left)*Quaternion.AngleAxis(N(offsets,"roll-deg"),Vector3.forward);
   var scale=child.Element("overlay")?.Elements("animation").FirstOrDefault(a=>a.Element("type")?.Value.Trim()=="scale");
   if(scale!=null)node.localScale=new Vector3(N(scale,"y-offset",1),N(scale,"z-offset",1),N(scale,"x-offset",1));
   if(child.Element("name")!=null)node.name=child.Element("name").Value;
  }
  return group;
 }
 public static void BuildAndInspect(){
  EnsureSource();Directory.CreateDirectory(Report);AssetDatabase.Refresh();imported.Clear();
  foreach(var file in Directory.GetFiles(R+"/Models","*.obj",SearchOption.AllDirectories)){
   var importer=(ModelImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));if(!importer)continue;
   if(importer.globalScale!=1||!importer.isReadable||importer.importNormals!=ModelImporterNormals.Calculate){importer.globalScale=1;importer.isReadable=true;importer.importNormals=ModelImporterNormals.Calculate;importer.normalSmoothingAngle=65;importer.SaveAndReimport();}
  }
  var root=new GameObject("A37B_SourceAssembly");
  try{
   Xml(Path.Combine(source,"Models/a37.xml"),root.transform);
   PrefabUtility.SaveAsPrefabAsset(root,R+"/A37Visual.prefab");
   File.WriteAllLines(Path.Combine(Report,"assembled-models.txt"),imported);
   Preview(root);
  }finally{Object.DestroyImmediate(root);}
  InspectDonors();AssetDatabase.SaveAssets();Debug.Log("[A37] Source visual assembly and donor inspection complete; tier=1 price=13M. Flight not built/tested.");
 }
 static void Preview(GameObject root){
  var cam=new GameObject("A37PreviewCamera").AddComponent<Camera>();var sun=new GameObject("A37PreviewSun").AddComponent<Light>();
  sun.type=LightType.Directional;sun.intensity=2;sun.transform.rotation=Quaternion.Euler(40,-35,0);
  RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.65f,.65f);
  cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.12f,.17f,.21f);cam.nearClipPlane=.025f;cam.farClipPlane=100;cam.fieldOfView=55;
  try{cam.transform.position=new Vector3(-9,4,8);cam.transform.LookAt(new Vector3(0,0,0));Save(cam,"exterior");cam.transform.position=new Vector3(-.303f,.122f,1.384f);cam.transform.rotation=Quaternion.Euler(3,0,0);cam.fieldOfView=80;var pilot=root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="ThePilot");if(pilot)pilot.gameObject.SetActive(false);Save(cam,"cockpit");}
  finally{Object.DestroyImmediate(cam.gameObject);Object.DestroyImmediate(sun.gameObject);}
 }
 static void Save(Camera camera,string name){
  Directory.CreateDirectory(Path.Combine(Report,"previews"));var rt=new RenderTexture(1280,800,24);var tex=new Texture2D(1280,800,TextureFormat.RGBA32,false);var previous=RenderTexture.active;
  try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,800),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Report,"previews/"+name+".png"),tex.EncodeToPNG());}
  finally{RenderTexture.active=previous;camera.targetTexture=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
 }
 static void InspectDonors(){
  var lines=new List<string>();
  var assembly=System.Reflection.Assembly.LoadFrom(Path.Combine(Project,"Packages/nuclearoption/Assembly-CSharp.dll"));
  if(AssetDatabase.LoadAllAssetsAtPath("Packages/nuclearoption/Assembly-CSharp.dll").OfType<MonoScript>().Any(s=>s.name=="Aircraft"&&s.GetClass()==null))AssetDatabase.ImportAsset("Packages/nuclearoption/Assembly-CSharp.dll",ImportAssetOptions.ForceUpdate);
  lines.Add("GAME ASSEMBLY "+assembly.FullName+" Aircraft="+assembly.GetType("Aircraft"));
  foreach(var script in AssetDatabase.LoadAllAssetsAtPath("Packages/nuclearoption/Assembly-CSharp.dll").OfType<MonoScript>().Where(s=>new[]{"Aircraft","Turbojet","AeroPart"}.Contains(s.name)))lines.Add("SCRIPT "+script.name+" class="+script.GetClass());
  foreach(var donor in new[]{"trainer","Fighter1"}){
   var path=AssetDatabase.GetAllAssetPaths().Single(p=>p.StartsWith("Assets/Blueprinter/_donotship/")&&p.EndsWith("/"+donor+"_PLACEHOLDER.prefab"));AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
   var prefab=Native(donor,".prefab") as GameObject;lines.Add("DONOR "+donor);lines.Add("COMPONENT TYPES: "+string.Join(",",prefab.GetComponentsInChildren<Component>(true).Where(c=>c).Select(c=>c.GetType().FullName).Distinct()));
   foreach(var part in prefab.GetComponentsInChildren<Component>(true).Where(c=>c&&new[]{"AeroPart","LandingGear","Turbojet","JetNozzle","WeaponManager","Aircraft","ControlSurface","HighLiftDevice"}.Contains(c.GetType().Name))){lines.Add("NODE "+part.name+" "+part.GetType().Name+" position="+prefab.transform.InverseTransformPoint(part.transform.position));lines.Add(EditorJsonUtility.ToJson(part,true));}
  }File.WriteAllLines(Path.Combine(Report,"donor-inspection.txt"),lines);
 }
}

