using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Blueprinter;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

// Original integration code, MIT; aircraft geometry and textures remain GPL-2.0.
public static class A37AircraftBuilder {
 public const string R=A37VisualBuilder.R, Key="A37B_Dragonfly_Uruguay";
 static readonly Vector3 ModelScale=new Vector3(10.93f/11.96947f,1,8.62f/8.457175f);
 static Component C(GameObject g,string type)=>A37VisualBuilder.Comp(g,type);
 static Transform T(GameObject g,string name)=>g.GetComponentsInChildren<Transform>(true).First(t=>t.name==name);
 static Transform Ref(SerializedObject s,string name){var o=s.FindProperty(name).objectReferenceValue;return o is GameObject g?g.transform:o as Transform;}
 static IEnumerable<Component> All(GameObject g,string type)=>g.GetComponentsInChildren<Component>(true).Where(c=>c&&c.GetType().Name==type);
 static void Number(SerializedObject s,string name,float value){var p=s.FindProperty(name);if(p==null)throw new Exception("Required field missing: "+name);p.floatValue=value;}
 static void Text(SerializedObject s,string name,string value)=>s.FindProperty(name).stringValue=value;
 static void Renderers(SerializedObject s,string name,IEnumerable<Renderer> renderers){var p=s.FindProperty(name);var a=renderers.Where(r=>r).Distinct().ToArray();p.arraySize=a.Length;for(int i=0;i<a.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=a[i];}
 static Bounds WorldBounds(MeshFilter mesh){var b=mesh.sharedMesh.bounds;var r=new Bounds(mesh.transform.TransformPoint(b.center),Vector3.zero);foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1})r.Encapsulate(mesh.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z))));return r;}
 static Object Native(string n,string ext)=>A37VisualBuilder.Native(n,ext);
 public static void Build(){
  A37VisualBuilder.EnsureSource();AssetDatabase.Refresh();Directory.CreateDirectory(R+"/Collision");
  var nativeRoot=Native("trainer",".prefab") as GameObject;
  var nativeDef=new SerializedObject(C(nativeRoot,"Aircraft")).FindProperty("definition").objectReferenceValue as ScriptableObject;
  var nativeParams=new SerializedObject(nativeDef).FindProperty("aircraftParameters").objectReferenceValue as ScriptableObject;
  foreach(var f in new[]{"A37.prefab","A37Definition.asset","A37Parameters.asset","A37Hangars.asset"})if(AssetDatabase.LoadMainAssetAtPath(R+"/"+f))AssetDatabase.DeleteAsset(R+"/"+f);
  var definition=Object.Instantiate(nativeDef);definition.name=Key;var parameters=Object.Instantiate(nativeParams);parameters.name=Key+"Parameters";
  var ds=new SerializedObject(definition);Text(ds,"jsonKey",Key);Text(ds,"unitName","A-37B Dragonfly Uruguay");Text(ds,"code","A-37B");Text(ds,"description","Uruguayan light attack jet. Tier 1, $13 million. Two dry J85 engines and eight underwing stations. Flight tuning is experimental.");
  Number(ds,"value",13);Number(ds,"mass",2817);Number(ds,"length",8.62f);Number(ds,"width",10.93f);Number(ds,"height",2.7f);Number(ds,"radarSize",.6f);ds.FindProperty("spawnOffset").vector3Value=new Vector3(0,1.42f,0);ds.FindProperty("aircraftParameters").objectReferenceValue=parameters;ds.ApplyModifiedPropertiesWithoutUndo();
  var info=ds.FindProperty("aircraftInfo");info.FindPropertyRelative("emptyWeight").floatValue=2817;info.FindPropertyRelative("maxWeight").floatValue=7000;info.FindPropertyRelative("maxSpeed").floatValue=770;info.FindPropertyRelative("stallSpeed").floatValue=182;ds.ApplyModifiedPropertiesWithoutUndo();
  var ps=new SerializedObject(parameters);Text(ps,"aircraftName","A-37B Dragonfly Uruguay");ps.FindProperty("rankRequired").intValue=1;ps.FindProperty("liveries").arraySize=0;
  // Native trainer defaults contain five sets and a gun pod absent from this airframe.
  var defaults=ps.FindProperty("loadouts");defaults.arraySize=2;
  for(int faction=0;faction<2;faction++){var weapons=defaults.GetArrayElementAtIndex(faction).FindPropertyRelative("weapons");weapons.arraySize=4;for(int station=0;station<4;station++)weapons.GetArrayElementAtIndex(station).objectReferenceValue=null;}
  ps.FindProperty("StandardLoadouts").arraySize=0;Number(ps,"maxSpeed",770f/3.6f);Number(ps,"takeoffSpeed",60);Number(ps,"approachSpeed",55);Number(ps,"landingSpeed",51);Number(ps,"shortLandingSpeed",51);Number(ps,"cornerSpeed",110);ps.ApplyModifiedPropertiesWithoutUndo();
  AssetDatabase.CreateAsset(definition,R+"/A37Definition.asset");AssetDatabase.CreateAsset(parameters,R+"/A37Parameters.asset");ConfigureLivery(parameters,nativeParams);
  var root=Object.Instantiate(nativeRoot);root.name=Key;
  try{
   foreach(var lod in root.GetComponentsInChildren<LODGroup>(true))lod.enabled=false;
   // Compress donor stations, leaving unit scales and native suspension mechanics intact.
   var transforms=root.GetComponentsInChildren<Transform>(true);var original=transforms.ToDictionary(t=>t,t=>t.position);
   foreach(var t in transforms)if(t!=root.transform)t.position=Vector3.Scale(original[t],new Vector3(.84f,.65f,.59f));
   var aircraft=new SerializedObject(C(root,"Aircraft"));var oldCockpit=aircraft.FindProperty("cockpitRenderers");var cockpitRenderers=Enumerable.Range(0,oldCockpit.arraySize).Select(i=>oldCockpit.GetArrayElementAtIndex(i).objectReferenceValue as Renderer).Where(r=>r).ToArray();
   var displays=new HashSet<Renderer>(cockpitRenderers.Where(r=>r.name.ToLowerInvariant().Contains("screen")||r.name.ToLowerInvariant().Contains("display")));
   foreach(var mesh in root.GetComponentsInChildren<MeshFilter>(true))if(!displays.Contains(mesh.GetComponent<Renderer>()))mesh.sharedMesh=null;
   foreach(var mesh in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))mesh.sharedMesh=null;
   var visual=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(R+"/A37Visual.prefab"),root.transform);visual.name="A37Exterior";visual.transform.localScale=ModelScale;
   A37Markings.Apply(visual);
   foreach(var t in visual.GetComponentsInChildren<Transform>(true))t.gameObject.layer=root.layer;
   var body=T(visual,"a37"); // Root OBJ object, separate from XML assembly group.
   var bodyMeshes=body.GetComponentsInChildren<MeshFilter>(true).Where(m=>m.sharedMesh).ToArray();
   var cabin=Object.Instantiate(T(visual,"interior").gameObject,root.transform);cabin.name="A37Cockpit";cabin.transform.position=T(visual,"interior").position;cabin.transform.rotation=T(visual,"interior").rotation;cabin.transform.localScale=ModelScale;
   var firstPilot=cabin.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="ThePilot");if(firstPilot)firstPilot.gameObject.SetActive(false);
   int cockpitLayer=cockpitRenderers.First().gameObject.layer;
   foreach(var t in cabin.GetComponentsInChildren<Transform>(true))t.gameObject.layer=cockpitLayer;
   foreach(var n in new[]{"vitres","verriere","tourverriere","rebord"}){
    var part=bodyMeshes.FirstOrDefault(m=>m.name==n);if(!part)continue;var copy=Object.Instantiate(part.gameObject,cabin.transform);copy.transform.position=part.transform.position;copy.transform.rotation=part.transform.rotation;copy.transform.localScale=part.transform.lossyScale;copy.layer=cockpitLayer;
   }
   var eye=new GameObject("A37PilotViewPoint").transform;eye.SetParent(T(root,"cockpit_F"),false);eye.position=Vector3.Scale(new Vector3(-.303f,.122f,1.384f),ModelScale);eye.rotation=Quaternion.identity;
   // Functional native displays are compact backup instruments below the analog panel.
   foreach(var display in displays){display.transform.position+=new Vector3(-.28f,-.50f,1.82f)-display.bounds.center;display.transform.localScale*=.4f;}
   aircraft.FindProperty("cockpitViewPoint").objectReferenceValue=eye;aircraft.FindProperty("definition").objectReferenceValue=definition;
   Renderers(aircraft,"cockpitRenderers",displays.Concat(cabin.GetComponentsInChildren<Renderer>(true)));Renderers(aircraft,"exteriorRenderers",visual.GetComponentsInChildren<Renderer>(true));aircraft.ApplyModifiedPropertiesWithoutUndo();
   File.WriteAllLines("../../work/a37/imported-bounds.txt",bodyMeshes.Where(m=>new[]{"roueG","roueD","profondeur","direction","aileronG","voletG"}.Contains(m.name)).Select(m=>m.name+" "+WorldBounds(m)));FitControls(root,bodyMeshes);FitGear(root,visual,bodyMeshes);ConfigureWeapons(root,parameters);ConfigureEngines(root);FitPhysics(root,bodyMeshes);CreateIcon(visual,definition);
   foreach(var t in root.GetComponentsInChildren<Transform>(true))foreach(var g in Enumerable.Range(0,t.childCount).Select(t.GetChild).GroupBy(c=>c.name).Where(g=>g.Count()>1)){int i=0;foreach(var child in g)if(i++>0)child.name+="_A37_"+i;}
   var prefab=PrefabUtility.SaveAsPrefabAsset(root,R+"/A37.prefab");ds.Update();ds.FindProperty("unitPrefab").objectReferenceValue=prefab;ds.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(definition);
  }finally{Object.DestroyImmediate(root);}
  AddHangars(nativeDef);AssetDatabase.SaveAssets();Validate();A37CollisionAudit.Run();ModBuilder.Build("A37B",Key,"0.1.0",Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Builds"));Debug.Log("[A37] Compiled prototype; in-game flight NOT yet validated.");
 }
 static void FitPhysics(GameObject root,MeshFilter[] meshes){
  var parts=All(root,"AeroPart").ToArray();float mass=parts.Sum(c=>new SerializedObject(c).FindProperty("mass").floatValue);
  float wing=parts.Where(c=>c.name.StartsWith("wing")||c.name.StartsWith("aileron")||c.name.StartsWith("flap")).Sum(c=>new SerializedObject(c).FindProperty("wingArea").floatValue);
  foreach(var part in parts){var so=new SerializedObject(part);Number(so,"mass",so.FindProperty("mass").floatValue*2817f/mass);if(part.name.StartsWith("wing")||part.name.StartsWith("aileron")||part.name.StartsWith("flap"))Number(so,"wingArea",so.FindProperty("wingArea").floatValue*17.09f/wing);else Number(so,"wingArea",so.FindProperty("wingArea").floatValue*.45f);so.ApplyModifiedPropertiesWithoutUndo();}
  // The trainer's original mass distribution is aft of the A-37 main wheels.
  // Reweight actual structural stations, preserving total empty mass and native
  // body contacts/inertias. Target gives about 17% static nose-wheel loading.
  var weights=parts.Select(p=>(double)new SerializedObject(p).FindProperty("mass").floatValue).ToArray();double lower=-2,upper=2;
  for(int iteration=0;iteration<45;iteration++){double k=(lower+upper)/2,total=0,moment=0;for(int i=0;i<parts.Length;i++){double w=weights[i]*Math.Exp(k*parts[i].transform.position.z);total+=w;moment+=w*parts[i].transform.position.z;}if(moment/total<1.05)lower=k;else upper=k;}
  double bias=(lower+upper)/2;double denominator=parts.Select((p,i)=>weights[i]*Math.Exp(bias*p.transform.position.z)).Sum();
  for(int i=0;i<parts.Length;i++){var s=new SerializedObject(parts[i]);Number(s,"mass",(float)(2817*weights[i]*Math.Exp(bias*parts[i].transform.position.z)/denominator));if((parts[i].name.StartsWith("vstab")||parts[i].name=="rudder_R")&&parts[i].name.EndsWith("_R")){Number(s,"wingArea",0);Number(s,"dragArea",0);}s.ApplyModifiedPropertiesWithoutUndo();}
  // Donor lift stations retain the trainer's swept planform. Fit fixed-wing
  // application points to a measured 40-percent chord section of this mesh.
  // This is an initial aerodynamic engineering fit, to be validated in flight.
  var wingMesh=meshes.First(m=>m.name=="ailes");var vertices=wingMesh.sharedMesh.vertices.Select(wingMesh.transform.TransformPoint).ToArray();var triangles=wingMesh.sharedMesh.triangles;var liftReport=new List<string>();
  foreach(var part in parts.Where(p=>p.name.StartsWith("wing"))){float x=part.transform.position.x;var zs=new List<float>();for(int i=0;i<triangles.Length;i+=3)for(int e=0;e<3;e++){var a=vertices[triangles[i+e]];var b=vertices[triangles[i+(e+1)%3]];if(Mathf.Abs(b.x-a.x)<1e-6f||(x-a.x)*(x-b.x)>0)continue;zs.Add(Mathf.Lerp(a.z,b.z,(x-a.x)/(b.x-a.x)));}if(zs.Count<2)throw new Exception("Wing section missing at "+part.name);float z=Mathf.Lerp(zs.Max(),zs.Min(),.40f);var so=new SerializedObject(part);var normal=Ref(so,"liftNormal");if(!normal)normal=part.transform;var target=new Vector3(x,part.transform.position.y,z);so.FindProperty("centerOfLift").vector3Value=normal.InverseTransformDirection(target-part.transform.position);so.ApplyModifiedPropertiesWithoutUndo();liftReport.Add(part.name+" liftWorld="+target+" massStation="+part.transform.position);}
  File.WriteAllLines("../../work/a37/lift-stations.txt",liftReport);
  File.WriteAllText("../../work/a37/mass-stations.txt",string.Join("\n",parts.Select(p=>p.name+" mass="+new SerializedObject(p).FindProperty("mass").floatValue.ToString("F2",CultureInfo.InvariantCulture)+" position="+p.transform.position))+"\nEmpty target CG z=1.05m; total mass=2817kg. Engineering fit, not a measured real-aircraft datum.\n");
  var tanks=All(root,"FuelTank").ToArray();float totalFuel=tanks.Sum(t=>new SerializedObject(t).FindProperty("fuelCapacity").floatValue);
  foreach(var tank in tanks){var s=new SerializedObject(tank);var cap=s.FindProperty("fuelCapacity");cap.floatValue*=1536f/totalFuel;s.ApplyModifiedPropertiesWithoutUndo();}
  // Collider bounds come from the A-37 geometry; no donor wings or nose remain collidable.
  var body=WorldBounds(meshes.First(m=>m.name=="fuselage"));var done=new HashSet<GameObject>();
  foreach(var col in root.GetComponentsInChildren<MeshCollider>(true)){
   if(!done.Add(col.gameObject))continue;string n=col.name;Bounds b;bool left=n.EndsWith("_L");
   string source=n.StartsWith("aileron")?(left?"aileronG":"aileronD"):n.StartsWith("flap")?(left?"voletG":"voletD"):n.StartsWith("elevator")?"profondeur":n.StartsWith("rudder")?"direction":null;
   if(n.StartsWith("weaponbay")||n.StartsWith("vstab")&&n.EndsWith("_R")||n.StartsWith("rudder")&&n.EndsWith("_R")){col.enabled=false;continue;}
   if(source!=null)b=WorldBounds(meshes.First(m=>m.name==source));
   else if(n.StartsWith("wing")){var wb=WorldBounds(meshes.First(m=>m.name=="ailes"));float lo=n.StartsWith("wingtip")?4.3f:n.StartsWith("wing2")?2.65f:n.StartsWith("wing1_")?1.25f:.9f;float hi=n.StartsWith("wingtip")?5.45f:n.StartsWith("wing2")?4.3f:n.StartsWith("wing1_")?2.65f:1.25f;if(n.StartsWith("wingroot2"))hi=1.1f;float z=wb.center.z;float depth=Mathf.Min(1.35f,wb.size.z);if(n.StartsWith("wingroot1")){z+=depth*.25f;depth*=.48f;}if(n.StartsWith("wingroot2")){z-=depth*.25f;depth*=.48f;}b=new Bounds(new Vector3((left?-1:1)*(lo+hi)/2,wb.center.y,z),new Vector3(hi-lo-.025f,.1f,depth));}
   else if(n.StartsWith("hstab"))b=new Bounds(new Vector3(left?-1:1,.397f,-3.18f),new Vector3(1.7f,.08f,.38f));
   else if(n.StartsWith("vstab"))b=new Bounds(new Vector3(0,n.StartsWith("vstab1")?.70f:1.13f,-3.71f),new Vector3(.06f,.39f,.36f));
   else if(n.StartsWith("engine")||n.StartsWith("intake")||n.StartsWith("nozzle"))b=new Bounds(new Vector3(left?-.65f:.65f,-.25f,n.StartsWith("intake")?.5f:n.StartsWith("nozzle")?-1.8f:-.65f),new Vector3(.45f,.5f,n.StartsWith("engine")?1.3f:.5f));
   else if(n.Contains("gear")){col.enabled=false;continue;}
   else{float z=col.transform.position.z;float lo=z>2.7f?2.7f:n=="cockpit_F"?1.9f:z>1?1:z>-.5f?-.5f:z>-2.3f?-2.3f:-4.1f;float hi=z>2.7f?4.1f:n=="cockpit_R"?1.9f:z>1?2.7f:z>-.5f?1:z>-2.3f?-.5f:-2.3f;b=new Bounds(new Vector3(0,body.center.y,(lo+hi)/2),new Vector3(z>2.7f?.55f:.68f,Mathf.Min(.8f,body.size.y),hi-lo-.025f));}
   if(source=="voletG"||source=="voletD"){var min=b.min;var max=b.max;if(left)min.x=-2.88f;else max.x=2.88f;b.SetMinMax(min,max);} if(n=="tail")b=new Bounds(new Vector3(0,-.14f,-3.2f),new Vector3(.55f,.78f,1.75f)); if(source=="direction")b=new Bounds(new Vector3(0,.94f,-4.14f),new Vector3(.055f,.79f,.22f)); if(source=="profondeur"){var min=b.min;var max=b.max;if(left)max.x=-.025f;else min.x=.025f;b.SetMinMax(min,max);}
   Hull(col,b,R+"/Collision/"+n+".asset");
  }
 }
 static void Hull(MeshCollider col,Bounds world,string path){
  var b=new Bounds(col.transform.InverseTransformPoint(world.center),Vector3.zero);foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1})b.Encapsulate(col.transform.InverseTransformPoint(world.center+Vector3.Scale(world.extents,new Vector3(x,y,z))));
  var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();var c=world.center;var e=Vector3.Max(world.extents,Vector3.one*.025f);mesh.vertices=new[]{c+new Vector3(-e.x,-e.y,-e.z),c+new Vector3(e.x,-e.y,-e.z),c+new Vector3(e.x,e.y,-e.z),c+new Vector3(-e.x,e.y,-e.z),c+new Vector3(-e.x,-e.y,e.z),c+new Vector3(e.x,-e.y,e.z),c+new Vector3(e.x,e.y,e.z),c+new Vector3(-e.x,e.y,e.z)}.Select(v=>col.transform.InverseTransformPoint(v)).ToArray();mesh.triangles=new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};mesh.RecalculateNormals();mesh.RecalculateBounds();col.sharedMesh=mesh;EditorUtility.SetDirty(mesh);
 }
 static void FitControls(GameObject root,MeshFilter[] meshes){
  foreach(var side in new[]{"L","R"}){T(root,"vstab1_"+side).position=new Vector3(0,.70f,-3.71f);T(root,"vstab2_"+side).position=new Vector3(0,1.13f,-3.71f);T(root,"hstab_"+side).position=new Vector3(side=="L"?-1:1,.397f,-3.18f);}
  foreach(var p in new[]{new[]{"aileron_L","aileronG"},new[]{"aileron_R","aileronD"},new[]{"rudder_L","direction"},new[]{"elevator_L","profondeur"}}){var c=C(T(root,p[0]).gameObject,"ControlSurface");var so=new SerializedObject(c);var moving=Ref(so,"visibleMesh");var mesh=meshes.First(m=>m.name==p[1]);var b=WorldBounds(mesh);c.transform.position=b.center;moving.position=new Vector3(b.center.x,b.center.y,b.max.z);mesh.transform.SetParent(moving,true);}
  // Both elevator force stations straddle the centerline. The source visual
  // is a combined elevator mesh, driven in pitch only from its central hinge.
  var elevatorMeshes=meshes.First(m=>m.name=="profondeur");var eb=WorldBounds(elevatorMeshes);var movingLeft=Ref(new SerializedObject(C(T(root,"elevator_L").gameObject,"ControlSurface")),"visibleMesh");var hold=Object.Instantiate(elevatorMeshes.gameObject);hold.transform.position=elevatorMeshes.transform.position;hold.transform.rotation=elevatorMeshes.transform.rotation;hold.transform.localScale=elevatorMeshes.transform.lossyScale;
  T(root,"elevator_L").position=new Vector3(-1,eb.center.y,eb.center.z);movingLeft.position=new Vector3(0,eb.center.y,eb.max.z);elevatorMeshes.transform.position=hold.transform.position;elevatorMeshes.transform.rotation=hold.transform.rotation;Object.DestroyImmediate(hold);T(root,"elevator_R").position=new Vector3(1,eb.center.y,eb.center.z);
  foreach(var c in All(root,"ControlSurface").Where(c=>c.name=="rudder_R"||c.name.StartsWith("elevator_"))){var s=new SerializedObject(c);Number(s,"rollRange",0);if(c.name=="rudder_R")Number(s,"yawRange",0);s.ApplyModifiedPropertiesWithoutUndo();}
  foreach(var side in new[]{"L","R"}){var flap=C(T(root,"flap_"+side).gameObject,"HighLiftDevice");var so=new SerializedObject(flap);var moving=so.FindProperty("movingParts").GetArrayElementAtIndex(0).FindPropertyRelative("transform").objectReferenceValue as Transform;var mesh=meshes.First(m=>m.name==(side=="L"?"voletG":"voletD"));var b=WorldBounds(mesh);flap.transform.position=b.center;moving.position=new Vector3(b.center.x,b.center.y,b.max.z);mesh.transform.SetParent(moving,true);}
 }
 static void FitGear(GameObject root,GameObject visual,MeshFilter[] meshes){
  string[] names={"gear_F_sprung","gear_L_sprung","gear_R_sprung"},wheels={"roueA","roueG","roueD"};
  for(int i=0;i<3;i++){var gear=C(T(root,names[i]).gameObject,"LandingGear");var so=new SerializedObject(gear);var b=WorldBounds(meshes.First(m=>m.name==wheels[i]));var wheel=so.FindProperty("wheels").GetArrayElementAtIndex(0).objectReferenceValue as Transform;var hinge=Ref(so,"gearHinge");var assembly=i==0?hinge:hinge.parent;assembly.position+=b.center-wheel.position;float radius=b.extents.y;Number(so,"wheelRadius",radius);Number(so,"rollingResistance",.01f);Number(so,"suspensionTravel",.22f);Number(so,"springRate",i==0?90000:150000);Number(so,"dampingRate",i==0?13000:18000);Number(so,"mass",75);var unsprung=Ref(so,"unsprung");unsprung.position=b.center;var bump=Ref(so,"bumpStop");bump.position=b.center+Vector3.up*(.22f-radius);var cast=Ref(so,"castPoint");if(cast)cast.position=bump.position;wheel.position=b.center;so.FindProperty("gearDoors").arraySize=0;so.ApplyModifiedPropertiesWithoutUndo();}
  CreateSourceGearTracks(visual);
 }
 static float Num(XElement e,string n,float fallback=0)=>e?.Element(n)==null?fallback:float.Parse(e.Element(n).Value,CultureInfo.InvariantCulture);
 static Vector3 FG(float x,float y,float z)=>A37VisualBuilder.FG(x,y,z);
 static void CreateSourceGearTracks(GameObject visual){
  var xml=XDocument.Load(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Temp/A37Source/Cessna-A37-Dragonfly/Models/a37.xml")).Root;
  var body=visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="a37"&&t.GetComponentsInChildren<MeshFilter>(true).Length>10);
  var groups=new Dictionary<string,Transform>();
  foreach(var a in xml.Elements("animation").Where(a=>a.Element("type")==null&&a.Element("name")!=null)){
   var name=a.Element("name").Value;if(!name.StartsWith("Train")||groups.ContainsKey(name))continue;var g=new GameObject(name).transform;g.SetParent(body,false);groups[name]=g;
   foreach(var n in a.Elements("object-name")){var t=body.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==n.Value);if(t)t.SetParent(g,true);}
  }
  foreach(var a in xml.Elements("animation").Where(a=>a.Element("type")?.Value=="rotate"&&a.Element("property")?.Value.StartsWith("gear/gear[")==true)){
   int channel=int.Parse(a.Element("property").Value.Substring(10,1));var objects=a.Elements("object-name").Select(e=>e.Value).ToArray();var axis=a.Element("axis");Vector3 center,direction;
   if(axis.Element("x1-m")!=null){center=FG(Num(axis,"x1-m"),Num(axis,"y1-m"),Num(axis,"z1-m"));direction=FG(Num(axis,"x2-m"),Num(axis,"y2-m"),Num(axis,"z2-m"))-center;}else{var c=a.Element("center");center=FG(Num(c,"x-m"),Num(c,"y-m"),Num(c,"z-m"));direction=FG(Num(axis,"x"),Num(axis,"y"),Num(axis,"z"));}
   var entries=a.Element("interpolation").Elements("entry").ToArray();float up=-Num(entries.First(),"dep"),down=-Num(entries.Last(),"dep");
   var pivot=new GameObject("A37GearAxis").transform;pivot.SetParent(visual.transform,false);pivot.localPosition=center;pivot.localRotation=Quaternion.FromToRotation(Vector3.right,direction.normalized);
   var motion=new GameObject("A37GearMotion_"+channel+"_"+up.ToString(CultureInfo.InvariantCulture)+"_"+down.ToString(CultureInfo.InvariantCulture)).transform;motion.SetParent(pivot,false);
   foreach(var n in objects){var target=body.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==n);if(target)target.SetParent(motion,true);}
   motion.localRotation=Quaternion.AngleAxis(down,Vector3.right);
  }
 }
 static void ConfigureEngines(GameObject root){foreach(var engine in All(root,"Turbojet")){var s=new SerializedObject(engine);Number(s,"maxThrust",1293f*9.80665f);Number(s,"maxSpeed",205);s.FindProperty("thrustVectoring").vector3Value=Vector3.zero;s.FindProperty("thrustVectoringGain").vector3Value=Vector3.zero;s.FindProperty("throttleRemap").vector2Value=new Vector2(0,1);Number(s,"fuelConsumptionMin",.025f);Number(s,"fuelConsumptionMax",.36f);s.ApplyModifiedPropertiesWithoutUndo();}
  foreach(var nozzle in All(root,"JetNozzle")){bool left=nozzle.name.EndsWith("_L");var s=new SerializedObject(nozzle);var thrust=new GameObject("A37ThrustPoint").transform;thrust.SetParent(nozzle.transform,false);thrust.position=new Vector3(left?-.60f:.60f,-.26f,-1.8f);thrust.rotation=Quaternion.identity;s.FindProperty("thrustTransform").objectReferenceValue=thrust;s.FindProperty("afterburners").arraySize=0;Number(s,"thrustProportion",1);s.ApplyModifiedPropertiesWithoutUndo();}
 }
 static void ConfigureWeapons(GameObject root,ScriptableObject parameters){
  var wm=new SerializedObject(C(root,"WeaponManager"));var sets=wm.FindProperty("hardpointSets");sets.arraySize=5;int index=0;var gunMount=A37GunBuilder.Build();var internalSet=sets.GetArrayElementAtIndex(0);internalSet.FindPropertyRelative("name").stringValue="Internal 7.62 mm minigun";internalSet.FindPropertyRelative("precludingHardpointSets").arraySize=0;internalSet.FindPropertyRelative("SymmetryWithPrev").boolValue=false;internalSet.FindPropertyRelative("weaponMount").objectReferenceValue=null;var gunOptions=internalSet.FindPropertyRelative("weaponOptions");gunOptions.arraySize=1;gunOptions.GetArrayElementAtIndex(0).objectReferenceValue=gunMount;var gunPoints=internalSet.FindPropertyRelative("hardpoints");gunPoints.arraySize=1;var gunPoint=gunPoints.GetArrayElementAtIndex(0);var nose=T(root,"nose");var gunTransform=new GameObject("A37InternalGunStation").transform;gunTransform.SetParent(nose,true);gunTransform.position=new Vector3(.20f,-.20f,4.12f);gunTransform.rotation=Quaternion.identity;gunPoint.FindPropertyRelative("transform").objectReferenceValue=gunTransform;gunPoint.FindPropertyRelative("part").objectReferenceValue=C(nose.gameObject,"AeroPart");gunPoint.FindPropertyRelative("HardpointIndex").intValue=-1;gunPoint.FindPropertyRelative("bayDoors").arraySize=0;gunPoint.FindPropertyRelative("doorOpenDuration").floatValue=0;gunPoint.FindPropertyRelative("pylonOptions").arraySize=0;foreach(var n in new[]{"Pylon","Plug"})gunPoint.FindPropertyRelative(n).objectReferenceValue=null;foreach(var n in new[]{"BuiltInWeapons","BuiltInTurrets"})gunPoint.FindPropertyRelative(n).arraySize=0;
  for(int s=0;s<4;s++){var set=sets.GetArrayElementAtIndex(s+1);set.FindPropertyRelative("name").stringValue="Underwing pair "+(s+1);set.FindPropertyRelative("precludingHardpointSets").arraySize=0;set.FindPropertyRelative("SymmetryWithPrev").boolValue=false;set.FindPropertyRelative("weaponMount").objectReferenceValue=null;
   var options=set.FindPropertyRelative("weaponOptions");string[] allowed=s<3?new[]{"bomb_125_single","bomb_250_single","RocketPod1_single"}:new[]{"bomb_125_single","RocketPod1_single"};options.arraySize=allowed.Length+1;options.GetArrayElementAtIndex(0).objectReferenceValue=null;for(int k=0;k<allowed.Length;k++)options.GetArrayElementAtIndex(k+1).objectReferenceValue=Native(allowed[k],".asset");
   var points=set.FindPropertyRelative("hardpoints");points.arraySize=2;
   for(int side=0;side<2;side++){var p=points.GetArrayElementAtIndex(side);var part=T(root,(s<2?"wing1_":"wing2_")+(side==0?"L":"R"));var mount=new GameObject("A37Station_"+index).transform;mount.SetParent(part,true);mount.position=new Vector3((side==0?-1:1)*(3.076f+s*.65f)*ModelScale.x,-.75f,(.96f-s*.18f)*ModelScale.z);mount.rotation=Quaternion.identity;p.FindPropertyRelative("transform").objectReferenceValue=mount;p.FindPropertyRelative("part").objectReferenceValue=C(part.gameObject,"AeroPart");p.FindPropertyRelative("HardpointIndex").intValue=index++;p.FindPropertyRelative("bayDoors").arraySize=0;p.FindPropertyRelative("doorOpenDuration").floatValue=0;p.FindPropertyRelative("pylonOptions").arraySize=0;foreach(var n in new[]{"Pylon","Plug"})p.FindPropertyRelative(n).objectReferenceValue=null;foreach(var n in new[]{"BuiltInWeapons","BuiltInTurrets"})p.FindPropertyRelative(n).arraySize=0;}
  }wm.ApplyModifiedPropertiesWithoutUndo();
  var parameterObject=new SerializedObject(parameters);var defaults=parameterObject.FindProperty("loadouts");defaults.arraySize=2;for(int faction=0;faction<2;faction++){var weapons=defaults.GetArrayElementAtIndex(faction).FindPropertyRelative("weapons");weapons.arraySize=5;weapons.GetArrayElementAtIndex(0).objectReferenceValue=gunMount;for(int station=1;station<5;station++)weapons.GetArrayElementAtIndex(station).objectReferenceValue=null;}parameterObject.ApplyModifiedPropertiesWithoutUndo();
 }
 static void ConfigureLivery(ScriptableObject parameters,ScriptableObject nativeParams){
  string path=R+"/A37Camouflage.asset";var asset=AssetDatabase.LoadMainAssetAtPath(path);if(!asset){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LiveryData")).First(t=>t!=null);asset=ScriptableObject.CreateInstance(type);AssetDatabase.CreateAsset(asset,path);}var s=new SerializedObject(asset);s.FindProperty("Texture").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Texture2D>(R+"/Models/texture.png");Number(s,"Glossiness",.2f);s.FindProperty("Colors").arraySize=0;s.ApplyModifiedPropertiesWithoutUndo();
  var native=new SerializedObject(nativeParams);var p=new SerializedObject(parameters);var liveries=p.FindProperty("liveries");liveries.arraySize=2;for(int i=0;i<2;i++){var e=liveries.GetArrayElementAtIndex(i);e.FindPropertyRelative("name").stringValue="FAU 282 - woodland camouflage";e.FindPropertyRelative("faction").objectReferenceValue=native.FindProperty("liveries").GetArrayElementAtIndex(i).FindPropertyRelative("faction").objectReferenceValue;e.FindPropertyRelative("assetReference").FindPropertyRelative("m_AssetGUID").stringValue=AssetDatabase.AssetPathToGUID(path);}p.ApplyModifiedPropertiesWithoutUndo();
 }
 static void CreateIcon(GameObject visual,ScriptableObject definition){var model=Object.Instantiate(visual);model.transform.position=Vector3.up*1000;var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));mat.SetColor("_BaseColor",Color.white);foreach(var r in model.GetComponentsInChildren<MeshRenderer>(true))r.sharedMaterials=Enumerable.Repeat(mat,r.sharedMaterials.Length).ToArray();var camera=new GameObject("A37IconCamera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;camera.orthographicSize=6;camera.transform.position=new Vector3(0,1030,0);camera.transform.rotation=Quaternion.Euler(90,0,0);camera.farClipPlane=50;var rt=new RenderTexture(256,256,24);var tex=new Texture2D(256,256,TextureFormat.RGBA32,false);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,256,256),0,0);tex.Apply();File.WriteAllBytes(R+"/A37Icon.png",tex.EncodeToPNG());RenderTexture.active=old;Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(model);Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);Object.DestroyImmediate(mat);AssetDatabase.ImportAsset(R+"/A37Icon.png");var importer=(TextureImporter)AssetImporter.GetAtPath(R+"/A37Icon.png");importer.textureType=TextureImporterType.Sprite;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.SaveAndReimport();var s=new SerializedObject(definition);s.FindProperty("mapIcon").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(R+"/A37Icon.png");s.ApplyModifiedPropertiesWithoutUndo();}
 static void AddHangars(Object donor){OpReferenceIndex.Refresh();var op=ScriptableObject.CreateInstance<OpAddAircraftToHangars>();op.aircraftJsonKey=Key;foreach(var guid in AssetDatabase.FindAssets("t:BuildingDefinition",new[]{"Assets/Blueprinter/_donotship"})){var d=AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));var s=new SerializedObject(d);var prefab=s.FindProperty("unitPrefab").objectReferenceValue as GameObject;if(!prefab)continue;var names=new List<string>();foreach(var h in All(prefab,"Hangar")){var list=new SerializedObject(h).FindProperty("availableAircraft");if(Enumerable.Range(0,list.arraySize).Any(i=>list.GetArrayElementAtIndex(i).objectReferenceValue==donor))names.Add(h.name.Replace("_PLACEHOLDER",""));}if(names.Count>0)op.hangars.Add(new OpAddAircraftToHangars.HangarTarget{hangarUnitJsonKey=s.FindProperty("jsonKey").stringValue,hangarNames=names});}if(op.hangars.Count==0)throw new Exception("No native trainer hangars");AssetDatabase.CreateAsset(op,R+"/A37Hangars.asset");}
 public static void Validate(){var root=AssetDatabase.LoadAssetAtPath<GameObject>(R+"/A37.prefab");var d=new SerializedObject(AssetDatabase.LoadMainAssetAtPath(R+"/A37Definition.asset"));var p=new SerializedObject(AssetDatabase.LoadMainAssetAtPath(R+"/A37Parameters.asset"));if(d.FindProperty("value").floatValue!=13||p.FindProperty("rankRequired").intValue!=1)throw new Exception("Catalog mismatch");if(root.GetComponentsInChildren<Component>(true).Any(c=>!c))throw new Exception("Missing native component");if(All(root,"Turbojet").Count()!=2||All(root,"LandingGear").Count()!=3)throw new Exception("Missing engines/gear");foreach(var n in All(root,"JetNozzle"))if(new SerializedObject(n).FindProperty("afterburners").arraySize!=0)throw new Exception("A37 has no afterburner");File.WriteAllText("../../work/a37/build-validation.txt","Tier 1 / 13 million; 2 dry engines, 3 native gear assemblies, own icon.\nIN-GAME FLIGHT: NOT TESTED\n");}
}









